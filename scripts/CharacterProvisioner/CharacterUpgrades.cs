using System.Text.Json.Nodes;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Configuration.Quests;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Persistence;
using org.mariuszgromada.math.mxparser;

// Targeted local test-data operation. No initializer or global game configuration is edited.
internal sealed class CharacterUpgrades(GameConfiguration config, IPlayerContext context, Account account)
{
    private readonly JsonNode clientStats = JsonNode.Parse(File.ReadAllText("/client-items/Stats/ItemStats.json"))!;
    private readonly Dictionary<int, JsonNode> clientGroups = new();
    private static readonly AttributeDefinition[] BaseStats = [Stats.BaseStrength, Stats.BaseAgility, Stats.BaseVitality, Stats.BaseEnergy];
    private static readonly HashSet<short> ExcludedSkills = [58,67,68,69,70,71,76,210,211,212,213,49];

    // Authorized local test-data exception: every main attribute of every test character is raised
    // by this amount above the legal point budget. It is part of the intended saved state, so the
    // allocation and the validation both account for it.
    private const int TestStatBoost = 5000;

    public async Task RunAsync(bool apply, bool check)
    {
        Check(config.MaximumLevel == 400 && config.MaximumMasterLevel == 200, "Unexpected caps.");
        var sum = account.Characters.Single(c => c.Name == "OpenCodeX");
        var rf = account.Characters.Single(c => c.Name == "RKINGG");
        if (check)
        {
            foreach (var character in new[] { sum, rf, account.Characters.Single(c => c.Name == "OpenCodeXx") })
                Validate(character);
            Console.WriteLine("VERIFIED: all three persisted characters, point budgets, skills, socket levels, bag geometry and both client/server requirements.");
            return;
        }
        Check(sum.CharacterClass?.Number == 23 && rf.CharacterClass?.Number is 24 or 25, "Unexpected target classes.");
        Check(await context.GetAccountIdByCharacterNameAsync("OpenCodeXx") is null, "BM already exists; refusing duplicate creation.");
        var bm = context.CreateNew<Character>();
        bm.Name = "OpenCodeXx";
        Check(config.IsValidCharacterName(bm.Name),"Invalid new character name.");
        bm.CharacterSlot = account.GetFreeCharacterSlot(config) ?? throw new InvalidOperationException("Account is full.");
        context.InitializeNewCharacter(bm, config.CharacterClasses.Single(c => c.Number == 7));

        Equip(sum, 12,43); Equip(sum,13,12,9); Equip(sum,13,8,10); Equip(sum,13,21,11);
        LevelAndQuest(bm,7);
        Equip(bm,0,26,0,true); Equip(bm,0,26,1,true);
        for (var group=7; group<=11; group++) Equip(bm,group,45,socket:true);
        for (var group=7; group<=11; group++) Bag(bm,MakeItem(bm,group,46,true));
        Equip(bm,12,36); Equip(bm,13,13,9); Equip(bm,13,8,10); Equip(bm,13,21,11);

        LevelAndQuest(rf,25);
        Equip(rf,0,35,0); Equip(rf,0,35,1);
        foreach (var group in new[] {7,8,9,11}) Equip(rf,group,73);
        Equip(rf,12,50); Equip(rf,13,13,9); Equip(rf,13,8,10); Equip(rf,13,21,11);

        foreach (var character in new[] {sum,bm,rf}) LearnRegular(character);
        await MasterBuild(bm, [(325,20),(326,10),(330,20),(331,10),(332,20),(336,20),
            (347,10),(349,20),(353,10),(359,20),(361,20),(362,20)]);
        await MasterBuild(rf, [(599,20),(551,10),(552,10),(554,10),(555,10),(557,20),(559,20),(560,20),
            (603,10),(568,20),(571,20),(606,10),(607,20)]);
        foreach (var character in new[] {sum,bm,rf})
        {
            character.UsedFruitPoints = character.GetMaximumFruitPoints();
            Check(character.UsedNegFruitPoints == 0, "Negative-fruit history needs separate review.");
            Allocate(character);
            character.Inventory!.Money = config.MaximumInventoryMoney;
            Validate(character);
            Print(character);
        }
        if (!apply) { Console.WriteLine("VALIDATED DRY RUN: no SaveChanges called."); return; }
        Check(Environment.GetEnvironmentVariable("ORIGINSIX_OFFLINE_APPLY") == "OpenCodeX", "Offline confirmation missing.");
        account.Characters.Add(bm);
        Check(await context.SaveChangesAsync(), "SaveChanges failed.");
        Console.WriteLine("SAVED: Summoner additions/STR correction, new OpenCodeXx Blade Master, existing RKINGG Fist Master upgrade.");
    }

    private void LevelAndQuest(Character character, int classNumber)
    {
        var finalClass = config.CharacterClasses.Single(c => c.Number == classNumber);
        var existing = account.Characters.Contains(character);
        var created = character.CreateDate;
        var keys = character.KeyConfiguration;
        var map = character.CurrentMap;
        var x = character.PositionX;
        var y = character.PositionY;
        context.InitializeNewCharacter(character, finalClass);
        if (existing)
        {
            character.CreateDate=created; character.KeyConfiguration=keys;
            character.CurrentMap=map; character.PositionX=x; character.PositionY=y;
        }
        Set(character,Stats.Level,400); Set(character,Stats.MasterLevel,200);
        character.Experience = Exp(config.ExperienceFormula!,400);
        character.MasterExperience = Exp(config.MasterExperienceFormula!,200);
        foreach (var stat in finalClass.StatAttributes.Where(s => s.Attribute == Stats.PointsPerLevelUp)) Set(character,stat.Attribute!,stat.BaseValue);
        foreach (var quest in Advancement(character))
        foreach (var reward in quest.Rewards.Where(r => r.RewardType == QuestRewardType.Attribute && r.AttributeReward != null))
        {
            var initial = finalClass.StatAttributes.FirstOrDefault(s => s.Attribute == reward.AttributeReward)?.BaseValue ?? 0;
            Set(character,reward.AttributeReward!,initial+reward.Value);
        }
        var last = Advancement(character).MaxBy(q=>q.Number)!;
        var state = character.QuestStates.SingleOrDefault(s=>s.Group==last.Group);
        if (state==null) { state=context.CreateNew<CharacterQuestState>(); state.Group=last.Group; character.QuestStates.Add(state); }
        Check(state.ActiveQuest == null, "Active advancement quest must be resolved first.");
        state.LastFinishedQuest=last;
    }

    private QuestDefinition[] Advancement(Character character)
    {
        int[] lineage = character.CharacterClass!.Number switch { 7 => [4,6,7], 23=>[20,22,23],25=>[24,25],_=>throw new InvalidOperationException("Unsupported class.") };
        var last=config.Monsters.Single(m=>m.Number==406).Quests.Single(q=>q.Number==6);
        return config.Monsters.Where(m=>m.Number is 235 or 229 or 406).SelectMany(m=>m.Quests)
            .Where(q=>q.Group==last.Group && q.Number<=6 && (q.QualifiedCharacter==null || lineage.Contains(q.QualifiedCharacter.Number)))
            .OrderBy(q=>q.Number).ToArray();
    }

    private int StatBudget(Character character)
    {
        var cc=character.CharacterClass!;
        var initial=cc.StatAttributes.Single(s=>s.Attribute==Stats.PointsPerLevelUp).BaseValue;
        var budget=(int)(399*initial);
        foreach (var q in Advancement(character))
        foreach (var r in q.Rewards)
        {
            if (r.RewardType==QuestRewardType.LevelUpPoints) budget+=r.Value;
            if (r.RewardType==QuestRewardType.Attribute && r.AttributeReward==Stats.PointsPerLevelUp) budget+=(400-q.MinimumCharacterLevel)*r.Value;
        }
        return budget;
    }

    private Item MakeItem(Character character,int group,int number,bool socket)
    {
        var def=config.Items.Single(d=>d.Group==group && d.Number==number);
        Check(def.QualifiedCharacters.Count==0 || def.QualifiedCharacters.Contains(character.CharacterClass!),$"Wrong item class: {def.Name}.");
        var item=context.CreateNew<Item>(); item.Definition=def; item.Level=def.MaximumItemLevel; item.HasSkill=def.Skill!=null;
        var options=def.PossibleItemOptions.SelectMany(o=>o.PossibleOptions).Distinct().ToArray();
        foreach (var option in options.Where(o=>o.OptionType==ItemOptionTypes.Luck || o.OptionType==ItemOptionTypes.Wing ||
            (!socket && (o.OptionType==ItemOptionTypes.Excellent || o.OptionType==ItemOptionTypes.GuardianOption)))) Link(item,option,0);
        var additional=options.Where(o=>o.OptionType==ItemOptionTypes.Option)
            .OrderByDescending(o=>o.PowerUpDefinition?.TargetAttribute== (character.CharacterClass!.Number==23 ? Stats.WizardryBaseDmg : Stats.PhysicalBaseDmg)).FirstOrDefault();
        if (additional!=null) Link(item,additional,additional.LevelDependentOptions.Select(l=>l.Level).DefaultIfEmpty(1).Max());
        if (socket)
        {
            Check(def.MaximumSockets>0,"Not a socket item."); item.SocketCount=def.MaximumSockets;
            Check(item.SocketCount==3,"Review changed socket maximum.");
            (SocketSubOptionType Type,int Number)[] seeds=group<6
                ? [(SocketSubOptionType.Lightning,1),(SocketSubOptionType.Ice,2),(SocketSubOptionType.Fire,4)]
                : [(SocketSubOptionType.Water,3),(SocketSubOptionType.Earth,0),(SocketSubOptionType.Wind,1)];
            for (var i=0;i<seeds.Length;i++)
            {
                var seed=options.Single(o=>o.OptionType==ItemOptionTypes.SocketOption && o.SubOptionType==(int)seeds[i].Type && o.Number==seeds[i].Number);
                Link(item,seed,seed.LevelDependentOptions.Max(l=>l.Level),i);
            }
            var bonus=options.Where(o=>o.OptionType==ItemOptionTypes.SocketBonusOption).OrderBy(o=>o.Number);
            Link(item,group<6 ? bonus.Last() : bonus.First(),0);
        }
        else
        {
            var harmony=options.FirstOrDefault(o=>o.OptionType==ItemOptionTypes.HarmonyOption && o.Number==(group>=7 && group<=11 ? 7 : 4));
            if (harmony!=null) Link(item,harmony,harmony.LevelDependentOptions.Where(l=>l.RequiredItemLevel<=item.Level).Max(l=>l.Level));
        }
        item.Durability=item.GetMaximumDurabilityOfOnePiece(); return item;
    }

    private void Equip(Character c,int group,int number,int? slot=null,bool socket=false)
    {
        var def=config.Items.Single(d=>d.Group==group && d.Number==number);
        var target=slot ?? def.ItemSlot!.ItemSlots.Single();
        Check(def.ItemSlot!.ItemSlots.Contains(target),"Invalid equipped slot.");
        var current=c.Inventory!.Items.SingleOrDefault(i=>i.ItemSlot==target);
        if (current?.Definition==def && current.Level==def.MaximumItemLevel) return;
        if (current!=null) Bag(c,current);
        var item=MakeItem(c,group,number,socket); item.ItemSlot=checked((byte)target); c.Inventory.Items.Add(item);
    }

    private void Bag(Character c,Item item)
    {
        var occupied=BagCells(c,item);
        for (var y=0;y<8;y++) for(var x=0;x<8;x++)
        {
            if (x+item.Definition!.Width>8 || y+item.Definition.Height>8) continue;
            var cells=Cells(12+y*8+x,item).ToArray();
            if(cells.Any(occupied.Contains)) continue;
            item.ItemSlot=(byte)(12+y*8+x); if(!c.Inventory!.Items.Contains(item)) c.Inventory.Items.Add(item); return;
        }
        throw new InvalidOperationException($"No safe bag space on {c.Name}.");
    }
    private static IEnumerable<int> Cells(int slot,Item item)
    {
        for(var y=0;y<item.Definition!.Height;y++) for(var x=0;x<item.Definition.Width;x++) yield return slot+y*8+x;
    }
    private static HashSet<int> BagCells(Character c,Item? except=null) => c.Inventory!.Items.Where(i=>i!=except && i.ItemSlot>=12).SelectMany(i=>Cells(i.ItemSlot,i)).ToHashSet();

    private void LearnRegular(Character c)
    {
        var skills=config.Skills.Where(s=>s.MasterDefinition==null && s.Number<300 && !ExcludedSkills.Contains(s.Number) && s.QualifiedCharacters.Contains(c.CharacterClass!));
        foreach(var skill in skills)
        {
            if(c.LearnedSkills.Any(e=>e.Skill==skill)) continue;
            var entry=context.CreateNew<SkillEntry>(); entry.Skill=skill; c.LearnedSkills.Add(entry);
        }
    }

    private async Task MasterBuild(Character c,(short Number,int Level)[] build)
    {
        foreach(var entry in c.LearnedSkills.Where(e=>e.Skill?.MasterDefinition!=null).ToArray()) { c.LearnedSkills.Remove(entry); await context.DeleteAsync(entry); }
        Check(build.Sum(b=>b.Level)==200,"Master budget mismatch.");
        foreach(var allocation in build)
        {
            var skill=config.Skills.Single(s=>s.Number==allocation.Number);
            var entry=context.CreateNew<SkillEntry>(); entry.Skill=skill; entry.Level=allocation.Level;
            MasterPrerequisites(c,entry); c.LearnedSkills.Add(entry);
        }
        c.MasterLevelUpPoints=0;
    }
    private static void MasterPrerequisites(Character c,SkillEntry entry)
    {
        var skill=entry.Skill!; var md=skill.MasterDefinition!;
        Check(skill.QualifiedCharacters.Contains(c.CharacterClass!) && entry.Level>=md.MinimumLevel && entry.Level<=md.MaximumLevel,"Illegal master level/class.");
        Check(md.Rank<=1 || c.LearnedSkills.Any(e=>e.Level>=10 && e.Skill?.MasterDefinition is {} prev && prev.Root==md.Root && prev.Rank==md.Rank-1),"Missing previous master rank.");
        Check(md.RequiredMasterSkills.All(r=>c.LearnedSkills.Any(e=>e.Skill==r && (r.MasterDefinition==null || e.Level>=10))),"Missing required skill.");
    }

    private Dictionary<AttributeDefinition,int> Requirements(Character c)
    {
        var result=new Dictionary<AttributeDefinition,int>();
        foreach(var item in c.Inventory!.Items.Where(i=>i.ItemSlot<12 || (c.Name=="OpenCodeXx" && i.Definition!.Number==46)))
        {
            foreach(var r in item.Definition!.Requirements.Select(item.GetRequirement)) Add(r.Attr,r.Value);
            foreach(var r in ClientRequirements(item)) Add(r.Key,r.Value);
        }
        foreach(var skill in c.LearnedSkills.Select(e=>e.Skill!).Where(s=>s.MasterDefinition==null))
            foreach(var r in skill.Requirements) Add(r.Attribute!,r.MinimumValue);
        return result;
        void Add(AttributeDefinition a,int value) { result[a]=Math.Max(result.GetValueOrDefault(a),value); }
    }

    private Dictionary<AttributeDefinition,int> ClientRequirements(Item item)
    {
        var group=item.Definition!.Group;
        if(!clientGroups.TryGetValue(group,out var data))
        {
            var file=Directory.GetFiles("/client-items",$"Group{group:00}_*.json").Single();
            clientGroups[group]=data=JsonNode.Parse(File.ReadAllText(file))!;
        }
        var d=data["items"]!.AsArray().Single(n=>n!["number"]!.GetValue<int>()==item.Definition.Number)!;
        Check(d["requirementsByLevel"]==null,"Review client per-level requirement overrides.");
        var profileName=d["statProfile"]?.GetValue<string>() ?? "none";
        var profile=clientStats["profiles"]!.AsArray().SingleOrDefault(p=>p!["name"]!.GetValue<string>()==profileName)?["requirements"];
        var defaults=clientStats["default"]!["requirements"]!;
        var req=d["requirements"];
        var excellent=item.ItemOptions.Any(o=>o.ItemOption?.OptionType==ItemOptionTypes.Excellent);
        var additional=item.ItemOptions.SingleOrDefault(o=>o.ItemOption?.OptionType==ItemOptionTypes.Option)?.Level ?? 0;
        var drop=(d["level"]?.GetValue<int>() ?? 0)+(excellent ? clientStats["dropLevelBonus"]!["excellent"]!.GetValue<int>() : 0);
        var result=new Dictionary<AttributeDefinition,int>();
        (string Key,AttributeDefinition Attr)[] stats=[("strength",Stats.TotalStrength),("dexterity",Stats.TotalAgility),("energy",Stats.TotalEnergy),("vitality",Stats.TotalVitality),("leadership",Stats.TotalLeadership)];
        foreach(var stat in stats)
        {
            var basis=req?[stat.Key]?.GetValue<int>() ?? 0; var term=profile?[stat.Key] ?? defaults[stat.Key];
            var value=0;
            if(basis>0)
            {
                if(term is JsonValue) value=basis;
                else value=(term?["add"]?.GetValue<int>() ?? 0)+basis*(drop+item.Level*(term?["levelWeight"]?.GetValue<int>() ?? 0))*(term?["percent"]?.GetValue<int>() ?? 0)/100;
            }
            if(stat.Key=="strength") value+=(profile?["strengthPerOptionLevel"] ?? defaults["strengthPerOptionLevel"])!.GetValue<int>()*additional;
            if(value>0) result[stat.Attr]=value;
        }
        var level=req?["level"]?.GetValue<int>() ?? 0;
        if(level>0) level+=item.Level*(profile?["levelPerItemLevel"] ?? defaults["levelPerItemLevel"])!.GetValue<int>();
        if(excellent && level>0 && item.Definition.Group!=12 && (d["excellentRaisesLevel"]?.GetValue<bool>() ?? true)) level+=defaults["excellentLevelAdd"]!.GetValue<int>();
        if(level>0) result[Stats.Level]=level;
        return result;
    }

    private void Allocate(Character c)
    {
        var req=Requirements(c); int Min(AttributeDefinition attr,int minimum)=>Math.Max(req.GetValueOrDefault(attr),minimum);
        var cc=c.CharacterClass!;
        var total=StatBudget(c)+c.UsedFruitPoints-c.UsedNegFruitPoints+(int)BaseStats.Sum(s=>cc.StatAttributes.Single(a=>a.Attribute==s).BaseValue);
        var str=Min(Stats.TotalStrength,(int)cc.StatAttributes.Single(a=>a.Attribute==Stats.BaseStrength).BaseValue);
        var agi=Min(Stats.TotalAgility,cc.Number==25 ? 700 : cc.Number==7 ? 300 : 300);
        var vit=Min(Stats.TotalVitality,200); var ene=Min(Stats.TotalEnergy,cc.Number==25 ? 300 : cc.Number==7 ? 100 : 23);
        var remaining=total-str-agi-vit-ene; Check(remaining>=0,$"Gear/skills exceed legal stat budget on {c.Name}: {str}/{agi}/{vit}/{ene} > {total}.");
        if(cc.Number==23) ene+=remaining; else if(cc.Number==7) str+=remaining; else vit+=remaining;
        str+=TestStatBoost; agi+=TestStatBoost; vit+=TestStatBoost; ene+=TestStatBoost;
        Set(c,Stats.BaseStrength,str); Set(c,Stats.BaseAgility,agi); Set(c,Stats.BaseVitality,vit); Set(c,Stats.BaseEnergy,ene); c.LevelUpPoints=0;
    }

    private void Validate(Character c)
    {
        Check(Get(c,Stats.Level)==400 && Get(c,Stats.MasterLevel)==200,"Level mismatch.");
        Check(c.UsedFruitPoints==c.GetMaximumFruitPoints(),"Fruit not at legal cap.");
        var baseTotal=BaseStats.Sum(s=>c.CharacterClass!.StatAttributes.Single(a=>a.Attribute==s).BaseValue);
        Check(BaseStats.Sum(s=>Get(c,s))+c.LevelUpPoints-baseTotal==StatBudget(c)+c.UsedFruitPoints-c.UsedNegFruitPoints+BaseStats.Length*TestStatBoost,"Normal-point accounting mismatch.");
        var masters=c.LearnedSkills.Where(e=>e.Skill?.MasterDefinition!=null).ToArray();
        Check(masters.Sum(e=>e.Level)+c.MasterLevelUpPoints==200,"Master-point accounting mismatch.");
        foreach(var e in masters) MasterPrerequisites(c,e);
        foreach(var skill in config.Skills.Where(s=>s.MasterDefinition==null && s.Number<300 && !ExcludedSkills.Contains(s.Number) && s.QualifiedCharacters.Contains(c.CharacterClass!)))
            Check(c.LearnedSkills.Any(e=>e.Skill==skill),"A normal class skill is missing.");
        foreach(var r in Requirements(c))
        {
            var actual=r.Key==Stats.TotalStrength ? Get(c,Stats.BaseStrength) : r.Key==Stats.TotalAgility ? Get(c,Stats.BaseAgility) : r.Key==Stats.TotalVitality ? Get(c,Stats.BaseVitality) : r.Key==Stats.TotalEnergy ? Get(c,Stats.BaseEnergy) : Get(c,r.Key);
            Check(actual>=r.Value,$"{c.Name} lacks {r.Key}: {actual} < {r.Value}.");
        }
        var cells=new HashSet<int>();
        foreach(var item in c.Inventory!.Items)
        {
            Check(item.Definition!=null,"Item definition missing.");
            foreach(var option in item.ItemOptions.Where(o=>o.Level>1)) Check(option.ItemOption!.LevelDependentOptions.Any(l=>l.Level==option.Level && l.RequiredItemLevel<=item.Level),"Undefined option level.");
            var seeds=item.ItemOptions.Where(o=>o.ItemOption?.OptionType==ItemOptionTypes.SocketOption).ToArray();
            Check(seeds.Length<=item.SocketCount && item.SocketCount<=item.Definition!.MaximumSockets,"Invalid socket count.");
            if(seeds.Length>0) Check(seeds.Length==3 && seeds.Select(s=>s.Index).Order().SequenceEqual(new[]{0,1,2}) && seeds.All(s=>s.Level==s.ItemOption!.LevelDependentOptions.Max(l=>l.Level)),"Socket maximum/index mismatch.");
            if(item.ItemSlot<12)
            {
                Check(item.Definition!.ItemSlot!.ItemSlots.Contains(item.ItemSlot),"Wrong equipped slot.");
                Check(item.Definition.QualifiedCharacters.Count==0 || item.Definition.QualifiedCharacters.Contains(c.CharacterClass!),"Wrong equipped class.");
            }
            else
            {
                Check((item.ItemSlot-12)%8+item.Definition!.Width<=8 && (item.ItemSlot-12)/8+item.Definition.Height<=8,"Bag boundary exceeded.");
                foreach(var cell in Cells(item.ItemSlot,item)) Check(cells.Add(cell),"Bag overlap.");
            }
        }
    }

    private void Link(Item item,IncreasableItemOption option,int level,int index=0)
    {
        var link=context.CreateNew<ItemOptionLink>(); link.ItemOption=option; link.Level=level; link.Index=index; item.ItemOptions.Add(link);
    }
    private void Set(Character c,AttributeDefinition def,float value)
    {
        var stat=c.Attributes.SingleOrDefault(a=>a.Definition==def);
        if(stat==null) { stat=context.CreateNew<StatAttribute>(config.Attributes.Single(a=>a==def),value); c.Attributes.Add(stat); } else stat.Value=value;
    }
    private static float Get(Character c,AttributeDefinition def)=>c.Attributes.SingleOrDefault(a=>a.Definition==def)?.Value ?? 0;
    private static long Exp(string formula,int level) { var expression=new Expression(formula,new Argument("level",level)); var value=expression.calculate(); Check(double.IsFinite(value),"Invalid EXP formula."); return checked((long)value); }
    private static void Check(bool value,string message) { if(!value) throw new InvalidOperationException(message); }
    private void Print(Character c)
    {
        Console.WriteLine($"PLAN {c.Name}: {c.CharacterClass!.Name.ValueInNeutralLanguage}; 400/200; STR {Get(c,Stats.BaseStrength)}, AGI {Get(c,Stats.BaseAgility)}, VIT {Get(c,Stats.BaseVitality)}, ENE {Get(c,Stats.BaseEnergy)}; fruit {c.UsedFruitPoints}; normal budget {StatBudget(c)}; master {c.LearnedSkills.Where(e=>e.Skill?.MasterDefinition!=null).Sum(e=>e.Level)}; regular skills {c.LearnedSkills.Count(e=>e.Skill?.MasterDefinition==null)}");
        foreach(var i in c.Inventory!.Items.OrderBy(i=>i.ItemSlot)) Console.WriteLine($"  GEAR slot {i.ItemSlot}: {i.Definition!.Name.ValueInNeutralLanguage}+{i.Level}; sockets={i.SocketCount}; options={i.ItemOptions.Count}");
    }
}
