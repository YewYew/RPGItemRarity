using HarmonyLib;
using rpgitemrarity.src;
using System;
using System.CodeDom.Compiler;
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
        private static rpgitemrarityConfig config;
        public const string harmonyID = "vsrpgrarity.Patches";
        public vsrpgrarityMod()
        {
            harmonyInstance = new Harmony(harmonyID);
            harmonyInstance.PatchAll();
        }
        public override void Start(ICoreAPI iapi)
        {
            config = new rpgitemrarityConfig();
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
                            //Create the value and set it.
                            itemstack.Attributes.SetFloat("rarity", (float)rng.NextDouble());
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
            bool name = !config.OverrideLanguageFiles;
            if (rarity >= config.ItemRarityProbablility["Unique"]) //0% chance
            {
                return name ? Lang.Get("rpgitemrarity:prefix-unique") : config.ItemRarityName["Unique"]; //Red
            }
            else if (rarity >= config.ItemRarityProbablility["Legendary"]) //5% chance
            {
                return name ? Lang.Get("rpgitemrarity:prefix-legendary") : config.ItemRarityName["Legendary"]; //Gold
            }
            else if (rarity >= config.ItemRarityProbablility["Epic"]) //10% chance
            {
                return name ? Lang.Get("rpgitemrarity:prefix-epic") : config.ItemRarityName["Epic"]; //Violet
            }
            else if (rarity >= config.ItemRarityProbablility["Rare"]) //15% chance
            {
                return name ? Lang.Get("rpgitemrarity:prefix-rare") : config.ItemRarityName["Rare"]; //Blue
            }
            else if (rarity >= config.ItemRarityProbablility["Uncommon"]) //20% chance
            {
                return name ? Lang.Get("rpgitemrarity:prefix-uncommon") : config.ItemRarityName["Uncommon"]; //Green
            }
            else if (rarity >= config.ItemRarityProbablility["Common"])//50% chance
            {
                return name ? Lang.Get("rpgitemrarity:prefix-common") : config.ItemRarityName["Common"]; //Grey
            }
            return "";
        }
        public static string rarityColorToString(float rarity)
        {
            if (rarity >= config.ItemRarityProbablility["Unique"]) //0% chance
            {
                return config.ItemRarityColor["Unique"]; //Red #ff0d00
            }
            else if (rarity >= config.ItemRarityProbablility["Legendary"]) //5% chance
            {
                return config.ItemRarityColor["Legendary"]; //Gold #ffd700
            }
            else if (rarity >= config.ItemRarityProbablility["Epic"]) //10% chance
            {
                return config.ItemRarityColor["Epic"]; //Violet #9F63FF
            }
            else if (rarity >= config.ItemRarityProbablility["Rare"]) //15% chance
            {
                return config.ItemRarityColor["Rare"]; //Blue #00DAFE
            }
            else if (rarity >= config.ItemRarityProbablility["Uncommon"]) //20% chance
            {
                return config.ItemRarityColor["Uncommon"]; //Green #48ff00
            }
            else if (rarity >= config.ItemRarityProbablility["Common"])//50% chance
            {
                return config.ItemRarityColor["Common"]; //Grey #bbbbbb
            }
            return "";
        }
        public static float getrarityModifier(float rarity)
        {
            if (rarity >= config.ItemRarityProbablility["Unique"]) //Unique, 50% bonus.
            {
                return config.ItemRarityModifier["Unique"];
            }
            else if (rarity >= config.ItemRarityProbablility["Legendary"]) //Legendary, 40% bonus.
            {
                return config.ItemRarityModifier["Legendary"];
            }
            else if (rarity >= config.ItemRarityProbablility["Epic"]) //Epic, 30% bonus.
            {
                return config.ItemRarityModifier["Epic"];
            }
            else if (rarity >= config.ItemRarityProbablility["Rare"]) //Rare, 20% bonus.
            {
                return config.ItemRarityModifier["Rare"];
            }
            else if (rarity >= config.ItemRarityProbablility["Uncommon"]) //Uncommon, 10% bonus.
            {
                return config.ItemRarityModifier["Uncommon"];
            }
            else if (rarity >= config.ItemRarityProbablility["Common"])//Common, no bonus.
            {
                return config.ItemRarityModifier["Common"];
            }
            return 1.00f;
        }
    }
}