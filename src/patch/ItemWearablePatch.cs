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
    [HarmonyPatch(typeof(ItemWearable))]
    public class ItemWearablePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("DamageItem"), HarmonyPriority(Priority.First)]
        public static void Hook_DamageItem(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, int amount = 1)
        {
            ItemStack itemstack = itemslot.Itemstack;
            vsrpgrarityMod.vrpgrarityUpdateItemRarityItemStack(itemstack);
        }
    }
}
