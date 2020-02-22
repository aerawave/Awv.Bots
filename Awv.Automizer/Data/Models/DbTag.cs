using System.ComponentModel.DataAnnotations.Schema;

namespace Awv.Automizer.Data.Models
{
    public class DbTag
    {
        public int Id { get; set; }
        [ForeignKey("ParentId")]
        public DbTag Parent { get; set; }
        public string Name { get; set; }

        public DbTag()
        {
        }

        public DbTag(string name)
            : this()
        {
            Name = name;
        }

        public override string ToString() => Name;
    }
}
