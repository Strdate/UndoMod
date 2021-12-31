using HarmonyLib;
using System;
using System.Reflection;

namespace UndoMod.Patches
{
    //[HarmonyPatch(typeof(PropTool))]
    //[HarmonyPatch("CreateProp", new Type[] { })]
    class PropToolPatch
    {
        static void Prefix()
        {
            UndoMod.Instsance.BeginObserving("Build prop", autoTerminate: true);
        }

        private static MethodInfo original = PatchUtil.Method(typeof(PropTool), "CreateProp");
        private static MethodInfo prefix = PatchUtil.Method(typeof(PropToolPatch), "Prefix");

        internal static void ManualPatch(Harmony _harmony)
        {
            _harmony.Patch(original: original, prefix: new HarmonyMethod(prefix));
        }

        internal static void ManualUnpatch(Harmony _harmony)
        {
            _harmony.Unpatch(original, prefix);
        }
    }
}
