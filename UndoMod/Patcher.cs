using HarmonyLib;
using System.ComponentModel;
using System.Linq;
using UndoMod.Patches;
using UnityEngine;

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

            _harmony.PatchAll(typeof(Patcher).Assembly);
            NetManagerPatch_PreReleaseSegmentImplementation.ManualPatch(_harmony);
            NetManagerPatch_PreReleaseNodeImplementation.ManualPatch(_harmony);
            if ( Enumerable.Count(_harmony.GetPatchedMethods()) != 20) {
                throw new System.Exception("Wrong number of methods were patched");
            }
        }

        public static void UnpatchAll()
        {
            if (!patched) return;

            var _harmony = new Harmony(HarmonyID);
            _harmony.UnpatchAll(HarmonyID);

            patched = false;
        }
    }
}
