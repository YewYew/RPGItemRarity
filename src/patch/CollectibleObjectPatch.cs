using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.GameContent;
using vsrpgrarity.src;

namespace vsrpgrarity.src.patch
{
    [HarmonyPatch]
    [HarmonyPatch(typeof(CollectibleObject))]
    public class CollectibleObjectPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("GetHeldItemName"), HarmonyPriority(Priority.Last)]
        public static void Hook_GetHeldItemName(CollectibleObject __instance, ItemStack itemStack, ref string __result)
        {
            if (itemStack.Attributes != null && itemStack.Attributes.HasAttribute("rarity"))
            {
                float rarity = itemStack.Attributes.GetFloat("rarity");
                string rarityString = vsrpgrarityMod.rarityToString(rarity);
                if (vsrpgrarityMod.rarityColorToString(rarity) != "") {
                    string rarityColor = vsrpgrarityMod.rarityColorToString(rarity);
                    rarityString = "<font color=\"" + rarityColor + "\" weight=\"bold\">" + rarityString;
                }
                if (!__result.Contains(rarityString))
                {
                    __result = rarityString + " " + __result + "</font>";
                }
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch("DamageItem"), HarmonyPriority(Priority.First)]
        public static void Hook_DamageItem(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, int amount = 1)
        {
            ItemStack itemstack = itemslot.Itemstack;
            vsrpgrarityMod.vrpgrarityUpdateItemRarityItemStack(itemstack);
        }
        /*
        //Theoretically, this lets us assign rarity upon crafting.
        //However, this brings the issue of shift+clicking making all output items have the same rarity.
        //e.g. Put in four flint knife blades and four sticks, and shift click, all four will have the same rarity modifier.
        //     However, clicking seperately one-by-one, each will get an individual modifier as intended.
        //     Quite certain this is due to other related functions using itemstack.Clone for stacks of unstackables.
        [HarmonyPrefix]
        [HarmonyPatch("ConsumeCraftingIngredients"), HarmonyPriority(Priority.Last)]
        public static void Hook_ConsumeCraftingIngredients(ItemSlot[] slots, ItemSlot outputSlot, GridRecipe matchingRecipe)
        {
            ItemStack itemstack = outputSlot.Itemstack;
            vsrpgrarityMod.vrpgrarityUpdateItemRarityItemStack(itemstack);
        }
        */
    }
}
