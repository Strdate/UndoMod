using HarmonyLib;
using System;

namespace UndoMod.Patches
{
    [HarmonyPatch(typeof(TreeTool))]
    [HarmonyPatch("CreateTree", new Type[] { })]
    class TreeToolPatch
    {
        static void Prefix()
        {
            UndoMod.Instsance.BeginObserving("Build tree");
        }

        static void Finalizer(Exception __exception)
        {
            UndoMod.Instsance.FinalizeObserving(__exception);
        }
    }
}
