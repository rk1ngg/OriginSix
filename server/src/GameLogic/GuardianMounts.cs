// <copyright file="GuardianMounts.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic;

using System.Runtime.CompilerServices;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Views.Inventory;
using MUnique.OpenMU.GameLogic.Views.World;

/// <summary>
/// OriginSix Guardian mounts (Ghost Horse to Ullr): mounts which are equipped in the inventory.
/// </summary>
/// <remarks>
/// A Guardian stays in its bag slot. Using it (right click) makes it the active Guardian, which walks
/// beside its owner, and using it again deactivates it; at most one Guardian is active. Using the
/// active Guardian on itself (the client's mount key) changes the ride: riding the Guardian, walking, riding
/// the equipped mount pet (e.g. a Fenrir) if there is one while the Guardian walks beside, and walking again. The active state is the item's skill flag,
/// which Guardians don't use otherwise, so it's persisted with the item and the client sees it.
/// The active Guardian appears to other players in place of the pet (slot 8) and adds its own
/// power-ups, while a pet in the pet slot keeps working.
/// </remarks>
public static class GuardianMounts
{
    /// <summary>
    /// The item group of the Guardians.
    /// </summary>
    public const byte Group = 13;

    /// <summary>
    /// The item number of the first Guardian (Ghost Horse).
    /// </summary>
    public const short FirstNumber = 146;

    /// <summary>
    /// The item number of the last Guardian (Ullr Rare).
    /// </summary>
    public const short LastNumber = 154;

    /// <summary>
    /// The mount pets which can be ridden instead of the Guardian: Uniria, Dinorant, Dark Horse and Fenrir.
    /// </summary>
    private static readonly short[] MountPetNumbers = [2, 3, 4, 37];

    /// <summary>
    /// What the players with an active Guardian ride. Riding is not persisted: after entering the game,
    /// an active Guardian walks beside its owner.
    /// </summary>
    private static readonly ConditionalWeakTable<Player, StrongBox<GuardianRide>> Rides = new();

    /// <summary>
    /// What a player with an active Guardian rides.
    /// </summary>
    public enum GuardianRide
    {
        /// <summary>
        /// Nothing: the Guardian walks beside its owner.
        /// </summary>
        None,

        /// <summary>
        /// The Guardian.
        /// </summary>
        Guardian,

        /// <summary>
        /// Nothing after riding the Guardian: the Guardian walks beside its owner, and the mount key
        /// rides the equipped mount pet next.
        /// </summary>
        Dismounted,

        /// <summary>
        /// The equipped mount pet, e.g. a Fenrir; the Guardian isn't shown to other players.
        /// </summary>
        MountPet,
    }

    /// <summary>
    /// Determines whether the item is a Guardian.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns><c>true</c>, if the item is a Guardian.</returns>
    public static bool IsGuardianMount(this Item item) => item.Definition is { Group: Group, Number: >= FirstNumber and <= LastNumber };

    /// <summary>
    /// Gets the active Guardian of the player, which is a Guardian in the bag with the active flag.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The active Guardian, or <c>null</c>.</returns>
    public static Item? GetActiveGuardian(this Player player)
        => player.Inventory?.Items.FirstOrDefault(item => item.IsGuardianMount() && item.HasSkill && item.ItemSlot > InventoryConstants.LastEquippableItemSlotIndex);

    /// <summary>
    /// Determines whether the player rides the active Guardian; otherwise it walks beside the player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns><c>true</c>, if the player rides the active Guardian.</returns>
    public static bool IsRidingGuardian(this Player player) => player.GetGuardianRide() == GuardianRide.Guardian;

    /// <summary>
    /// Gets what the player with an active Guardian rides.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>What the player rides.</returns>
    public static GuardianRide GetGuardianRide(this Player player) => Rides.TryGetValue(player, out var ride) ? ride.Value : GuardianRide.None;

    /// <summary>
    /// Creates a copy of the Guardian in the pet slot, which is how it appears and adds its power-ups.
    /// </summary>
    /// <param name="guardian">The guardian.</param>
    /// <returns>The copy.</returns>
    public static Item AsPetSlotItem(this Item guardian)
    {
        var copy = new TemporaryItem();
        copy.AssignValues(guardian);
        copy.ItemSlot = InventoryConstants.PetSlot;
        return copy;
    }

    /// <summary>
    /// Creates the copy of the Guardian which other players see: while it's ridden, it carries the
    /// first Fenrir variant flag, which the OriginSix client reads as "ridden" for a Guardian.
    /// </summary>
    /// <param name="guardian">The guardian.</param>
    /// <param name="player">The owner of the guardian.</param>
    /// <returns>The copy.</returns>
    public static Item AsAppearanceItem(this Item guardian, Player player)
    {
        var copy = guardian.AsPetSlotItem();
        if (player.IsRidingGuardian()
            && player.GameContext.Configuration.ItemOptions.SelectMany(o => o.PossibleOptions).FirstOrDefault(o => o.OptionType == ItemOptionTypes.BlackFenrir) is { } ridingFlag)
        {
            copy.ItemOptions.Add(new ItemOptionLink { ItemOption = ridingFlag });
        }

        return copy;
    }

    /// <summary>
    /// Changes the ride of the active Guardian's owner: from walking to riding the Guardian, to walking, to riding the
    /// equipped mount pet if there is one (otherwise the Guardian again), and to walking again.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="guardian">The guardian in the bag, which must be the active one.</param>
    /// <returns>The completion.</returns>
    public static async ValueTask ToggleRidingAsync(Player player, Item guardian)
    {
        if (player.GetActiveGuardian() != guardian)
        {
            return;
        }

        var mountPet = player.GetEquippedMountPet();
        var next = player.GetGuardianRide() switch
        {
            GuardianRide.None => GuardianRide.Guardian,
            GuardianRide.Guardian => GuardianRide.Dismounted,
            GuardianRide.Dismounted when mountPet is not null => GuardianRide.MountPet,
            GuardianRide.Dismounted => GuardianRide.Guardian,
            _ => GuardianRide.None,
        };

        Rides.AddOrUpdate(player, new StrongBox<GuardianRide>(next));
        if (next == GuardianRide.MountPet)
        {
            await ShowAsync(player, mountPet!, true).ConfigureAwait(false);
        }
        else
        {
            await ShowAsync(player, guardian.AsAppearanceItem(player), true).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gets the mount pet in the pet slot (Uniria, Dinorant, Dark Horse or Fenrir), which can be ridden instead of the Guardian.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The mount pet, or <c>null</c>.</returns>
    public static Item? GetEquippedMountPet(this Player player)
        => player.Inventory?.GetItem(InventoryConstants.PetSlot) is { Definition: { Group: Group } definition } pet
           && MountPetNumbers.Contains(definition.Number)
            ? pet
            : null;

    /// <summary>
    /// Activates the Guardian, or deactivates it if it's already active. Another active Guardian is deactivated.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="guardian">The guardian in the bag.</param>
    /// <returns>The completion.</returns>
    public static async ValueTask ToggleAsync(Player player, Item guardian)
    {
        var previous = player.GetActiveGuardian();
        if (previous != guardian && guardian.Durability <= 0)
        {
            // Without life, a Guardian can't be summoned until it's repaired.
            await player.ShowBlueMessageAsync("The Guardian has no life left. Repair it to summon it.").ConfigureAwait(false);
            return;
        }

        if (previous is not null)
        {
            await SetActiveAsync(player, previous, false).ConfigureAwait(false);
        }

        if (previous != guardian)
        {
            await SetActiveAsync(player, guardian, true).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Decreases the life of the active Guardian when its owner is hit, like the life of a Fenrir, but it lasts
    /// about three times longer. Without life, the Guardian is put away; it stays in the inventory to be repaired.
    /// </summary>
    /// <param name="player">The player who was hit.</param>
    /// <param name="damage">The damage.</param>
    /// <returns>The completion.</returns>
    public static async ValueTask DecreaseLifeAsync(Player player, float damage)
    {
        if (player.GetActiveGuardian() is not { Durability: > 0 } guardian || player.Attributes is not { } attributes)
        {
            return;
        }

        // The Fenrir formula (damage factor 2, divided by 200) with damage factor 1, divided by 300.
        var durationIncrease = attributes[Stats.PetDurationIncrease];
        if (durationIncrease == 0)
        {
            durationIncrease = 1;
        }

        var decrement = 1 + (damage / (player.GameContext.Configuration.DamagePerOnePetDurability * durationIncrease));
        decrement /= 300;
        await player.DecreaseItemDurabilityAsync(guardian, decrement).ConfigureAwait(false);
        if (guardian.Durability <= 0)
        {
            await SetActiveAsync(player, guardian, false).ConfigureAwait(false);
            await player.ShowBlueMessageAsync("The Guardian has no life left and returned to the inventory.").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Adds the power-ups of the active Guardian, e.g. after the inventory power-ups were initialized.
    /// </summary>
    /// <param name="player">The player.</param>
    public static void AddActivePowerUps(Player player)
    {
        if (player.GetActiveGuardian() is { } guardian)
        {
            AddPowerUps(player, guardian);
        }
    }

    private static async ValueTask SetActiveAsync(Player player, Item guardian, bool active)
    {
        guardian.HasSkill = active;
        Rides.Remove(player);
        if (active)
        {
            AddPowerUps(player, guardian);
        }
        else
        {
            RemovePowerUps(player, guardian);
        }

        await player.InvokeViewPlugInAsync<IItemUpgradedPlugIn>(p => p.ItemUpgradedAsync(guardian)).ConfigureAwait(false);

        var shown = active ? guardian.AsAppearanceItem(player) : player.Inventory?.GetItem(InventoryConstants.PetSlot) ?? guardian.AsPetSlotItem();
        var shownEquipped = active || (shown.ItemSlot == InventoryConstants.PetSlot && !shown.IsGuardianMount());
        await ShowAsync(player, shown, shownEquipped).ConfigureAwait(false);
    }

    private static async ValueTask ShowAsync(Player player, Item shown, bool isEquipped)
    {
        player.OnAppearanceChanged();
        await player.ForEachWorldObserverAsync<IAppearanceChangedPlugIn>(
            p => p.AppearanceChangedAsync(player, shown, isEquipped),
            true).ConfigureAwait(false);
    }

    private static void AddPowerUps(Player player, Item guardian)
    {
        if (player.Attributes is not { } attributes)
        {
            return;
        }

        RemovePowerUps(player, guardian);
        var powerUps = player.GameContext.ItemPowerUpFactory.GetPowerUps(guardian.AsPetSlotItem(), attributes).ToList();
        attributes.ItemPowerUps.Add(guardian, powerUps);
    }

    private static void RemovePowerUps(Player player, Item guardian)
    {
        if (player.Attributes?.ItemPowerUps.Remove(guardian, out var powerUps) is true)
        {
            foreach (var powerUp in powerUps)
            {
                powerUp.Dispose();
            }
        }
    }
}
