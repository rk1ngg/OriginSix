// <copyright file="AddGuardianMountsPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// OriginSix custom content: the Guardian mounts (Ghost Horse to Ullr, with their Rare variants),
/// helper items 13/146 to 13/154 which are equipped in the inventory (see <see cref="GuardianMounts"/>).
/// </summary>
/// <remarks>
/// The official names follow docs/GUARDIAN_RUUD_UI_PLAN.md. Like the official mount type Guardians,
/// each one adds attack speed +7, besides health and armor, while it's active. They have no equipment slot and don't drop.
/// The Guardians are added to existing databases only; a new database doesn't get this custom content.
/// Version 2 makes them take one inventory slot, like the official Guardians.
/// Version 4 gives them an item slot type without slots: like a pet, a Guardian has life (its durability, 255), and an
/// item without slot type and with a durability above 1 would be stackable. Without slots, it's not equipped in any.
/// Version 5 makes them repairable: like a pet, a Guardian loses life when its owner is hit.
/// Version 6 sets the owner's table of 2026-10-29 (docs/GUARDIAN_RUUD_UI_PLAN.md): more health and armor per tier,
/// and the Ullr (Rare) with health +777, armor +555 and attack speed +28.
/// Version 7 adds attack +28 and wizardry +28 to the Ullr (Rare), like the Gold Fenrir's attack and wizardry options.
/// Version 8 adds the health after the percentage bonuses, like the Gold Fenrir, so a Guardian adds exactly its value.
/// </remarks>
[PlugIn]
[Display(Name = PlugInName, Description = PlugInDescription)]
[Guid("7C3E5A91-2B4D-4F68-A0E7-9D51C3B82F46")]
public class AddGuardianMountsPlugIn : UpdatePlugInBase
{
    /// <summary>
    /// The plug in name.
    /// </summary>
    internal const string PlugInName = "Add OriginSix Guardian mounts";

    /// <summary>
    /// The plug in description.
    /// </summary>
    internal const string PlugInDescription = "OriginSix custom content: adds the Guardian mounts 13/146 to 13/154 (Ghost Horse, Ice Dragon, Fierce Lion, Shining Tail and Ullr, with their Rare variants), which are equipped by using them in the inventory and add attack speed +7.";

    private const byte GuardianWidth = 1;

    private const byte GuardianHeight = 1;

    private const byte GuardianDurability = 255;

    private const short GuardianSlotTypeNumber = 146;

    private const byte GuardianMaximumLevel = 15;


    private static readonly string[] Names =
    [
        "Ghost Horse",
        "Ice Dragon",
        "Ice Dragon (Rare)",
        "Fierce Lion",
        "Fierce Lion (Rare)",
        "Shining Tail",
        "Shining Tail (Rare)",
        "Ullr",
        "Ullr (Rare)",
    ];

    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => PlugInName;

    /// <inheritdoc />
    public override string Description => PlugInDescription;

    /// <inheritdoc />
    public override bool IsMandatory => true;

    /// <summary>
    /// The maximum health, defense ("Armor"), attack speed, attack and wizardry of each Guardian, from the Ghost Horse to the Ullr (Rare),
    /// as the owner set them on 2026-10-29. They are above the Gold Fenrir (+200 health) and rise with each tier; the
    /// Ullr (Rare) has the lucky values 777, 555 and 28. There are no elemental options in S6.
    /// </summary>
    private static readonly (float Health, float Armor, float AttackSpeed, float Attack, float Wizardry)[] Bonuses =
    [
        (255, 150, 7, 0, 0),
        (305, 180, 7, 0, 0),
        (330, 195, 7, 0, 0),
        (355, 210, 7, 0, 0),
        (380, 225, 7, 0, 0),
        (405, 240, 7, 0, 0),
        (430, 255, 7, 0, 0),
        (505, 275, 7, 0, 0),
        (777, 555, 28, 28, 28),
    ];

    /// <inheritdoc />
    public override int Version => 8;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 10, 29, 4, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override DateTime UpdatedAt => new(2026, 10, 29, 16, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        var guardianSlotType = GetOrCreateGuardianSlotType(context, gameConfiguration);
        for (var number = GuardianMounts.FirstNumber; number <= GuardianMounts.LastNumber; number++)
        {
            var index = number - GuardianMounts.FirstNumber;
            if (gameConfiguration.Items.FirstOrDefault(item => item.Group == GuardianMounts.Group && item.Number == number) is not { } guardian)
            {
                guardian = context.CreateNew<ItemDefinition>();
                guardian.Group = GuardianMounts.Group;
                guardian.Number = number;
                guardian.Name = Names[index];
                guardian.Durability = GuardianDurability;
                guardian.MaximumItemLevel = GuardianMaximumLevel;
                guardian.DropsFromMonsters = false;
                guardian.SetGuid(guardian.Group, guardian.Number);
                gameConfiguration.Items.Add(guardian);
            }

            // Version 2: a Guardian takes one inventory slot. Version 3: health and armor besides attack speed.
            // Version 4: not stackable. Version 5: repairable, to restore its life.
            guardian.IsRepairable = true;
            guardian.Durability = GuardianDurability;
            guardian.ItemSlot = guardianSlotType;
            guardian.Width = GuardianWidth;
            guardian.Height = GuardianHeight;
            guardian.BasePowerUpAttributes.Clear();
            guardian.BasePowerUpAttributes.Add(CreatePowerUp(context, gameConfiguration, Stats.AttackSpeedAny, Bonuses[index].AttackSpeed));
            guardian.BasePowerUpAttributes.Add(CreatePowerUp(context, gameConfiguration, Stats.MaximumHealth, Bonuses[index].Health, AggregateType.AddFinal));
            guardian.BasePowerUpAttributes.Add(CreatePowerUp(context, gameConfiguration, Stats.DefenseBase, Bonuses[index].Armor));

            // Version 7: attack and wizardry, added like the Gold Fenrir's options.
            if (Bonuses[index].Attack > 0)
            {
                guardian.BasePowerUpAttributes.Add(CreatePowerUp(context, gameConfiguration, Stats.PhysicalBaseDmg, Bonuses[index].Attack, AggregateType.AddFinal));
            }

            if (Bonuses[index].Wizardry > 0)
            {
                guardian.BasePowerUpAttributes.Add(CreatePowerUp(context, gameConfiguration, Stats.WizardryBaseDmg, Bonuses[index].Wizardry));
            }
        }

        return ValueTask.CompletedTask;
    }

    private static ItemSlotType GetOrCreateGuardianSlotType(IContext context, GameConfiguration gameConfiguration)
    {
        var slotTypeId = GuidHelper.CreateGuid<ItemSlotType>(GuardianSlotTypeNumber);
        if (gameConfiguration.ItemSlotTypes.FirstOrDefault(slotType => slotType.GetId() == slotTypeId) is { } existing)
        {
            return existing;
        }

        var guardianSlotType = context.CreateNew<ItemSlotType>();
        guardianSlotType.SetGuid(GuardianSlotTypeNumber);
        guardianSlotType.Description = "Guardian";
        gameConfiguration.ItemSlotTypes.Add(guardianSlotType);
        return guardianSlotType;
    }

    private static ItemBasePowerUpDefinition CreatePowerUp(IContext context, GameConfiguration gameConfiguration, AttributeDefinition attribute, float value, AggregateType aggregateType = AggregateType.AddRaw)
    {
        var powerUp = context.CreateNew<ItemBasePowerUpDefinition>();
        powerUp.TargetAttribute = attribute.GetPersistent(gameConfiguration);
        powerUp.BaseValue = value;
        powerUp.AggregateType = aggregateType;
        return powerUp;
    }
}
