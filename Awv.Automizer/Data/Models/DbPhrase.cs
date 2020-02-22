using System.ComponentModel.DataAnnotations.Schema;

namespace Awv.Automizer.Data.Models
{
    public class DbPhrase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [ForeignKey("DictionaryId")]
        public DbDictionary Dictionary { get; set; }

        public DbPhrase()
        {
        }

        public DbPhrase(string name)
            : this()
        {
            Name = name;
        }

        public DbPhrase(string name, string description)
            : this()
        {
            Name = name;
            Description = description;
        }

        public override string ToString() => Name;
    }
}
