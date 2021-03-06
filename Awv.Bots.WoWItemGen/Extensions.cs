﻿using Awv.Automation.Generation.Interface;
using Awv.Automation.Lexica;
using Awv.Automation.Lexica.Compositional.Lexigrams;
using Awv.Lexica.Compositional;
using Awv.Lexica.Compositional.Lexigrams;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Awv.Bots.WoWItemGen
{
    public static class Extensions
    {
        public static string ToWoWTime(this TimeSpan time)
        {
            var values = new List<string>();

            if (time.Days > 0)
            {
                var days = (int)(time.TotalDays - (time.TotalDays % 1));
                values.Add($"{days} day{(days == 1 ? "" : "s")}");
            }

            if (time.Hours > 0)
            {
                var hours = time.Hours;
                values.Add($"{hours} day{(hours == 1 ? "" : "s")}");
            }

            if (time.Minutes > 0)
                values.Add($"{time.Minutes} min");

            if (time.Seconds > 0)
                values.Add($"{time.Seconds} sec");

            if (values.Count == 0)
                values.Add("0 sec");

            return string.Join(", ", values);
        }

        public static void ReplaceTags(this Composition composition, IEnumerable<string> tags, PhraseGenerator generator, IRNG random, string defaultValue)
        {
            for (var i = 0; i < composition.Count; i++)
            {
                var child = composition[i];
                if (child is Composition)
                {
                    (child as Composition).ReplaceTags(tags, generator, random, defaultValue);
                }
                else if (child is TagLexigram)
                {
                    var tag = child as TagLexigram;
                    if (tags.Contains(tag.Tag))
                    {
                        composition[i] = new Lexigram(generator.Generate(random)?.ToString() ?? defaultValue);
                    }
                }
            }
        }

        public static void ReplaceTag(this Composition composition, string tag, PhraseGenerator generator, IRNG random, string defaultValue)
            => ReplaceTags(composition, new string[] { tag }, generator, random, defaultValue);

    }
}
