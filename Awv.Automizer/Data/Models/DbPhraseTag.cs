using System.ComponentModel.DataAnnotations.Schema;

namespace Awv.Automizer.Data.Models
{
    public class DbPhraseTag
    {
        [ForeignKey("PhraseId")]
        public int PhraseId { get; set; }
        [ForeignKey("TagId")]
        public int TagId { get; set; }

        public DbPhraseTag() { }

        public DbPhraseTag(int phraseId, int tagId)
        {
            PhraseId = phraseId;
            TagId = tagId;
        }
    }
}
