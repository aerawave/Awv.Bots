using Awv.Automizer.Data;
using Awv.Bots.Logging;
using Awv.Bots.Runner;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Awv.Lexica.Compositional;
using Awv.Automation.LexicaOld;
using Awv.Lexica.Compositional.Lexigrams;
using Awv.Automation.Lexica.Compositional.Lexigrams;
using Awv.Lexica.Compositional.Lexigrams.Interface;
using Awv.Automation.LexicaOld.Interface;
using Awv.Automation.Lexica.Compositional;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Awv.Bots.LocalClient
{
    class Program
    {
        static Configurator App { get; set; }
        static ILogger logger { get; set; } = new AwvLogFactory().CreateLogger<Program>();
        static void Main(string[] args)
        {
            Configure(args);
            UpdateDatabase();
        }

        static void Configure(string[] inputArgs)
        {
            App = new Configurator().Configure(inputArgs);

            App.Args.LogInitialization<Program>(logger);
            App.AddDbContext<AutomizerContext>("Automizer");

            App.BuildServiceProvider();
        }

        static void UpdateDatabase()
        {
            var db = App.Provider.GetService<AutomizerContext>();
            var categories = db.FormatCategories.ToArray();

            foreach (var category in categories)
            {
                var formats = db.GetFormats(category.Name).ToArray();
                var formatValues = new JArray();
                foreach(var format in formats)
                {
                    var concept = new ConceptParser($"{{{new StringCleaner(format.Value).Clean()}}}").ParseConcept() as Concept;
                    var comp = Convert(concept);

                    format.Value = comp.ToString();
                }
                db.SaveChanges();
            }
        }

        static ILexigram Convert(IConcept target)
        {
            if (target is Phrase) return Convert(target as Phrase);
            else if (target is Concept) return Convert(target as Concept);
            return null;
        }

        static ILexigram Convert(Phrase phrase)
        {
            ILexigram lexigram = null;
            var value = phrase.ActualWord;
            switch (phrase.Type)
            {
                case PhraseType.Phrase:
                    lexigram = new Lexigram(new StringCleaner(phrase.ActualWord).Dirty());
                    break;
                case PhraseType.NumberGenerator:
                    var range = value.Split('-');
                    var min = int.Parse(range[0]);
                    var max = int.Parse(range[1]);
                    lexigram = new CodeLexigram(null, $"ri({min},{max})");
                    break;
                case PhraseType.Tag:
                    lexigram = new TagLexigram(null, value);
                    break;
            }

            if (phrase.Chance < 1d)
            {
                var sublex = new ConditionalLexigram();
                sublex.Add(lexigram);
                sublex.ChanceCode.Code = phrase.Chance.ToString();
                lexigram = sublex;
            }
            return lexigram;
        }

        static Composition Convert(Concept concept)
        {
            if (concept.Chance < 1d) return ConvertConditional(concept);
            else return ConvertStandard(concept);
        }

        static ConditionalLexigram ConvertConditional(Concept concept)
        {
            var comp = new ConditionalLexigram(ConvertStandard(concept));
            comp.ChanceCode.Code = concept.Chance.ToString();
            return comp;
        }

        private static AutomationParser parser = new AutomationParser("#");
        static Composition ConvertStandard(Concept concept)
        {
            var comp = new Composition();

            foreach (var sub in concept.Concepts)
            {
                var last = comp.LastOrDefault();
                var next = Convert(sub);

                if (last is TagLexigram && next is Lexigram)
                {
                    var lexigram = next as Lexigram;
                    if (lexigram.Value.Length > 0)
                    {
                        var ch = lexigram.Value[0];
                        if (!parser.GetTagBreakers().Contains(ch) && (AutomationParser.TagValidChars.Contains(ch) || char.IsLetterOrDigit(ch)))
                        {
                            lexigram.Value = $"\\{lexigram.Value}";
                        }
                    }
                }
                comp.Add(next);
            }

            return comp;
        }
    }
}
