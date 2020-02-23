using Awv.Automation.Generation;
using Awv.Automation.Generation.Interface;
using Awv.Automation.Lexica;
using Awv.Automation.SocialMedia.Facebook;
using Awv.Automation.SocialMedia.Facebook.Posts;
using Awv.Automation.SocialMedia.Twitter;
using Awv.Automation.SocialMedia.Twitter.Tweets;
using Awv.Automizer.Data;
using Awv.Automizer.Data.Models;
using Awv.Bots.Logging;
using Awv.Bots.Runner;
using Awv.Bots.WoWItemGen.DataUpkeep;
using Awv.Bots.WoWItemGen.Generators;
using Awv.Bots.WoWItemGen.Libraries;
using Awv.Games.WoW.Graphics;
using Awv.Games.WoW.Items.Equipment;
using Awv.Games.WoW.Items.Equipment.Interface;
using Awv.Games.WoW.Stats;
using Awv.Games.WoW.Tooltips;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Awv.Bots.WoWItemGen
{
    class Program
    {
        static Configurator App { get; set; }
        static ILogger logger { get; set; } = new AwvLogFactory().CreateLogger<Program>();
        static void Main(string[] args)
        {
            Configure(args);
            VerifyData();
            var generator = CreateGenerator();
            try
            {

                PostToFacebook(generator);
                logger.LogInformation("Posted to Facebook!");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error posting to Facebook...");
            }

            try
            {
                PostToTwitter(generator);
                logger.LogInformation("Posted to Twitter!");
            }
            catch(Exception exception)
            {
                logger.LogError(exception, "Error posting to Twitter...");
            }
        }

        static void PostToFacebook(WeaponGenerator generator)
        {
            var item = GenerateWeaponWithIcon(generator);
            var provider = new ItemTooltipProvider();
            var ttgen = TooltipGenerators.Resolve(item);
            var tooltip = ttgen.Generate(provider, item, 4f);
            var facebook = App.Provider.GetService<IFacebookClient>();

            var itemName = item.GetName();

            var post = new FacebookPhotoPost();

            post.Caption = itemName;

            post.SetImage($"{itemName}.png", tooltip);

            facebook.Send(post);
        }

        static void PostToTwitter(WeaponGenerator generator)
        {
            var item = GenerateWeaponWithIcon(generator);
            var provider = new ItemTooltipProvider();
            var ttgen = TooltipGenerators.Resolve(item);
            var tooltip = ttgen.Generate(provider, item, 4f);
            var twitter = App.Provider.GetService<ITwitterClient>();

            var tooltipWithBackground = new Image<Rgba32>(tooltip.Width, tooltip.Height);

            tooltipWithBackground.Mutate(x =>
            {
                x.Fill(Rgba32.Black, new Rectangle(0, 0, tooltip.Width, tooltip.Height));
                x.DrawImage(tooltip, 1f);
            });

            var itemName = item.GetName();

            var tweet = new Tweet();

            tweet.Caption = itemName;

            tweet.AddImage($"{itemName}.png", tooltipWithBackground);

            twitter.Send(tweet);
        }

        static void VerifyData()
        {
            var verified = true;

            verified = verified && Directory.Exists(Paths.DataDirectory);
            verified = verified && Directory.Exists(Paths.IconUsage);
            verified = verified && File.Exists(Paths.WeaponTypes);

            if (!verified)
            {
                Directory.CreateDirectory(Paths.DataDirectory);
                var upkeeper = new DataUpkeeper();

                upkeeper.UpdateData();
            }
        }

        static void Configure(string[] inputArgs)
        {
            App = new Configurator().Configure(inputArgs);
            ConfigurePaths();

            App.Args.LogInitialization<Program>(logger);
            App.AddDbContext<AutomizerContext>("Automizer");
            App.AddTwitterClient();
            App.AddFacebookClient();

            App.BuildServiceProvider();
        }

        static void ConfigurePaths()
        {
            var paths = App.Config.GetSection("paths");
            Paths.BlizzardInterfaceArt = paths["BlizzardInterfaceArt"];
            Paths.DatabaseDirectory = paths["DatabaseDirectory"];
            Paths.DataDirectory = paths["DataDirectory"];
        }

        static WeaponGenerator CreateGenerator()
        {
            var db = App.Provider.GetService<AutomizerContext>();
            var generator = new WeaponGenerator();
            var phrases = GetPhrases(db);
            generator.Phrases = new PhraseGenerator(phrases);
            generator.SystemPhrases = new PhraseGenerator(GetSystemPhrases(db));
            generator.PrimaryStats = new StatGenerator(StatType.Primary, phrases.Where(phrase => phrase.IsTagged("primarystat")).ToArray());
            generator.PrimaryStatCount = new CountGenerator(2, 3, 0.5d);
            generator.SecondaryStats = new StatGenerator(StatType.Secondary, phrases.Where(phrase => phrase.IsTagged("secondarystat")));
            generator.SecondaryStatCount = new CountGenerator(0, 3, 0.5d);
            generator.TertiaryStats = new StatGenerator(StatType.Tertiary, phrases.Where(phrase => phrase.IsTagged("secondarystat")));
            generator.TertiaryStatCount = new CountGenerator(0, 2, 0.2d);
            generator.CorruptionStats = new StatGenerator(StatType.Corruption, phrases.Where(phrase => phrase.IsTagged("corruptionstat")));
            generator.CorruptionStatCount = new CountGenerator(0, 1, 0.05d);
            generator.Icon = new EquipmentIconGenerator(Paths.Icons, Paths.IconUsage);
            generator.WeaponType = new WeaponTypeGenerator(GetWeaponTypes(Paths.WeaponTypes));
            generator.Name = new PredefinedGenerator<string>(GetNameFormats(db));
            generator.Equip = new PredefinedGenerator<string>(GetEquipFormats(db));
            generator.Use = new PredefinedGenerator<string>(GetUseFormats(db));
            generator.Corruption = new PredefinedGenerator<string>(GetCorruptionFormats(db));
            generator.Flavor = new PredefinedGenerator<string>(GetFlavorFormats(db));
            return generator;
        }

        static Phrase[] GetPhrases(AutomizerContext db) => GetPhrases(db, "Warcraft Stats", "Default");
        static Phrase[] GetSystemPhrases(AutomizerContext db) => GetPhrases(db, "Warcraft Binds On", "Warcraft Equipment Slots", "Warcraft Other");
        static Phrase[] GetPhrases(AutomizerContext db, params string[] dictionaries)
        {
            var phrases = new List<Phrase>();
            var source = new DbPhrase[0];

            foreach (var dictionary in dictionaries)
            {
                source = db.GetPhrases(dictionary).ToArray();
                foreach (var phrase in source)
                    phrases.Add(phrase.Convert(db));
            }

            return phrases.ToArray();
        }
        static EquipmentTypeDefinition[] GetWeaponTypes(string weaponTypesJsonPath)
        {
            var fileContents = File.ReadAllText(weaponTypesJsonPath);
            var jarray = JArray.Parse(fileContents);
            var definitions = new List<EquipmentTypeDefinition>();
            foreach (var token in jarray)
                definitions.Add(token.ToObject<EquipmentTypeDefinition>());
            return definitions.ToArray();
        }
        static string[] GetNameFormats(AutomizerContext db) => GetFormats(db, "Warcraft Weapon Names");
        static string[] GetEquipFormats(AutomizerContext db) => GetFormats(db, "Warcraft Equips");
        static string[] GetUseFormats(AutomizerContext db) => GetFormats(db, "Warcraft Uses");
        static string[] GetCorruptionFormats(AutomizerContext db) => GetFormats(db, "Warcraft Corruptions");
        static string[] GetFlavorFormats(AutomizerContext db) => GetFormats(db, "Warcraft Flavors");
        static JArray FormatData = null;
        static string[] GetFormats(AutomizerContext db, params string[] categories)
        {
            return categories
                .Select(category => db.GetFormats(category)
                .Select(format => new StringCleaner(format.Value).Clean()))
                .SelectMany(format => format)
                .ToArray();
        }
        static IEnumerable<IRNG> GenerateRNGs(int count)
        {
            var rngs = new List<IRNG>();
            var rng = new RNG();
            for (var i = 0; i < count; i++)
                rngs.Add(new RNG(rng.Next()));
            return rngs;
        }
        static IEnumerable<IWeapon> GenerateWeapons(WeaponGenerator generator, IEnumerable<IRNG> rngs)
        {
            return rngs.Select(rng =>
            {
                Core.RNG = rng;
                Core.Generator = generator;
                return generator.Generate(rng);
            });
        }
        static IEnumerable<IWeapon> GenerateWeapons(WeaponGenerator generator, int count)
        {
            return GenerateWeapons(generator, GenerateRNGs(count));
        }
        static IWeapon GenerateWeaponWithIcon(WeaponGenerator generator)
        {
            IWeapon weaponWithIcon = null;
            IEnumerable<IWeapon> weapons;
            while (weaponWithIcon == null)
            {
                logger.LogInformation("No weapons found with icons.");
                weapons = GenerateWeapons(generator, 5);
                weaponWithIcon = weapons.FirstOrDefault(weapon => weapon.GetIcon() != null);
            }
            return weaponWithIcon;
        }
    }
}
