using ColossalFramework.Math;
using HarmonyLib;
using SharedEnvironment;
using System;
using System.Reflection;
using UndoMod.Utils;
using UnityEngine;

namespace UndoMod.Patches
{
    //[HarmonyPatch(typeof(PropManager))]
    //[HarmonyPatch("ReleaseProp")]
    class PropManagerPatch_ReleaseProp
    {
        static void Prefix(ushort prop)
        {
            ref PropInstance data = ref ManagerUtils.Prop(prop);
            if (data.m_flags != 0 && PatchUtil.CheckIfObserving()) {
                if (UndoMod.Instsance.Observing) {
                    try {
                        var constructable = UndoMod.Instsance.WrappersDictionary.RegisterProp(prop);
                        constructable.ForceSetId(0);
                        UndoMod.Instsance.ReportObservedAction(new ActionRelease(constructable));
                    }
                    catch (Exception e) {
                        Debug.Log(e);
                        UndoMod.Instsance.InvalidateAll();
                    }
                } else {
                    //Invalidator.Instance.InvalidProps.Add(Prop);
                }
            }
        }

        private static MethodInfo original = PatchUtil.Method(typeof(PropManager), "ReleaseProp");
        private static MethodInfo prefix = PatchUtil.Method(typeof(PropManagerPatch_ReleaseProp), "Prefix");

        internal static void ManualPatch(Harmony _harmony)
        {
            _harmony.Patch(original: original, prefix: new HarmonyMethod(prefix));
        }

        internal static void ManualUnpatch(Harmony _harmony)
        {
            _harmony.Unpatch(original, prefix);
        }
    }

    //[HarmonyPatch(typeof(PropManager))]
    //[HarmonyPatch("CreateProp")]
    class PropManagerPatch_CreateProp
    {
        static void Postfix(bool __result, ref ushort prop)
        {
            if (__result && PatchUtil.CheckIfObserving()) {
                if (UndoMod.Instsance.Observing) {
                    try {
                        var constructable = UndoMod.Instsance.WrappersDictionary.RegisterProp(prop);
                        UndoMod.Instsance.ReportObservedAction(new ActionCreate(constructable));
                    }
                    catch (Exception e) {
                        Debug.Log(e);
                        UndoMod.Instsance.InvalidateAll();
                    }
                } else {
                    //Invalidator.Instance.InvalidProps.Add(prop);
                }
            }
            UndoMod.Instsance.TerminateObservingIfVanilla();
        }

        private static MethodInfo original = PatchUtil.Method(typeof(PropManager), "CreateProp");
        private static MethodInfo postfix = PatchUtil.Method(typeof(PropManagerPatch_CreateProp), "Postfix");

        internal static void ManualPatch(Harmony _harmony)
        {
            _harmony.Patch(original: original, postfix: new HarmonyMethod(postfix));
        }

        internal static void ManualUnpatch(Harmony _harmony)
        {
            _harmony.Unpatch(original, postfix);
        }
    }
}
