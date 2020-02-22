using Awv.Games.WoW.Data;
using Awv.Games.WoW.Data.Artwork;
using Awv.Games.WoW.Data.Items;
using SixLabors.ImageSharp;
using System.IO;

namespace Awv.Bots.WoWItemGen.DataUpkeep
{
    public class DataUpkeeper
    {
        public void UpdateData()
        {
            var db = CreateItemDatabase(Paths.DatabaseDirectory);

            db.ExportIconUsage(Paths.IconUsage, ExportFormat.PlainText);

            var json = db.GenerateWeaponTypesJson();

            using var jsonFile = File.Open(Paths.WeaponTypes, FileMode.Create);
            using var writer = new StreamWriter(jsonFile);

            writer.Write(json.ToString());
        }

        public void UpdateArtwork()
        {
            var updater = new ArtworkUpdater("updater.json", Paths.BLP_BlizzardInterfaceArt, Paths.BlizzardInterfaceArt);
            updater.Start();
        }

        private ItemDatabase CreateItemDatabase(string databaseDirectory)
        {
            var database = new ItemDatabase();
            database.ListFile.Load(Path.Combine(databaseDirectory, "listfile.csv"));
            database.ItemClasses.Load(Path.Combine(databaseDirectory, "itemclass.csv"));
            database.ItemSubClasses.Load(Path.Combine(databaseDirectory, "itemsubclass.csv"));
            database.Items.Load(Path.Combine(databaseDirectory, "item.csv"));
            database.ItemSearchNames.Load(Path.Combine(databaseDirectory, "itemsearchname.csv"));
            database.AssociateRecords();
            return database;
        }
    }
}
