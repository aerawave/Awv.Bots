using System.Collections.Generic;

namespace Awv.Bots
{
    public struct StringCleaner
    {
        private static Dictionary<string, string> Replacers { get; set; } = new Dictionary<string, string>();
        public string Value { get; set; }

        public StringCleaner(string value)
        {
            Value = value;
        }

        private static Dictionary<string, string> GetReplacers()
        {
            if (Replacers.Count > 0) return Replacers;

            Replacers.Add("\\n", "\n");

            return Replacers;
        }

        public string Clean()
        {
            var replacers = GetReplacers();
            var value = Value;

            foreach (var key in replacers.Keys)
                value = value.Replace(key, replacers[key]);

            return value;
        }

        public string Dirty()
        {
            var replacers = GetReplacers();
            var value = Value;

            foreach (var key in replacers.Keys)
                value = value.Replace(replacers[key], key);

            return value;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
