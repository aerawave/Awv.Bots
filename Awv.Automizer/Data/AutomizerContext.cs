using Awv.Automizer.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Awv.Automizer.Data
{
    public class AutomizerContext : DbContext
    {
        public AutomizerContext(DbContextOptions options)
            : base(options) { }

        public AutomizerContext(string connectionString)
            : this (GetOptionsFromConnectionString(connectionString))
        {
        }

        private static DbContextOptions GetOptionsFromConnectionString(string connectionString)
        {
            var options = new DbContextOptionsBuilder<AutomizerContext>();
            options.UseSqlServer(connectionString);
            return options.Options;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<DbPhraseTag>()
                .HasKey(pt => new { pt.PhraseId, pt.TagId });
        }

        #region Phrase Procedures
        public DbPhrase GetPhrase(string name)
        {
            return Phrases.FromSqlRaw("SELECT * FROM [dbo].[GetPhrase]({0})", name).Include(phrase => phrase.Dictionary).AsEnumerable().FirstOrDefault();
        }
        public DbPhrase CreatePhrase(string name, string description)
        {
            if (description == null)
                return Phrases.FromSqlRaw("EXEC CreatePhrase {0}", name).AsEnumerable().FirstOrDefault();
            else
                return Phrases.FromSqlRaw("EXEC CreatePhrase {0}, {1}", name, description).AsEnumerable().FirstOrDefault();
        }
        public DbTag[] GetTags(string phrase)
        {
            return Tags.FromSqlRaw("EXEC GetTags {0}", phrase).ToArray();
        }
        public void UpdateTags(string phrase, params string[] tags)
        {
            Database.ExecuteSqlRaw("EXEC ClearTags {0}", phrase);
            foreach (var tag in tags)
            {
                if (GetTag(tag) == null) CreateTag(tag, null);
                Database.ExecuteSqlRaw("EXEC AddTag {0}, {1}", phrase, tag);
            }
        }
        #endregion
        #region Tag Procedures
        public DbTag GetTag(string tag)
        {
            return Tags.FromSqlRaw("SELECT * FROM [dbo].[GetTag]({0})", tag).AsEnumerable().FirstOrDefault();
        }
        public DbTag CreateTag(string tag, string parentTag)
        {
            if (parentTag == null)
                return Tags.FromSqlRaw("EXEC CreateTag {0}", tag).AsEnumerable().FirstOrDefault();
            else
                return Tags.FromSqlRaw("EXEC CreateTag {0}, {1}", tag, parentTag).AsEnumerable().FirstOrDefault();
        }
        #endregion
        #region Tags Procedures
        public DbTag[] GetTagsUnderParent(string name)
        {
            return Tags.FromSqlRaw("EXEC GetTagsUnderParent {0}", name).AsEnumerable().ToArray();
        }
        #endregion
        #region Dictionary Procedures
        public DbDictionary GetDictionary(string name)
        {
            return Dictionaries.FromSqlRaw("SELECT * FROM [dbo].[GetDictionary]({0})", name).AsEnumerable().FirstOrDefault();
        }
        public DbDictionary CreateDictionary(string name, string description)
        {
            if (description == null)
                return Dictionaries.FromSqlRaw("EXEC CreateDictionary {0}", name).AsEnumerable().FirstOrDefault();
            else
                return Dictionaries.FromSqlRaw("EXEC CreateDictionary {0}, {1}", name, description).AsEnumerable().FirstOrDefault();
        }
        public DbPhrase[] GetPhrases(string dictionary)
        {
            return Phrases.FromSqlRaw("EXEC GetPhrases {0}", dictionary).AsEnumerable().ToArray();
        }
        public void AddPhrases(string dictionary, params string[] phrases)
        {
            foreach (var phrase in phrases)
                Database.ExecuteSqlRaw("EXEC AddPhrase {0}, {1}", dictionary, phrase);
        }
        public void UpdatePhrases(string dictionary, params string[] phrases)
        {
            Database.ExecuteSqlRaw("EXEC ClearPhrases {0}", dictionary);
            foreach (var phrase in phrases)
            {
                if (GetPhrase(phrase) == null) CreatePhrase(phrase, null);
                Database.ExecuteSqlRaw("EXEC AddPhrase {0}, {1}", dictionary, phrase);
            }
        }
        public void DeleteDictionary(string dictionaryName)
        {
            var dictionary = GetDictionary(dictionaryName);
            var phrases = GetPhrases(dictionary.Name);
            foreach (var phrase in phrases)
                phrase.Dictionary = null;
            Dictionaries.Remove(dictionary);
            SaveChanges();
        }
        #endregion
        #region Format Procedures
        public DbFormatCategory CreateFormatCategory(string name, string description)
        {
            if (description == null)
                return FormatCategories.FromSqlRaw("EXEC CreateFormatCategory {0}", name).AsEnumerable().FirstOrDefault();
            else
                return FormatCategories.FromSqlRaw("EXEC CreateFormatCategory {0}, {1}", name, description).AsEnumerable().FirstOrDefault();
        }
        public DbFormatCategory GetFormatCategory(string name)
        {
            return FormatCategories.FromSqlRaw("EXEC GetFormatCategory {0}", name).AsEnumerable().FirstOrDefault();
        }
        public void UpdateFormats(string category, params int[] formatIds)
        {
            var deletes = GetFormats(category).Where(format => !formatIds.Contains(format.Id));
            Database.ExecuteSqlRaw("EXEC ClearFormats {0}", category);
            foreach(var formatId in formatIds)
            {
                Database.ExecuteSqlRaw("EXEC AddFormat {0}, {1}", category, formatId);
            }
            foreach (var format in deletes)
                Formats.Remove(format);
            SaveChanges();
        }
        public DbFormat CreateFormat(string formatString, string category)
        {
            var format = Formats.FromSqlRaw("EXEC CreateFormat {0}, {1}", formatString, category).AsEnumerable().FirstOrDefault();
            return GetFormat(format.Id);
        }
        public IEnumerable<DbFormat> GetFormats(string category)
        {
            return Formats.FromSqlRaw("EXEC GetFormats {0}", category).AsEnumerable().ToArray();
        }
        public DbFormat GetFormat(int id)
        {
            return Formats.FromSqlRaw("SELECT * FROM [dbo].[GetFormat]({0})", id).Include(format => format.Category).AsEnumerable().FirstOrDefault();
        }
        #endregion

        public DbSet<DbTag> Tags { get; set; }
        public DbSet<DbPhrase> Phrases { get; set; }
        public DbSet<DbPhraseTag> PhraseTags { get; set; }
        public DbSet<DbDictionary> Dictionaries { get; set; }
        public DbSet<DbFormatCategory> FormatCategories { get; set; }
        public DbSet<DbFormat> Formats { get; set; }
    }
}
