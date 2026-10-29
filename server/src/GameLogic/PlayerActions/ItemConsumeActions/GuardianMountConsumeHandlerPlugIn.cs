// <copyright file="GuardianMountConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// OriginSix: using (right clicking) a Guardian mount in the inventory summons or dismisses it; using
/// the summoned Guardian on itself (the client's mount key) mounts or dismounts it.
/// The Guardian is not consumed and stays in its slot; see <see cref="GuardianMounts"/>.
/// </summary>
public abstract class GuardianMountConsumeHandlerPlugIn : IItemConsumeHandlerPlugIn
{
    /// <inheritdoc />
    public ItemIdentifier Key => new(this.Number, GuardianMounts.Group);

    /// <summary>
    /// Gets the item number of the Guardian.
    /// </summary>
    protected abstract short Number { get; }

    /// <inheritdoc />
    public async ValueTask<bool> ConsumeItemAsync(Player player, Item item, Item? targetItem, FruitUsage fruitUsage)
    {
        if (!item.IsGuardianMount() || item.ItemSlot <= InventoryConstants.LastEquippableItemSlotIndex || player.PlayerState.CurrentState != PlayerState.EnteredWorld)
        {
            return false;
        }

        if (targetItem == item)
        {
            // Used on itself (the client's mount key): mount or dismount the active Guardian.
            await GuardianMounts.ToggleRidingAsync(player, item).ConfigureAwait(false);
        }
        else
        {
            await GuardianMounts.ToggleAsync(player, item).ConfigureAwait(false);
        }

        return true;
    }
}

/// <summary>Using the Ghost Horse.</summary>
[Guid("B6E1D0A2-41C7-4E0B-9C3F-1A7D2E8F4C01")]
[PlugIn]
[Display(Name = "Guardian mount: Ghost Horse", Description = "OriginSix: using the Ghost Horse activates or deactivates it as the Guardian mount.")]
public sealed class GhostHorseConsumeHandlerPlugIn : GuardianMountConsumeHandlerPlugIn
{
    /// <inheritdoc />
    protected override short Number => 146;
}

/// <summary>Using the Ice Dragon.</summary>
[Guid("B6E1D0A2-41C7-4E0B-9C3F-1A7D2E8F4C02")]
[PlugIn]
[Display(Name = "Guardian mount: Ice Dragon", Description = "OriginSix: using the Ice Dragon activates or deactivates it as the Guardian mount.")]
public sealed class IceDragonConsumeHandlerPlugIn : GuardianMountConsumeHandlerPlugIn
{
    /// <inheritdoc />
    protected override short Number => 147;
}

/// <summary>Using the Ice Dragon (Rare).</summary>
[Guid("B6E1D0A2-41C7-4E0B-9C3F-1A7D2E8F4C03")]
[PlugIn]
[Display(Name = "Guardian mount: Ice Dragon (Rare)", Description = "OriginSix: using the Ice Dragon (Rare) activates or deactivates it as the Guardian mount.")]
public sealed class IceDragonRareConsumeHandlerPlugIn : GuardianMountConsumeHandlerPlugIn
{
    /// <inheritdoc />
    protected override short Number => 148;
}

/// <summary>Using the Fierce Lion.</summary>
[Guid("B6E1D0A2-41C7-4E0B-9C3F-1A7D2E8F4C04")]
[PlugIn]
[Display(Name = "Guardian mount: Fierce Lion", Description = "OriginSix: using the Fierce Lion activates or deactivates it as the Guardian mount.")]
public sealed class FierceLionConsumeHandlerPlugIn : GuardianMountConsumeHandlerPlugIn
{
    /// <inheritdoc />
    protected override short Number => 149;
}

/// <summary>Using the Fierce Lion (Rare).</summary>
[Guid("B6E1D0A2-41C7-4E0B-9C3F-1A7D2E8F4C05")]
[PlugIn]
[Display(Name = "Guardian mount: Fierce Lion (Rare)", Description = "OriginSix: using the Fierce Lion (Rare) activates or deactivates it as the Guardian mount.")]
public sealed class FierceLionRareConsumeHandlerPlugIn : GuardianMountConsumeHandlerPlugIn
{
    /// <inheritdoc />
    protected override short Number => 150;
}

/// <summary>Using the Shining Tail.</summary>
[Guid("B6E1D0A2-41C7-4E0B-9C3F-1A7D2E8F4C06")]
[PlugIn]
[Display(Name = "Guardian mount: Shining Tail", Description = "OriginSix: using the Shining Tail activates or deactivates it as the Guardian mount.")]
public sealed class ShiningTailConsumeHandlerPlugIn : GuardianMountConsumeHandlerPlugIn
{
    /// <inheritdoc />
    protected override short Number => 151;
}

/// <summary>Using the Shining Tail (Rare).</summary>
[Guid("B6E1D0A2-41C7-4E0B-9C3F-1A7D2E8F4C07")]
[PlugIn]
[Display(Name = "Guardian mount: Shining Tail (Rare)", Description = "OriginSix: using the Shining Tail (Rare) activates or deactivates it as the Guardian mount.")]
public sealed class ShiningTailRareConsumeHandlerPlugIn : GuardianMountConsumeHandlerPlugIn
{
    /// <inheritdoc />
    protected override short Number => 152;
}

/// <summary>Using the Ullr.</summary>
[Guid("B6E1D0A2-41C7-4E0B-9C3F-1A7D2E8F4C08")]
[PlugIn]
[Display(Name = "Guardian mount: Ullr", Description = "OriginSix: using the Ullr activates or deactivates it as the Guardian mount.")]
public sealed class UllrConsumeHandlerPlugIn : GuardianMountConsumeHandlerPlugIn
{
    /// <inheritdoc />
    protected override short Number => 153;
}

/// <summary>Using the Ullr (Rare).</summary>
[Guid("B6E1D0A2-41C7-4E0B-9C3F-1A7D2E8F4C09")]
[PlugIn]
[Display(Name = "Guardian mount: Ullr (Rare)", Description = "OriginSix: using the Ullr (Rare) activates or deactivates it as the Guardian mount.")]
public sealed class UllrRareConsumeHandlerPlugIn : GuardianMountConsumeHandlerPlugIn
{
    /// <inheritdoc />
    protected override short Number => 154;
}
