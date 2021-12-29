using ColossalFramework.Math;
using HarmonyLib;
using SharedEnvironment;
using System;
using System.Reflection;
using UndoMod.Utils;
using UnityEngine;

namespace UndoMod.Patches
{
    [HarmonyPatch(typeof(PropManager))]
    [HarmonyPatch("ReleaseProp")]
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
    }

    [HarmonyPatch(typeof(PropManager))]
    [HarmonyPatch("CreateProp")]
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
        }
    }
}
