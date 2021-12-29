using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UndoMod.Patches;

namespace UndoMod
{
    public static class Patcher
    {
        public static readonly string HarmonyID = "strad.undomod";
        private static bool patched = false;

        public static void PatchAll()
        {
            if (patched) return;

            var _harmony = new Harmony(HarmonyID);
            patched = true;
            

            /*NetToolPatch.Patch(_harmony);
            NetManagerPatch.Patch(_harmony);*/

            BulldozeToolPatch.Patch();
            TreeToolPatch.Patch();
            PropToolPatch.Patch();
            NetToolPatch.Patch();
            BuildingToolPatch.Patch();
            TreeManagerPatch.Patch();
            PropManagerPatch.Patch();
            NetManagerPatch.Patch(_harmony);
            BuildingManagerPatch.Patch(_harmony);
        }

        public static void UnpatchAll()
        {
            if (!patched) return;

            var _harmony = new Harmony(HarmonyID);

            BuildingManagerPatch.Unpatch(_harmony);
            NetManagerPatch.Unpatch(_harmony);
            PropManagerPatch.Unpatch();
            BuildingToolPatch.Unpatch();
            TreeManagerPatch.Unpatch();
            NetToolPatch.Unpatch();
            PropToolPatch.Unpatch();
            TreeToolPatch.Unpatch();
            BulldozeToolPatch.Unpatch();
            /*NetManagerPatch.Unpatch(_harmony);
            NetToolPatch.Unpatch(_harmony);*/

            patched = false;
        }
    }
}
