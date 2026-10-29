// <copyright file="AddLuckyItemMerchantsPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Items;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// OriginSix custom content: seven merchants outside the Lorencia bar, one per character class,
/// which sell the lucky items of that class for 1 zen each: +15, with luck, the maximum
/// additional option and the complete lucky set option.
/// </summary>
/// <remarks>
/// The merchant numbers 690 to 696 are unused by the season 6 client and server; the OriginSix
/// client shows them as a character of their class. The price is a fixed store price of the
/// store items, so the lucky items keep their calculated price everywhere else. The merchants are
/// added to existing databases only; a new database doesn't get this custom content.
/// Version 2 upgrades the store items and turns the merchants towards the bar door (west).
/// </remarks>
[PlugIn]
[Display(Name = PlugInName, Description = PlugInDescription)]
[Guid("5E2B9D17-6A4C-4F80-B3E2-91C7D04A6F38")]
public class AddLuckyItemMerchantsPlugIn : UpdatePlugInBase
{
    /// <summary>
    /// The plug in name.
    /// </summary>
    internal const string PlugInName = "Add OriginSix lucky item merchants";

    /// <summary>
    /// The plug in description.
    /// </summary>
    internal const string PlugInDescription = "OriginSix custom content: adds seven merchants (690 to 696) outside the Lorencia bar, one per character class, which sell the lucky items of that class for 1 zen each (+15, luck, maximum additional option, lucky set option).";

    /// <summary>
    /// The first merchant number.
    /// </summary>
    internal const short FirstMerchantNumber = 690;

    /// <summary>
    /// The last merchant number.
    /// </summary>
    internal const short LastMerchantNumber = 696;

    private const byte MerchantY = 130;

    private const int StoreWidth = 8;

    private const int StoreHeight = 15;

    private const int StorePrice = 1;

    private const byte StoreItemLevel = 15;

    private const int LuckySetDiscriminator = 1;

    private static readonly (short Number, string Name, CharacterClassNumber CharacterClass, byte X)[] Merchants =
    [
        (690, "Lucky Merchant (Dark Wizard)", CharacterClassNumber.DarkWizard, 132),
        (691, "Lucky Merchant (Dark Knight)", CharacterClassNumber.DarkKnight, 134),
        (692, "Lucky Merchant (Fairy Elf)", CharacterClassNumber.FairyElf, 136),
        (693, "Lucky Merchant (Magic Gladiator)", CharacterClassNumber.MagicGladiator, 138),
        (694, "Lucky Merchant (Dark Lord)", CharacterClassNumber.DarkLord, 140),
        (695, "Lucky Merchant (Summoner)", CharacterClassNumber.Summoner, 142),
        (696, "Lucky Merchant (Rage Fighter)", CharacterClassNumber.RageFighter, 144),
    ];

    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => PlugInName;

    /// <inheritdoc />
    public override string Description => PlugInDescription;

    /// <inheritdoc />
    public override bool IsMandatory => true;

    /// <inheritdoc />
    public override int Version => 2;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 10, 29, 1, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override DateTime UpdatedAt => new(2026, 10, 29, 3, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddLuckyItemsUpdatePlugIn).GUID, typeof(AddLuckyItemSetsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        var luckyItems = gameConfiguration.Items
            .Where(item => item.Group is >= 7 and <= 11 && item.Number is >= Armors.LuckyItemNumberStart and <= Armors.LuckyItemNumberEnd)
            .OrderBy(item => item.Number)
            .ThenBy(item => item.Group)
            .ToList();

        var lorencia = gameConfiguration.Maps.First(map => map.Number == 0);
        foreach (var (number, name, characterClassNumber, x) in Merchants)
        {
            if (gameConfiguration.Monsters.FirstOrDefault(monster => monster.Number == number) is not { } merchant)
            {
                var characterClass = gameConfiguration.CharacterClasses.First(c => c.Number == (byte)characterClassNumber);
                merchant = context.CreateNew<MonsterDefinition>();
                merchant.Number = number;
                merchant.Designation = name;
                merchant.NpcWindow = NpcWindow.Merchant;
                merchant.ObjectKind = NpcObjectKind.PassiveNpc;
                merchant.MerchantStore = CreateStore(context, luckyItems.Where(item => item.QualifiedCharacters.Contains(characterClass)));
                merchant.SetGuid(merchant.Number);
                gameConfiguration.Monsters.Add(merchant);

                var spawn = context.CreateNew<MonsterSpawnArea>();
                lorencia.MonsterSpawns.Add(spawn);
                spawn.GameMap = lorencia;
                spawn.MonsterDefinition = merchant;
                spawn.Quantity = 1;
                spawn.SpawnTrigger = SpawnTrigger.Automatic;
                spawn.X1 = x;
                spawn.X2 = x;
                spawn.Y1 = MerchantY;
                spawn.Y2 = MerchantY;
            }

            foreach (var spawn in lorencia.MonsterSpawns.Where(spawn => spawn.MonsterDefinition == merchant))
            {
                spawn.Direction = Direction.West;
            }

            foreach (var item in merchant.MerchantStore?.Items ?? [])
            {
                UpgradeStoreItem(context, item);
            }
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Makes a store item +15 with luck, the maximum additional option and the lucky set option.
    /// </summary>
    private static void UpgradeStoreItem(IContext context, Item item)
    {
        if (item.Definition is not { } definition)
        {
            return;
        }

        item.Level = Math.Min(StoreItemLevel, definition.MaximumItemLevel);
        item.StorePrice = StorePrice;
        var possibleOptions = definition.PossibleItemOptions.SelectMany(o => o.PossibleOptions).ToList();
        if (!item.ItemOptions.Any(link => link.ItemOption?.OptionType == ItemOptionTypes.Luck)
            && possibleOptions.FirstOrDefault(o => o.OptionType == ItemOptionTypes.Luck) is { } luck)
        {
            var luckLink = context.CreateNew<ItemOptionLink>();
            luckLink.ItemOption = luck;
            item.ItemOptions.Add(luckLink);
        }

        if (possibleOptions.FirstOrDefault(o => o.OptionType == ItemOptionTypes.Option) is { } option)
        {
            var maximumLevel = option.LevelDependentOptions.Select(l => l.Level).DefaultIfEmpty(1).Max();
            if (item.ItemOptions.FirstOrDefault(link => link.ItemOption?.OptionType == ItemOptionTypes.Option) is not { } optionLink)
            {
                optionLink = context.CreateNew<ItemOptionLink>();
                optionLink.ItemOption = option;
                item.ItemOptions.Add(optionLink);
            }

            optionLink.Level = maximumLevel;
        }

        if (!item.ItemSetGroups.Any(set => set.AncientSetDiscriminator != 0)
            && definition.PossibleItemSetGroups.SelectMany(set => set.Items)
                .FirstOrDefault(i => i.ItemDefinition == definition && i.AncientSetDiscriminator == LuckySetDiscriminator) is { } itemOfSet)
        {
            item.ItemSetGroups.Add(itemOfSet);
        }
    }

    /// <summary>
    /// Creates a merchant store with the items placed row by row in the 8x15 store grid.
    /// </summary>
    private static ItemStorage CreateStore(IContext context, IEnumerable<ItemDefinition> definitions)
    {
        var store = context.CreateNew<ItemStorage>();
        var occupied = new bool[StoreWidth, StoreHeight];
        foreach (var definition in definitions)
        {
            if (FindFreeSlot(occupied, definition.Width, definition.Height) is not var (x, y))
            {
                break;
            }

            for (var dx = 0; dx < definition.Width; dx++)
            {
                for (var dy = 0; dy < definition.Height; dy++)
                {
                    occupied[x + dx, y + dy] = true;
                }
            }

            var item = context.CreateNew<Item>();
            item.Definition = definition;
            item.Durability = definition.Durability;
            item.ItemSlot = (byte)((y * StoreWidth) + x);
            item.StorePrice = StorePrice;
            store.Items.Add(item);
        }

        return store;
    }

    private static (int X, int Y)? FindFreeSlot(bool[,] occupied, int width, int height)
    {
        for (var y = 0; y + height <= StoreHeight; y++)
        {
            for (var x = 0; x + width <= StoreWidth; x++)
            {
                var free = true;
                for (var dx = 0; dx < width && free; dx++)
                {
                    for (var dy = 0; dy < height && free; dy++)
                    {
                        free = !occupied[x + dx, y + dy];
                    }
                }

                if (free)
                {
                    return (x, y);
                }
            }
        }

        return null;
    }
}
