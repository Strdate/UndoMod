using ColossalFramework.Math;
using HarmonyLib;
using SharedEnvironment;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UndoMod.Utils;
using UnityEngine;

namespace UndoMod.Patches
{
    [HarmonyPatch(typeof(BuildingManager))]
    [HarmonyPatch("ReleaseBuilding")]
    class BuildingManagerPatch_ReleaseBuilding
    {
        static void Prefix(ushort building)
        {
            UndoMod.Instsance.ObservingOnlyBuildings++;
            ref Building data = ref ManagerUtils.BuildingS(building);
            if ((data.m_flags != Building.Flags.None && (data.m_flags & Building.Flags.Deleted) == Building.Flags.None) && !UndoMod.Instsance.PerformingAction
                    && !UndoMod.Instsance.Invalidated) {
                if (UndoMod.Instsance.Observing) {
                    try {
                        var constructable = UndoMod.Instsance.WrappersDictionary.RegisterBuilding(building);
                        constructable.ForceSetId(0);
                        UndoMod.Instsance.ReportObservedAction(new ActionRelease(constructable));
                    }
                    catch (Exception e) {
                        Debug.Log(e);
                        UndoMod.Instsance.InvalidateAll();
                    }
                } else {
                    //Invalidator.Instance.InvalidBuildings.Add(building);
                }
            }
        }

        static void Finalizer()
        {
            UndoMod.Instsance.ObservingOnlyBuildings--;
        }
    }

    [HarmonyPatch(typeof(BuildingManager))]
    [HarmonyPatch("CreateBuilding")]
    class BuildingManagerPatch_CreateBuilding
    {
        static void Prefix()
        {
            UndoMod.Instsance.ObservingOnlyBuildings++;
        }

        static void Finalizer(bool __result, ref ushort building, Exception __exception)
        {
            UndoMod.Instsance.ObservingOnlyBuildings--;
            if (__result && __exception == null && !UndoMod.Instsance.PerformingAction && !UndoMod.Instsance.Invalidated /*&& CheckCaller()*/) {
                if (UndoMod.Instsance.Observing) {
                    try {
                        var constructable = UndoMod.Instsance.WrappersDictionary.RegisterBuilding(building);
                        UndoMod.Instsance.ReportObservedAction(new ActionCreate(constructable));
                    }
                    catch (Exception e) {
                        Debug.Log(e);
                        UndoMod.Instsance.InvalidateAll();
                    }
                } else {
                    //Invalidator.Instance.InvalidBuildings.Add(building);
                }
            }
        }
    }

    [HarmonyPatch(typeof(BuildingManager))]
    [HarmonyPatch("RelocateBuilding")]
    class BuildingManagerPatch_RelocateBuilding
    {
        static void Postfix()
        {
            UndoMod.Instsance.InvalidateAll(false);
        }
    }
}