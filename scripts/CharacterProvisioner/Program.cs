using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.Persistence.EntityFramework;
using MUnique.OpenMU.Persistence.EntityFramework.Json;
using MUnique.OpenMU.Persistence.Json;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.PlugIns;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration.Quests;
using org.mariuszgromada.math.mxparser;

JsonConverterRegistry.RegisterConverter(new LocalizedStringJsonConverter());
JsonConverterRegistry.RegisterConverter(new BinaryAsHexJsonConverter());
await new ConfigFileDatabaseConnectionStringProvider().InitializeAsync(default);
var provider = new PersistenceContextProvider(NullLoggerFactory.Instance, null);
using var configurationContext = provider.CreateNewConfigurationContext();
var configuration = (await configurationContext.GetAsync<GameConfiguration>()).Single();
using var context = provider.CreateNewPlayerContext(configuration);
var account = await context.GetAccountByLoginNameAsync("rkingg") ?? throw new InvalidOperationException("Target account missing.");
var characterClass = configuration.CharacterClasses.Single(c => c.Number == 23);
Console.WriteLine($"Account found; characters={account.Characters.Count}; class={characterClass.Name}; caps={configuration.MaximumLevel}/{configuration.MaximumMasterLevel}");
if (args.Contains("--upgrade-all"))
{
    await new CharacterUpgrades(configuration, context, account).RunAsync(args.Contains("--apply"), args.Contains("--check"));
    return;
}
if (args.Contains("--max-zen-fruit"))
{
    var saved = account.Characters.Single(c => c.Name == "OpenCodeX");
    Require(saved.CharacterClass == characterClass && saved.Inventory != null, "Unexpected target character.");
    var maximumFruit = saved.GetMaximumFruitPoints();
    var remaining = maximumFruit - saved.UsedFruitPoints;
    Require(remaining >= 0 && saved.UsedNegFruitPoints == 0, "Unexpected existing fruit accounting; review manually.");
    var energy = saved.Attributes.Single(a => a.Definition == Stats.BaseEnergy);
    energy.Value += remaining;
    saved.UsedFruitPoints = maximumFruit;
    saved.Inventory!.Money = configuration.MaximumInventoryMoney;
    Require(saved.UsedFruitPoints == maximumFruit && saved.Inventory.Money == configuration.MaximumInventoryMoney, "Resource cap mismatch.");
    Console.WriteLine($"RESOURCE PLAN OpenCodeX: Zen={saved.Inventory.Money}; positive fruit={maximumFruit}; added Energy={remaining}; final base Energy={energy.Value}; negative fruit=0.");
    if (!args.Contains("--apply")) { Console.WriteLine("VALIDATED DRY RUN: no SaveChanges called."); return; }
    Require(Environment.GetEnvironmentVariable("ORIGINSIX_OFFLINE_APPLY") == "OpenCodeX", "Offline apply confirmation missing.");
    Require(await context.SaveChangesAsync(), "Resource SaveChanges failed.");
    Console.WriteLine("SAVED maximum Zen and positive fruit points on OpenCodeX.");
    return;
}
if (args.Contains("--verify"))
{
    var saved = account.Characters.Single(c => c.Name == "OpenCodeX");
    Require(saved.CharacterClass == characterClass, "Saved class mismatch.");
    Require(saved.Attributes.Single(a => a.Definition == Stats.Level).Value == 400 &&
        saved.Attributes.Single(a => a.Definition == Stats.MasterLevel).Value == 200, "Saved level mismatch.");
    var masters = saved.LearnedSkills.Where(e => e.Skill?.MasterDefinition != null).ToArray();
    Require(masters.Length == 14 && masters.Sum(e => e.Level) == 200 && saved.MasterLevelUpPoints == 0, "Saved master budget mismatch.");
    foreach (var entry in masters)
    {
        var master = entry.Skill!.MasterDefinition!;
        Require(entry.Skill.QualifiedCharacters.Contains(characterClass) && entry.Level <= master.MaximumLevel, "Invalid saved master skill.");
        Require(master.Rank <= 1 || masters.Any(e => e.Level >= 10 && e.Skill!.MasterDefinition!.Root == master.Root &&
            e.Skill.MasterDefinition.Rank == master.Rank - 1), "Saved rank prerequisite missing.");
        Require(master.RequiredMasterSkills.All(s => saved.LearnedSkills.Any(e => e.Skill == s &&
            (s.MasterDefinition == null || e.Level >= 10))), "Saved skill prerequisite missing.");
    }
    var baseStats = new[] { Stats.BaseStrength, Stats.BaseAgility, Stats.BaseVitality, Stats.BaseEnergy };
    Require(saved.Attributes.Where(a => baseStats.Any(s => a.Definition == s)).Sum(a => a.Value) -
        baseStats.Sum(s => characterClass.StatAttributes.Single(a => a.Attribute == s).BaseValue) == 2255 + saved.UsedFruitPoints - saved.UsedNegFruitPoints && saved.LevelUpPoints == 0,
        "Saved normal stat budget mismatch.");
    Require(saved.Inventory!.Items.Count == 7, "Saved inventory count mismatch.");
    foreach (var item in saved.Inventory.Items)
    {
        Require(item.Level == 15 && item.Definition!.QualifiedCharacters.Contains(characterClass), "Invalid saved gear.");
        Require(item.ItemOptions.Count(o => o.ItemOption?.OptionType == ItemOptionTypes.Excellent) == 6, "Missing FO options.");
        Require(item.ItemOptions.Count(o => o.ItemOption?.OptionType == ItemOptionTypes.HarmonyOption) == 1, "Invalid yellow-option count.");
        foreach (var option in item.ItemOptions.Where(o => o.Level > 1))
            Require(option.ItemOption!.LevelDependentOptions.Any(l => l.Level == option.Level && l.RequiredItemLevel <= item.Level), "Unsupported saved option level.");
    }
    Console.WriteLine("VERIFIED: persisted level/class, 2255 normal points, 200 master points/prerequisites, seven +15 FO items and supported yellow/additional option levels.");
    return;
}
if (!args.Contains("--inspect"))
{
    await ProvisionAsync();
    return;
}
foreach (var skill in configuration.Skills.Where(s => s.MasterDefinition != null && s.QualifiedCharacters.Contains(characterClass)).OrderBy(s => s.MasterDefinition!.Root!.Id).ThenBy(s => s.MasterDefinition!.Rank).ThenBy(s => s.Number))
{
    var master = skill.MasterDefinition!;
    Console.WriteLine($"MASTER {skill.Number}: {skill.Name.ValueInNeutralLanguage} root={master.Root} rank={master.Rank} min={master.MinimumLevel} max={master.MaximumLevel} required=[{string.Join(',', master.RequiredMasterSkills.Select(s => s.Number))}]");
}
foreach (var definition in configuration.Items.Where(d => (d.Group == 5 && d.Number is 20 or 23) || (d.Group >= 7 && d.Group <= 11 && d.Number == 43)))
{
    var item = context.CreateNew<Item>();
    item.Definition = definition;
    item.Level = 15;
    Console.WriteLine($"ITEM {definition.Group}/{definition.Number}: {definition.Name.ValueInNeutralLanguage}; qualified={definition.QualifiedCharacters.Contains(characterClass)}; slots={string.Join(',', definition.ItemSlot!.ItemSlots)}");
    foreach (var options in definition.PossibleItemOptions)
    {
        Console.WriteLine($"  OPTION GROUP {options.Name.ValueInNeutralLanguage} max={options.MaximumOptionsPerItem}");
        foreach (var option in options.PossibleOptions)
            Console.WriteLine($"    {option.OptionType?.Name} number={option.Number} target={option.PowerUpDefinition?.TargetAttribute} levels=[{string.Join(',', option.LevelDependentOptions.Select(l => l.Level))}]");
    }
    foreach (var requirement in definition.Requirements.Select(item.GetRequirement))
        Console.WriteLine($"  REQ {requirement.Attr}: {requirement.Value}");
}
Console.WriteLine("INSPECT ONLY: no SaveChanges was called.");

async Task ProvisionAsync()
{
    const string name = "OpenCodeX";
    Require(configuration.MaximumLevel == 400 && configuration.MaximumMasterLevel == 200, "Unexpected level caps.");
    Require(configuration.IsValidCharacterName(name), "Invalid character name.");
    Require(await context.GetAccountIdByCharacterNameAsync(name) is null, "Character already exists; refusing to overwrite.");
    var slot = account.GetFreeCharacterSlot(configuration) ?? throw new InvalidOperationException("Account is full.");
    var character = context.CreateNew<Character>();
    character.Name = name;
    character.CharacterSlot = slot;
    context.InitializeNewCharacter(character, characterClass);
    SetStat(character, Stats.Level, configuration.MaximumLevel);
    SetStat(character, Stats.MasterLevel, configuration.MaximumMasterLevel);
    character.Experience = Experience(configuration.ExperienceFormula!, configuration.MaximumLevel);
    character.MasterExperience = Experience(configuration.MasterExperienceFormula!, configuration.MaximumMasterLevel);

    // Complete only the class-advancement chain, applying its configured rewards.
    var lastQuest = configuration.Monsters.Single(m => m.Number == 406).Quests.Single(q => q.Number == 6);
    var quests = configuration.Monsters.Where(m => m.Number is 235 or 229 or 406)
        .SelectMany(m => m.Quests)
        .Where(q => q.Group == lastQuest.Group && q.Number <= 6 &&
            (q.QualifiedCharacter == null || q.QualifiedCharacter.Number is 20 or 22 or 23))
        .OrderBy(q => q.Number).ToArray();
    Require(quests.Length == 6, "Unexpected advancement quest chain.");
    var pointsPerLevel = characterClass.StatAttributes.Single(a => a.Attribute == Stats.PointsPerLevelUp).BaseValue;
    var statBudget = checked((int)((configuration.MaximumLevel - 1) * pointsPerLevel));
    foreach (var quest in quests)
    foreach (var reward in quest.Rewards)
    {
        if (reward.RewardType == QuestRewardType.LevelUpPoints) statBudget += reward.Value;
        if (reward.RewardType != QuestRewardType.Attribute || reward.AttributeReward == null) continue;
        var oldValue = character.Attributes.FirstOrDefault(a => a.Definition == reward.AttributeReward)?.Value ?? 0;
        SetStat(character, reward.AttributeReward, oldValue + reward.Value);
        if (reward.AttributeReward == Stats.PointsPerLevelUp)
            statBudget += (configuration.MaximumLevel - quest.MinimumCharacterLevel) * reward.Value;
    }
    var questState = context.CreateNew<CharacterQuestState>();
    questState.Group = lastQuest.Group;
    questState.LastFinishedQuest = lastQuest;
    character.QuestStates.Add(questState);
    Require(statBudget == 2255, "Unexpected stat budget; re-review progression.");

    // Requested FO admin-test equipment: no sockets or invented option definitions.
    AddGear(character, 5, 20);
    AddGear(character, 5, 23);
    for (byte group = 7; group <= 11; group++) AddGear(character, group, 43);
    var requirements = character.Inventory!.Items.SelectMany(i => i.Definition!.Requirements.Select(i.GetRequirement)).ToArray();
    var strength = Math.Max(21, requirements.Where(r => r.Attr == Stats.TotalStrength).Max(r => r.Value));
    var agility = Math.Max(300, requirements.Where(r => r.Attr == Stats.TotalAgility).Max(r => r.Value));
    const int vitality = 200;
    var baseStats = new[] { Stats.BaseStrength, Stats.BaseAgility, Stats.BaseVitality, Stats.BaseEnergy };
    var initialStatSum = baseStats.Sum(stat => characterClass.StatAttributes.Single(a => a.Attribute == stat).BaseValue);
    var energy = checked((int)(initialStatSum + statBudget - strength - agility - vitality));
    SetStat(character, Stats.BaseStrength, strength);
    SetStat(character, Stats.BaseAgility, agility);
    SetStat(character, Stats.BaseVitality, vitality);
    SetStat(character, Stats.BaseEnergy, energy);
    character.LevelUpPoints = 0;
    foreach (var requirement in requirements)
    {
        var actual = requirement.Attr == Stats.Level ? 400 : requirement.Attr == Stats.TotalStrength ? strength :
            requirement.Attr == Stats.TotalAgility ? agility : requirement.Attr == Stats.TotalEnergy ? energy :
            throw new InvalidOperationException($"Unreviewed equipment requirement {requirement.Attr}.");
        Require(actual >= requirement.Value, $"Equipment requirement failed: {requirement.Attr}.");
    }

    // Summoner PvM spells, not event/GM skills; book skill is supplied by the equipped book.
    foreach (var skill in configuration.Skills.Where(s => s.Number is 214 or 215 or 217 or 218 or 219 or 221 or 222 or 230))
    {
        Require(skill.MasterDefinition == null && skill.QualifiedCharacters.Contains(characterClass), "Invalid regular skill.");
        var entry = context.CreateNew<SkillEntry>();
        entry.Skill = skill;
        character.LearnedSkills.Add(entry);
    }
    Require(character.LearnedSkills.Count == 8, "Missing regular skill.");

    // 100 points in spell/tome PvM damage; 100 in stick/tome mastery, sustain and Berserker.
    (short Id, int Level)[] build = [(325,20),(450,10),(453,10),(455,20),(456,20),(457,20),
        (347,10),(465,10),(466,10),(467,20),(468,10),(359,10),(469,10),(470,20)];
    var masterBudget = checked((int)(configuration.MaximumMasterLevel * characterClass.BaseAttributeValues.Single(a => a.Definition == Stats.MasterPointsPerLevelUp).Value));
    Require(masterBudget == 200 && build.Sum(x => x.Level) == masterBudget, "Invalid master-point budget.");
    foreach (var allocation in build)
    {
        var skill = configuration.Skills.Single(s => s.Number == allocation.Id);
        var master = skill.MasterDefinition ?? throw new InvalidOperationException("Not a master skill.");
        Require(skill.QualifiedCharacters.Contains(characterClass), "Wrong master-skill class.");
        Require(allocation.Level >= master.MinimumLevel && allocation.Level <= master.MaximumLevel, "Invalid master-skill level.");
        Require(master.Rank <= 1 || character.LearnedSkills.Any(e => e.Level >= 10 &&
            e.Skill?.MasterDefinition is { } previous && previous.Root == master.Root && previous.Rank == master.Rank - 1), "Missing previous rank.");
        Require(master.RequiredMasterSkills.All(required => character.LearnedSkills.Any(e => e.Skill == required &&
            (required.MasterDefinition == null || e.Level >= 10))), "Missing required master/base skill.");
        var entry = context.CreateNew<SkillEntry>();
        entry.Skill = skill;
        entry.Level = allocation.Level;
        character.LearnedSkills.Add(entry);
        Console.WriteLine($"MASTER {skill.Number}: {skill.Name.ValueInNeutralLanguage} = {allocation.Level}");
    }
    character.MasterLevelUpPoints = masterBudget - build.Sum(x => x.Level);
    Require(character.Inventory.Items.Count == 7 && character.Inventory.Items.Select(i => i.ItemSlot).Distinct().Count() == 7, "Invalid equipment slots.");
    Require(character.Attributes.Where(a => baseStats.Any(stat => a.Definition == stat)).Sum(a => a.Value) - initialStatSum == statBudget, "Stat accounting mismatch.");
    Console.WriteLine($"PLAN {name}: level 400/master 200; STR={strength}, AGI={agility}, VIT={vitality}, ENE={energy}; normal budget={statBudget}, master spent={masterBudget}; free points=0/0.");
    foreach (var item in character.Inventory.Items) Console.WriteLine($"GEAR {item}; durability={item.Durability}");
    if (!args.Contains("--apply")) { Console.WriteLine("VALIDATED DRY RUN: no SaveChanges called."); return; }
    Require(Environment.GetEnvironmentVariable("ORIGINSIX_OFFLINE_APPLY") == name, "Offline apply confirmation missing.");
    account.Characters.Add(character);
    Require(await context.SaveChangesAsync(), "SaveChanges failed.");
    Console.WriteLine($"SAVED {name} on rkingg, character slot {slot}.");
}

void AddGear(Character character, byte group, short number)
{
    var definition = configuration.Items.Single(d => d.Group == group && d.Number == number);
    Require(definition.QualifiedCharacters.Contains(characterClass), "Wrong equipment class.");
    var item = context.CreateNew<Item>();
    item.Definition = definition;
    item.Level = definition.MaximumItemLevel;
    Require(item.Level == 15, "Unexpected item-level cap.");
    item.ItemSlot = checked((byte)definition.ItemSlot!.ItemSlots.Single());
    item.HasSkill = definition.Skill != null;
    var options = definition.PossibleItemOptions.SelectMany(o => o.PossibleOptions).ToArray();
    foreach (var option in options.Where(o => o.OptionType == ItemOptionTypes.Excellent || o.OptionType == ItemOptionTypes.Luck || o.OptionType == ItemOptionTypes.GuardianOption))
        LinkOption(item, option, 0);
    var additional = options.Single(o => o.OptionType == ItemOptionTypes.Option);
    LinkOption(item, additional, additional.LevelDependentOptions.Max(l => l.Level));
    var harmony = options.Single(o => o.OptionType == ItemOptionTypes.HarmonyOption && o.Number == (group == 5 ? 4 : 7));
    var harmonyLevel = harmony.LevelDependentOptions.Where(l => l.RequiredItemLevel <= item.Level).Max(l => l.Level);
    LinkOption(item, harmony, harmonyLevel);
    Require(item.ItemOptions.Count(o => o.ItemOption?.OptionType == ItemOptionTypes.Excellent) == 6, "Expected all six Excellent options.");
    Require(item.ItemOptions.Count(o => o.ItemOption?.OptionType == ItemOptionTypes.HarmonyOption) == 1, "One yellow option only.");
    item.Durability = item.GetMaximumDurabilityOfOnePiece();
    character.Inventory!.Items.Add(item);
}

void LinkOption(Item item, IncreasableItemOption option, int level)
{
    var link = context.CreateNew<ItemOptionLink>();
    link.ItemOption = option;
    link.Level = level;
    Require(level <= 1 || option.LevelDependentOptions.Any(l => l.Level == level && l.RequiredItemLevel <= item.Level), "Undefined or inactive item-option level.");
    item.ItemOptions.Add(link);
}

void SetStat(Character character, AttributeDefinition definition, float value)
{
    var stat = character.Attributes.SingleOrDefault(a => a.Definition == definition);
    if (stat == null)
    {
        stat = context.CreateNew<StatAttribute>(configuration.Attributes.Single(a => a == definition), value);
        character.Attributes.Add(stat);
    }
    else stat.Value = value;
}

static long Experience(string formula, int level)
{
    var argument = new Argument("level", level);
    var expression = new Expression(formula, argument);
    var result = expression.calculate();
    Require(double.IsFinite(result) && result >= 0, "Invalid experience formula result.");
    return checked((long)result);
}

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
