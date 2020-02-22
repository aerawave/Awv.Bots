using Awv.Automation.Lexica;
using Awv.Automizer.Data.Models;
using System.Linq;

namespace Awv.Automizer.Data
{
    public static class DataExtensions
    {
        public static Phrase Convert(this DbPhrase record, AutomizerContext db)
        {
            var tags = db.GetTags(record.Name).Select(tag => tag.Name).ToArray();
            var phrase = new Phrase(record.Name, tags);

            return phrase;
        }
    }
}
