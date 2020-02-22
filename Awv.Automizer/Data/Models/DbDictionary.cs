using System;
using System.Collections.Generic;
using System.Text;

namespace Awv.Automizer.Data.Models
{
    public class DbDictionary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public DbDictionary()
        {
        }

        public DbDictionary(string name)
            : this()
        {
            Name = name;
        }

        public DbDictionary(string name, string description)
            : this()
        {
            Name = name;
            Description = description;
        }

        public override string ToString() => Name;
    }
}
