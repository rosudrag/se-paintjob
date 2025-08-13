using System.Collections.Generic;
using PaintJob.App.Models;

namespace PaintJob.App.Constants
{
    public static class StyleVariants
    {
        public const string DEFAULT_VARIANT = "Default";
        public const string MILITARY_STANDARD = "Standard";
        public const string MILITARY_STEALTH = "Stealth";
        public const string MILITARY_ASTEROID = "Asteroid";
        public const string MILITARY_INDUSTRIAL = "Industrial";
        public const string MILITARY_DEEP_SPACE = "Deep_Space";
        public const string RACING_FORMULA1 = "Formula1";
        public const string RACING_RALLY = "Rally";
        public const string RACING_STREET = "Street";
        public const string RACING_DRAG = "Drag";
        public const string RACING_ENDURANCE = "Endurance";
        public const string PIRATE_SKULL = "Skull";
        public const string PIRATE_KRAKEN = "Kraken";
        public const string PIRATE_BLACKBEARD = "Blackbeard";
        public const string PIRATE_CORSAIR = "Corsair";
        public const string PIRATE_MARAUDER = "Marauder";
        public const string CORPORATE_MINING = "Mining_Corp";
        public const string CORPORATE_TRANSPORT = "Transport_Co";
        public const string CORPORATE_SECURITY = "Security_Firm";
        public const string CORPORATE_RESEARCH = "Research_Lab";
        public const string CORPORATE_CONSTRUCTION = "Construction";
        public const string ALIEN_ORGANIC = "Organic";
        public const string ALIEN_CRYSTALLINE = "Crystalline";
        public const string ALIEN_TECHNO_ORGANIC = "Techno_Organic";
        public const string ALIEN_HIVE = "Hive";
        public const string ALIEN_ANCIENT = "Ancient";
        public const string RETRO_80S_NEON = "80s_Neon";
        public const string RETRO_50S_CHROME = "50s_Chrome";
        public const string RETRO_70S_DISCO = "70s_Disco";
        public const string RETRO_90S_CYBER = "90s_Cyber";
        public const string RETRO_ART_DECO = "Art_Deco";
        
        private static readonly Dictionary<Style, List<string>> _styleVariantMap = new Dictionary<Style, List<string>>
        {
            { Style.Rudimentary, new List<string> { DEFAULT_VARIANT } },
            { Style.Military, new List<string> { MILITARY_STANDARD, MILITARY_STEALTH, MILITARY_ASTEROID, MILITARY_INDUSTRIAL, MILITARY_DEEP_SPACE } },
            { Style.Racing, new List<string> { RACING_FORMULA1, RACING_RALLY, RACING_STREET, RACING_DRAG, RACING_ENDURANCE } },
            { Style.Pirate, new List<string> { PIRATE_SKULL, PIRATE_KRAKEN, PIRATE_BLACKBEARD, PIRATE_CORSAIR, PIRATE_MARAUDER } },
            { Style.Corporate, new List<string> { CORPORATE_MINING, CORPORATE_TRANSPORT, CORPORATE_SECURITY, CORPORATE_RESEARCH, CORPORATE_CONSTRUCTION } },
            { Style.Alien, new List<string> { ALIEN_ORGANIC, ALIEN_CRYSTALLINE, ALIEN_TECHNO_ORGANIC, ALIEN_HIVE, ALIEN_ANCIENT } },
            { Style.Retro, new List<string> { RETRO_80S_NEON, RETRO_50S_CHROME, RETRO_70S_DISCO, RETRO_90S_CYBER, RETRO_ART_DECO } }
        };
        
        public static Dictionary<Style, List<string>> GetStyleVariantMap()
        {
            return new Dictionary<Style, List<string>>(_styleVariantMap);
        }
        
        public static List<string> GetVariantsForStyle(Style style)
        {
            return _styleVariantMap.TryGetValue(style, out var variants) 
                ? new List<string>(variants) 
                : new List<string> { DEFAULT_VARIANT };
        }
        
        public static bool IsValidVariant(Style style, string variant)
        {
            return _styleVariantMap.TryGetValue(style, out var variants) && variants.Contains(variant);
        }
    }
}