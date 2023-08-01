using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Vintagestory.API.Common;

namespace rpgitemrarity.ModConfig {
    public class ModConfig {
        //How probability works:
        //An item generates with a rarity value (e.g. rarity = 0.87).
        //The game then runs through the list to see what rarity it is.
        //For example:
        //Unique is set to 0.99, 0.87 is lesser, so continue.
        //Legendary is set 0.95, 0.87 is lesser, so continue.
        //Epic is set to 0.85, 0.87 is greater, stop at rare.
        //
        //Lesser raritys should always have smaller values.
        //If all raritys is set above zero, regular items can generate (useful if common has a modifier).
        public Dictionary<string, float> ItemRarityProbablility = new Dictionary<string, float>
            {
                {"Unique"   , 0.99f},
                {"Legendary", 0.95f},
                {"Epic"     , 0.85f},
                {"Rare"     , 0.75f},
                {"Uncommon" , 0.50f},
                {"Common"   , 0.00f}
            };
        //How modifier works:
        //After a rarity is applied, this is the multiplier for the stats.
        //For example:
        //Item has a default durability of 10.
        //Unique has a multiplier of 1.5.
        //So the Unique Item would have 15 durability. (10 * 1.5).
        public Dictionary<string, float> ItemRarityModifier = new Dictionary<string, float>
            {
                {"Unique"   , 1.50f},
                {"Legendary", 1.40f},
                {"Epic"     , 1.30f},
                {"Rare"     , 1.20f},
                {"Uncommon" , 1.10f},
                {"Common"   , 1.00f}
            };
        //How color works:
        //It's a hex value. Just use something like:
        //https://colors-picker.com/hex-color-picker/
        public Dictionary<string, string> ItemRarityColor = new Dictionary<string, string>
            {
                {"Unique"   , "#ff0d00"},
                {"Legendary", "#ffd700"},
                {"Epic"     , "#9F63FF"},
                {"Rare"     , "#00DAFE"},
                {"Uncommon" , "#48ff00"},
                {"Common"   , "#bbbbbb"}
            };
        //If this is set, the below values can modify rarity names.
        public bool OverrideLanguageFiles = false;
        //This is what is prefixed before an item after it gets a rarity.
        //Setting these won't do anything unless you enable OverrideLanguageFiles.
        //By default, the game uses the language files for localization.
        public Dictionary<string, string> ItemRarityName = new Dictionary<string, string>
            {
                 {"Unique"   , "Unique"   },
                 {"Legendary", "Legendary"},
                 {"Epic"     , "Epic"     },
                 {"Rare"     , "Rare"     },
                 {"Uncommon" , "Uncommon" },
                 {"Common"   , "Common"   }
            };
        //Enable/Disable XSkills Compatability. Enabled by default because there isn't a real reason not to.
        //Does nothing without XSkills.
        //If you run into other mods that use "quality" attributes on itemstacks, you may want to set this to false.
        public bool XSkillsCompatability = true;
        public ModConfig() { }

        public static ModConfig Current { get; set; }

        public static ModConfig GetDefault()
        {
            ModConfig defaultConfig = new ModConfig();
            return defaultConfig;
        }
    }   
}

