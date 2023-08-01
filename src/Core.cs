using HarmonyLib;
using rpgitemrarity;
using rpgitemrarity.ModConfig;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using vsrpgrarity.src.patch;

namespace vsrpgrarity.src
{
    [HarmonyPatch]
    public sealed class vsrpgrarityMod : ModSystem
    {
        private readonly Harmony harmonyInstance;
        private static ModConfig config;
        public const string harmonyID = "vsrpgrarity.Patches";
        public vsrpgrarityMod()
        {
            harmonyInstance = new Harmony(harmonyID);
            harmonyInstance.PatchAll();
        }
        public override void Start(ICoreAPI iapi)
        {
            config = new ModConfig();
            harmonyInstance.PatchAll();
            iapi.Event.OnEntitySpawn += OnEntitySpawn;
            base.Start(iapi);
        }
        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
        }
        private void OnEntitySpawn(Entity spawnedEntity)
        {
            //This is redundant but may fix lag issues idk.
            if (spawnedEntity is EntityItem)
            {
                vrpgrarityUpdateItemRarityEntity(spawnedEntity);
            }
        }
        public void vrpgrarityUpdateItemRarityEntity(Entity spawnedEntity)
        {
            if (spawnedEntity is EntityItem)
            {
                //Since we confirm it's an item, set it as an item.
                EntityItem item = (EntityItem)spawnedEntity;
                //Get the itemstack.
                ItemStack itemstack = item.Itemstack;
                //Ensure its an item.

                vrpgrarityUpdateItemRarityItemStack(itemstack);
            }
        }
        public static void vrpgrarityUpdateItemRarityItemStack(ItemStack itemstack)
        {
            Random rng = new Random();

            if (itemstack != null)
            {
                //If not a block.
                if (itemstack.Block == null && itemstack.Item.MaxStackSize <= 1)
                {
                    if (itemstack.Collectible != null && itemstack.Collectible.Attributes != null)
                    {
                        //If it isn't already graded, grade it.
                        if (!(itemstack.Attributes.HasAttribute("rarity")))
                        {
                            if(ModConfig.Current.XSkillsCompatability)
                            {
                                //Create the value and set it.
                                //[XSKILLS] If the object has a "quality" value from XSKILLS (or another mod I suppose).
                                if (itemstack.Attributes.HasAttribute("quality"))
                                {
                                    //[XSKILLS] Do some math to make the 0-10 value of quality to 0-1.
                                    itemstack.Attributes.SetFloat("rarity", itemstack.Attributes.GetFloat("quality") / 10f);
                                }
                                else
                                {
                                    //Assign a normal random rarity value.
                                    itemstack.Attributes.SetFloat("rarity", (float)rng.NextDouble());
                                }
                            }
                            else
                            {
                                //Assign a normal random rarity value.
                                itemstack.Attributes.SetFloat("rarity", (float)rng.NextDouble());
                            }
                            //Get modifier amount.
                            float rarityMod = getrarityModifier(itemstack.Attributes.GetFloat("rarity"));
                            //Update Stats
                            //Thrown weapon.
                            if (itemstack.Collectible.Attributes["damage"].Exists)
                            {
                                float damage = itemstack.Collectible.Attributes["damage"].AsFloat();
                                damage = (float)Math.Round(damage * rarityMod);
                                itemstack.Attributes.SetFloat("damage", damage);
                            }
                            //Non thrown weapon
                            if (itemstack.Collectible.AttackPower > 0.5f)
                            {
                                float attackpower = itemstack.Collectible.AttackPower;
                                attackpower = attackpower * rarityMod;
                                itemstack.Attributes.SetFloat("attackpower", attackpower);
                            }
                            //Can Break
                            if (itemstack.Collectible.Durability != 0)
                            {
                                int durability = itemstack.Collectible.Durability;
                                durability = (int)Math.Round(durability * rarityMod);
                                itemstack.Attributes.SetInt("maxdurability", durability);
                            }
                            //Mining Speed
                            if (itemstack.Collectible.MiningSpeed != null && itemstack.Collectible.MiningSpeed != null)
                            {
                                //Dictionary<EnumBlockMaterial, float> modMiningSpeed = new Dictionary<EnumBlockMaterial, float>();
                                itemstack.Attributes.GetOrAddTreeAttribute("miningspeed");
                                foreach (var val in itemstack.Collectible.MiningSpeed)
                                {
                                    itemstack.Attributes.GetTreeAttribute("miningspeed").SetFloat(val.Key.ToString(), val.Value * rarityMod);
                                }
                            }
                            //Update Shield
                            if (itemstack.Collectible.Attributes["shield"].Exists)
                            {
                                //would loop, but it only has two values and its unlikely someone needs to add more.
                                //And, Can't make Collectible.Attributes["shield"]["protectionChance"] into a dictionary.

                                itemstack.Attributes.GetOrAddTreeAttribute("shield");
                                //Protection Chance
                                // Disabled because it is OP (can go over 100% easily as-is). And it doesn't rly make sense.
                                itemstack.Attributes.GetTreeAttribute("shield").GetOrAddTreeAttribute("protectionChance");
                                itemstack.Attributes.GetTreeAttribute("shield").GetTreeAttribute("protectionChance").SetFloat("passive",
                                    itemstack.Collectible.Attributes["shield"]["protectionChance"]["passive"].AsFloat(0));// * rarityMod);
                                itemstack.Attributes.GetTreeAttribute("shield").GetTreeAttribute("protectionChance").SetFloat("active",
                                    itemstack.Collectible.Attributes["shield"]["protectionChance"]["active"].AsFloat(0));// * rarityMod);

                                //DamageAbsorption
                                itemstack.Attributes.GetTreeAttribute("shield").GetOrAddTreeAttribute("damageAbsorption");
                                itemstack.Attributes.GetTreeAttribute("shield").GetTreeAttribute("damageAbsorption").SetFloat("passive",
                                    itemstack.Collectible.Attributes["shield"]["damageAbsorption"]["passive"].AsFloat(0) * rarityMod);
                                itemstack.Attributes.GetTreeAttribute("shield").GetTreeAttribute("damageAbsorption").SetFloat("active",
                                    itemstack.Collectible.Attributes["shield"]["damageAbsorption"]["active"].AsFloat(0) * rarityMod);
                            }
                            //Update Armor
                            if (itemstack.Collectible.Attributes["protectionModifiers"].Exists)
                            {
                                ProtectionModifiers protMod = itemstack.Collectible.Attributes?["protectionModifiers"].AsObject<ProtectionModifiers>();
                                itemstack.Attributes.GetOrAddTreeAttribute("protectionModifiers");
                                itemstack.Attributes.GetTreeAttribute("protectionModifiers").SetFloat("flatDamageReduction", protMod.FlatDamageReduction * rarityMod);
                                //itemstack.Attributes.GetTreeAttribute("protectionModifiers").SetBool("highDamageTierResistant", protMod.HighDamageTierResistant);
                                //itemstack.Attributes.GetTreeAttribute("protectionModifiers").SetFloat("perTierFlatDamageReductionLoss"); //These r arrays. how do set.
                                //itemstack.Attributes.GetTreeAttribute("protectionModifiers").SetFloat("perTierRelativeProtectionLoss", protMod.PerTierRelativeProtectionLoss); //These r arrays. how do set.
                                //itemstack.Attributes.GetTreeAttribute("protectionModifiers").SetFloat("protectionTier", protMod.ProtectionTier);
                                //itemstack.Attributes.GetTreeAttribute("protectionModifiers").SetFloat("relativeProtection", protMod.RelativeProtection * rarityMod);
                            }
                            /*
                            JsonObject jsonObj = itemstack.Collectible.Attributes?["statModifiers"];
                                 StatModifiers statModifiers = itemstack.Collectible.Attributes?["statModifiers"].AsObject<StatModifiers>();
                          if (jsonObj?.Exists == true)
                            {
                                api.Logger.Notification("HAS MODIFIERS!");
                                 api.Logger.Notification(statModifiers.healingeffectivness.ToString());
                                api.Logger.Notification(statModifiers.hungerrate.ToString());
                                api.Logger.Notification(statModifiers.rangedWeaponsAcc.ToString());
                                api.Logger.Notification(statModifiers.rangedWeaponsSpeed.ToString());
                                api.Logger.Notification(statModifiers.walkSpeed.ToString());
                                api.Logger.Notification(statModifiers.canEat.ToString());
                            }*/
                            if (itemstack.Collectible.Attributes?["warmth"].Exists == true)
                            {
                                if (itemstack.Collectible.Attributes["warmth"].AsFloat() > 0) itemstack.Attributes.SetFloat("warmth", itemstack.Collectible.Attributes["warmth"].AsFloat() * rarityMod);
                            }
                        }
                    }
                }
            }
        }
        //Internal stuff.
        public static string rarityToString(float rarity)
        {
            bool name = !ModConfig.Current.OverrideLanguageFiles;
            if (rarity >= ModConfig.Current.ItemRarityProbablility["Unique"]) //0% chance
            {
                return name ? Lang.Get("rpgitemrarity:prefix-unique") : config.ItemRarityName["Unique"]; //Red
            }
            else if (rarity >= ModConfig.Current.ItemRarityProbablility["Legendary"]) //5% chance
            {
                return name ? Lang.Get("rpgitemrarity:prefix-legendary") : ModConfig.Current.ItemRarityName["Legendary"]; //Gold
            }
            else if (rarity >= ModConfig.Current.ItemRarityProbablility["Epic"]) //10% chance
            {
                return name ? Lang.Get("rpgitemrarity:prefix-epic") : ModConfig.Current.ItemRarityName["Epic"]; //Violet
            }
            else if (rarity >= ModConfig.Current.ItemRarityProbablility["Rare"]) //15% chance
            {
                return name ? Lang.Get("rpgitemrarity:prefix-rare") : ModConfig.Current.ItemRarityName["Rare"]; //Blue
            }
            else if (rarity >= ModConfig.Current.ItemRarityProbablility["Uncommon"]) //20% chance
            {
                return name ? Lang.Get("rpgitemrarity:prefix-uncommon") : ModConfig.Current.ItemRarityName["Uncommon"]; //Green
            }
            else if (rarity >= ModConfig.Current.ItemRarityProbablility["Common"])//50% chance
            {
                return name ? Lang.Get("rpgitemrarity:prefix-common") : ModConfig.Current.ItemRarityName["Common"]; //Grey
            }
            return "";
        }
        public static string rarityColorToString(float rarity)
        {
            if (rarity >= ModConfig.Current.ItemRarityProbablility["Unique"]) //0% chance
            {
                return ModConfig.Current.ItemRarityColor["Unique"]; //Red #ff0d00
            }
            else if (rarity >= ModConfig.Current.ItemRarityProbablility["Legendary"]) //5% chance
            {
                return ModConfig.Current.ItemRarityColor["Legendary"]; //Gold #ffd700
            }
            else if (rarity >= ModConfig.Current.ItemRarityProbablility["Epic"]) //10% chance
            {
                return ModConfig.Current.ItemRarityColor["Epic"]; //Violet #9F63FF
            }
            else if (rarity >= ModConfig.Current.ItemRarityProbablility["Rare"]) //15% chance
            {
                return ModConfig.Current.ItemRarityColor["Rare"]; //Blue #00DAFE
            }
            else if (rarity >= ModConfig.Current.ItemRarityProbablility["Uncommon"]) //20% chance
            {
                return ModConfig.Current.ItemRarityColor["Uncommon"]; //Green #48ff00
            }
            else if (rarity >= ModConfig.Current.ItemRarityProbablility["Common"])//50% chance
            {
                return ModConfig.Current.ItemRarityColor["Common"]; //Grey #bbbbbb
            }
            return "";
        }
        public static float getrarityModifier(float rarity)
        {
            if (rarity >= ModConfig.Current.ItemRarityProbablility["Unique"]) //Unique, 50% bonus.
            {
                return ModConfig.Current.ItemRarityModifier["Unique"];
            }
            else if (rarity >= ModConfig.Current.ItemRarityProbablility["Legendary"]) //Legendary, 40% bonus.
            {
                return ModConfig.Current.ItemRarityModifier["Legendary"];
            }
            else if (rarity >= ModConfig.Current.ItemRarityProbablility["Epic"]) //Epic, 30% bonus.
            {
                return ModConfig.Current.ItemRarityModifier["Epic"];
            }
            else if (rarity >= ModConfig.Current.ItemRarityProbablility["Rare"]) //Rare, 20% bonus.
            {
                return ModConfig.Current.ItemRarityModifier["Rare"];
            }
            else if (rarity >= ModConfig.Current.ItemRarityProbablility["Uncommon"]) //Uncommon, 10% bonus.
            {
                return ModConfig.Current.ItemRarityModifier["Uncommon"];
            }
            else if (rarity >= ModConfig.Current.ItemRarityProbablility["Common"])//Common, no bonus.
            {
                return ModConfig.Current.ItemRarityModifier["Common"];
            }
            return 1.00f;
        }
        //Load/Create mod config.
        public override void AssetsFinalize(ICoreAPI api)
        {
            try
            {
                var Config = api.LoadModConfig<ModConfig>("rpgitemrarityConfig.json");
                if (Config != null)
                {
                    api.Logger.Notification("[rpgitemrarity] Mod Config successfully loaded.");
                    ModConfig.Current = Config;
                }
                else
                {
                    api.Logger.Notification("[rpgitemrarity] No Mod Config specified. Falling back to default settings");
                    ModConfig.Current = ModConfig.GetDefault();
                }
            }
            catch
            {
                ModConfig.Current = ModConfig.GetDefault();
                api.Logger.Error("[rpgitemrarity] Failed to load custom mod configuration. Falling back to default settings!");
            }
            finally
            {
                api.StoreModConfig(ModConfig.Current, "rpgitemrarityConfig.json");
            }
        }
    }
}