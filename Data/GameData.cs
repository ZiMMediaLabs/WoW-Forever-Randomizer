using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WoWForeverRandomizer.Data
{
    public enum Faction
    {
        Alliance,
        Horde
    }

    public enum FactionChoice
    {
        Alliance,
        Horde,
        Either
    }

    public class WowClass
    {
        public WowClass(string name, Color classColor)
        {
            Name = name;
            ClassColor = classColor;
        }

        public string Name { get; }

        public Color ClassColor { get; }

        public string IconPath => "Images/Classes/" + Name + ".png";
    }

    public class WowRace
    {
        public WowRace(string name, string subtitle, Faction faction, string portraitKey, params WowClass[] allowedClasses)
        {
            Name = name;
            Subtitle = subtitle;
            Faction = faction;
            PortraitKey = portraitKey;
            AllowedClasses = allowedClasses.ToList().AsReadOnly();
        }

        public string Name { get; }

        // Skyborne only: "High Order" (Alliance) or "Windshaper" (Horde)
        public string Subtitle { get; }

        public Faction Faction { get; }

        public string PortraitKey { get; }

        public IReadOnlyList<WowClass> AllowedClasses { get; }

        public string GetPortraitPath(bool female)
        {
            return "Images/Races/" + PortraitKey + (female ? "_Female.png" : "_Male.png");
        }
    }

    // WoW Forever beta roster (Oct 2026).
    // Skyborne is split by faction: High Order (Alliance) can be Mage, Windshaper (Horde) can be Shaman.
    public static class GameData
    {
        #region Classes

        // Official in-game class colors
        public static readonly WowClass Druid = new WowClass("Druid", ColorTranslator.FromHtml("#FF7C0A"));
        public static readonly WowClass Hunter = new WowClass("Hunter", ColorTranslator.FromHtml("#AAD372"));
        public static readonly WowClass Mage = new WowClass("Mage", ColorTranslator.FromHtml("#3FC7EB"));
        public static readonly WowClass Paladin = new WowClass("Paladin", ColorTranslator.FromHtml("#F48CBA"));
        public static readonly WowClass Priest = new WowClass("Priest", ColorTranslator.FromHtml("#FFFFFF"));
        public static readonly WowClass Rogue = new WowClass("Rogue", ColorTranslator.FromHtml("#FFF468"));
        public static readonly WowClass Shaman = new WowClass("Shaman", ColorTranslator.FromHtml("#0070DD"));
        public static readonly WowClass Warlock = new WowClass("Warlock", ColorTranslator.FromHtml("#8788EE"));
        public static readonly WowClass Warrior = new WowClass("Warrior", ColorTranslator.FromHtml("#C69B6D"));

        public static readonly IReadOnlyList<WowClass> AllClasses = new List<WowClass>
        {
            Druid, Hunter, Mage, Paladin, Priest, Rogue, Shaman, Warlock, Warrior
        }.AsReadOnly();

        #endregion

        #region Races

        public static readonly IReadOnlyList<WowRace> AllRaces = new List<WowRace>
        {
            // Alliance
            new WowRace("Human", null, Faction.Alliance, "Human", Hunter, Mage, Paladin, Priest, Rogue, Warlock, Warrior),
            new WowRace("Dwarf", null, Faction.Alliance, "Dwarf", Hunter, Paladin, Priest, Rogue, Shaman, Warrior),
            new WowRace("Night Elf", null, Faction.Alliance, "NightElf", Druid, Hunter, Priest, Rogue, Warrior),
            new WowRace("Gnome", null, Faction.Alliance, "Gnome", Mage, Priest, Rogue, Warlock, Warrior),
            new WowRace("Skyborne", "High Order", Faction.Alliance, "Skyborne", Druid, Hunter, Mage, Rogue, Warrior),

            // Horde
            new WowRace("Orc", null, Faction.Horde, "Orc", Hunter, Mage, Rogue, Shaman, Warlock, Warrior),
            new WowRace("Undead", null, Faction.Horde, "Undead", Mage, Paladin, Priest, Rogue, Warlock, Warrior),
            new WowRace("Tauren", null, Faction.Horde, "Tauren", Druid, Hunter, Shaman, Warrior),
            new WowRace("Troll", null, Faction.Horde, "Troll", Hunter, Mage, Priest, Rogue, Shaman, Warlock, Warrior),
            new WowRace("Skyborne", "Windshaper", Faction.Horde, "Skyborne", Druid, Hunter, Rogue, Shaman, Warrior),
        }.AsReadOnly();

        #endregion

        public static List<WowRace> GetRaces(FactionChoice choice)
        {
            if (choice == FactionChoice.Either)
            {
                return AllRaces.ToList();
            }

            Faction faction = choice == FactionChoice.Alliance ? Faction.Alliance : Faction.Horde;
            return AllRaces.Where(race => race.Faction == faction).ToList();
        }
    }
}
