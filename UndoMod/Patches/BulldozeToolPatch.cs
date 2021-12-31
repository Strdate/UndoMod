using HarmonyLib;
using System;
using System.Reflection;
namespace UndoMod.Patches
{
    [HarmonyPatch(typeof(BulldozeTool))]
    [HarmonyPatch("DeleteSegmentImpl")]
    class BulldozeToolPatch_DeleteSegmentImpl
    {
        static void Prefix()
        {
            UndoMod.Instsance.BeginObserving("Remove segment");
        }

        static void Finalizer(Exception __exception)
        {
            UndoMod.Instsance.FinalizeObserving(__exception);
        }
    }

    [HarmonyPatch(typeof(BulldozeTool))]
    [HarmonyPatch("DeleteNodeImpl")]
    class BulldozeToolPatch_DeleteNodeImpl
    {
        static void Prefix()
        {
            UndoMod.Instsance.BeginObserving("Remove node");
        }

        static void Finalizer(Exception __exception)
        {
            UndoMod.Instsance.FinalizeObserving(__exception);
        }
    }

    [HarmonyPatch(typeof(BulldozeTool))]
    [HarmonyPatch("DeleteBuildingImpl")]
    class BulldozeToolPatch_DeleteBuildingImpl
    {
        static void Prefix()
        {
            UndoMod.Instsance.BeginObserving("Remove building");
        }

        static void Finalizer(Exception __exception)
        {
            UndoMod.Instsance.FinalizeObserving(__exception);
        }
    }

    [HarmonyPatch(typeof(BulldozeTool))]
    [HarmonyPatch("DeleteTreeImpl")]
    class BulldozeToolPatch_DeleteTreeImpl
    {
        static void Prefix()
        {
            UndoMod.Instsance.BeginObserving("Remove tree");
        }

        static void Finalizer(Exception __exception)
        {
            UndoMod.Instsance.FinalizeObserving(__exception);
        }
    }

    [HarmonyPatch(typeof(BulldozeTool))]
    [HarmonyPatch("DeletePropImpl")]
    class BulldozeToolPatch_DeletePropImpl
    {
        static void Prefix()
        {
            UndoMod.Instsance.BeginObserving("Remove prop");
        }

        static void Finalizer(Exception __exception)
        {
            UndoMod.Instsance.FinalizeObserving(__exception);
        }
    }
}
