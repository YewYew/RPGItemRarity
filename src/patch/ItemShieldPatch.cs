using HarmonyLib;
using System;
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
    [HarmonyPatch(typeof(ItemShield))]
    public class ItemShieldPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch("GetHeldItemName"), HarmonyPriority(Priority.Last)]
        public static void Hook_GetHeldItemName(IItemStack itemStack, ref string __result)
        {
            if (itemStack.Attributes != null && itemStack.Attributes.HasAttribute("rarity"))
            {
                float rarity = itemStack.Attributes.GetFloat("rarity");
                String rarityColor = vsrpgrarityMod.rarityColorToString(rarity);
                String rarityString = "<font color=\"" + rarityColor + "\" weight=\"bold\">" + vsrpgrarityMod.rarityToString(rarity);
                if (!__result.Contains(rarityString))
                {
                    __result = rarityString + " " + __result + "</font>";
                }
            }
        }
    }
}
