using System.ComponentModel.DataAnnotations.Schema;

namespace Awv.Automizer.Data.Models
{
    public class DbFormat
    {
        public int Id { get; set; }
        public string Value { get; set; }
        [ForeignKey("CategoryId")]
        public DbFormatCategory Category { get; set; }

        public override string ToString() => Value;
    }
}
