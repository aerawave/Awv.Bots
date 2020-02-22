using System.IO;

namespace Awv.Bots.WoWItemGen
{
    public static class Paths
    {
        public static string TooltipFills = "Assets/Tooltips";

        public static string BLP_BlizzardInterfaceArt = null;

        public static string BlizzardInterfaceArt = null;
        public static string Interface => Path.Combine(BlizzardInterfaceArt, "Interface");
        public static string Icons => Path.Combine(Interface, "ICONS");
        public static string Tooltips => Path.Combine(Interface, "Tooltips");

        public static string DataDirectory = null;
        public static string IconUsage => Path.Combine(DataDirectory, "Icon Usage");
        public static string WeaponTypes => Path.Combine(DataDirectory, "weapon-types.json");

        public static string DatabaseDirectory = null;
    }
}
