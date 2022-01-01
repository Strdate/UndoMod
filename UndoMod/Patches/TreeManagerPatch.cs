using HarmonyLib;
using SharedEnvironment;
using System;
using UndoMod.Utils;
using UnityEngine;

namespace UndoMod.Patches
{
    [HarmonyPatch(typeof(TreeManager))]
    [HarmonyPatch("ReleaseTree")]
    class TreeManagerPatch_ReleaseTree
    {
        static void Prefix(uint tree)
        {
            ref TreeInstance data = ref ManagerUtils.Tree(tree);
            if (data.m_flags != 0 && PatchUtil.CheckIfObserving()) {
                if (UndoMod.Instsance.Observing) {
                    try {
                        var constructable = UndoMod.Instsance.WrappersDictionary.RegisterTree(tree);
                        constructable.ForceSetId(0);
                        UndoMod.Instsance.ReportObservedAction(new ActionRelease(constructable));
                    }
                    catch (Exception e) {
                        Debug.Log(e);
                        UndoMod.Instsance.InvalidateAll();
                    }
                } else {
                    //Invalidator.Instance.InvalidTrees.Add(tree);
                }
            }
        }
    }

    [HarmonyPatch(typeof(TreeManager))]
    [HarmonyPatch("CreateTree")]
    class TreeManagerPatch_CreateTree
    {
        static void Postfix(bool __result, ref uint tree)
        {
            if (__result && PatchUtil.CheckIfObserving()) {
                if (UndoMod.Instsance.Observing) {
                    try {
                        var constructable = UndoMod.Instsance.WrappersDictionary.RegisterTree(tree);
                        UndoMod.Instsance.ReportObservedAction(new ActionCreate(constructable));
                    }
                    catch (Exception e) {
                        Debug.Log(e);
                        UndoMod.Instsance.InvalidateAll();
                    }
                } else {
                    //
                }
            }
            UndoMod.Instsance.TerminateObservingIfVanilla();
        }
    }
}
