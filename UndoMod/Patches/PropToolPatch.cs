using HarmonyLib;
using System;

namespace UndoMod.Patches
{
    [HarmonyPatch(typeof(PropTool))]
    [HarmonyPatch("CreateProp", new Type[] { })]
    class PropToolPatch
    {
        static void Prefix()
        {
            UndoMod.Instsance.BeginObserving("Build prop");
        }

        static void Finalizer(Exception __exception)
        {
            UndoMod.Instsance.FinalizeObserving(__exception);
        }
    }
}
