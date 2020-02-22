using System;
using System.Collections.Generic;
using System.Text;

namespace Awv.Automizer.Data.Models
{
    public class DbFormatCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public DbFormatCategory()
        {
        }

        public DbFormatCategory(string name)
            : this()
        {
            Name = name;
        }

        public DbFormatCategory(string name, string description)
            : this()
        {
            Name = name;
            Description = description;
        }

        public override string ToString() => Name;
    }
}
