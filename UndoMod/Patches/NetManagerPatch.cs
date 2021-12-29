using ColossalFramework.Math;
using HarmonyLib;
using SharedEnvironment;
using System;
using System.Reflection;
using UndoMod.Utils;
using UnityEngine;

namespace UndoMod.Patches
{
    [HarmonyPatch(typeof(NetManager))]
    [HarmonyPatch("PreReleaseSegmentImplementation")]
    class NetManagerPatch_PreReleaseSegmentImplementation
    {
        static void Prefix(ushort segment)
        {
            if ((NetUtil.Segment(segment).m_flags != NetSegment.Flags.None) && PatchUtil.CheckIfObserving()) {
                if (UndoMod.Instsance.Observing) {
                    try {
                        var constructable = UndoMod.Instsance.WrappersDictionary.RegisterSegment(segment);
                        constructable.ForceSetId(0);
                        UndoMod.Instsance.ReportObservedAction(new ActionRelease(constructable));
                    }
                    catch (Exception e) {
                        Debug.Log(e);
                        UndoMod.Instsance.InvalidateAll();
                    }
                } else {
                    //Invalidator.Instance.InvalidSegments.Add(segment);
                }
            }
        }
    }

    [HarmonyPatch(typeof(NetManager))]
    [HarmonyPatch("PreReleaseNodeImplementation")]
    class NetManagerPatch_PreReleaseNodeImplementation
    {
        static void Prefix(out WrappedNode __state, ushort node, ref NetNode data, bool checkDeleted, bool checkTouchable)
        {
            WrappedNode constructable = null;
            if ((NetUtil.Node(node).m_flags != NetNode.Flags.None) && !UndoMod.Instsance.Invalidated && PreReleaseNodeImplementation_check(node, ref data, checkDeleted, checkTouchable)) {
                if (UndoMod.Instsance.Observing) {
                    try {
                        if (NetUtil.Node(node).m_building == 0 || UndoMod.Instsance.ObservingOnlyBuildings == 0) {
                            constructable = UndoMod.Instsance.WrappersDictionary.RegisterNode(node);
                            constructable.ForceSetId(0);
                        }
                        // We cannot report the node here because there still might be segments on it
                    }
                    catch (Exception e) {
                        Debug.Log(e);
                        UndoMod.Instsance.InvalidateAll();
                    }
                } else {
                    //Invalidator.Instance.InvalidNodes.Add(node);
                }
            }
            __state = constructable;
        }

        static void Postfix(WrappedNode __state, ushort node)
        {
            WrappedNode constructable = __state;
            try {
                if (constructable != null)
                    if (UndoMod.Instsance.ObservingOnlyBuildings == 0) {
                        if (!UndoMod.Instsance.PerformingAction) {
                            UndoMod.Instsance.ReportObservedAction(new ActionRelease(constructable));
                        }
                    } else {
                        UndoMod.Instsance.WrappersDictionary.AddBuildingNode(constructable);
                    }

            }
            catch (Exception e) {
                Debug.Log(e);
                UndoMod.Instsance.InvalidateAll();
            }
        }

        // semi stock code
        private static bool PreReleaseNodeImplementation_check(ushort node, ref NetNode data, bool checkDeleted, bool checkTouchable)
        {
            if (checkTouchable && (data.m_flags & NetNode.Flags.Untouchable) != NetNode.Flags.None) {
                return false;
            }
            if (checkDeleted) {
                for (int i = 0; i < 8; i++) {
                    ushort segment = data.GetSegment(i);
                    if (segment != 0 && (NetUtil.Segment(segment).m_flags & NetSegment.Flags.Deleted) == NetSegment.Flags.None) {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(NetManager))]
    [HarmonyPatch("CreateSegment")]
    class NetManagerPatch_CreateSegment
    {
        static void Postfix(ref ushort segment, bool __result)
        {
            //Debug.Log("Harmony patch: create segment postfix");
            if (__result && PatchUtil.CheckIfObserving()) {
                if (UndoMod.Instsance.Observing) {
                    try {
                        var constructable = UndoMod.Instsance.WrappersDictionary.RegisterSegment(segment);
                        UndoMod.Instsance.ReportObservedAction(new ActionCreate(constructable));
                    }
                    catch (Exception e) {
                        Debug.Log(e);
                        UndoMod.Instsance.InvalidateAll();
                    }
                } else {
                    //Invalidator.Instance.InvalidSegments.Add(segment);
                }
            }
        }
    }

    [HarmonyPatch(typeof(NetManager))]
    [HarmonyPatch("CreateNode")]
    class NetManagerPatch_CreateNode
    {
        static void Postfix(ref ushort node, bool __result)
        {
            if (__result && PatchUtil.CheckIfObserving()) {
                if (UndoMod.Instsance.Observing) {
                    try {
                        var constructable = UndoMod.Instsance.WrappersDictionary.RegisterNode(node);
                        UndoMod.Instsance.ReportObservedAction(new ActionCreate(constructable));
                    }
                    catch (Exception e) {
                        Debug.Log(e);
                        UndoMod.Instsance.InvalidateAll();
                    }
                } else {
                    //Invalidator.Instance.InvalidNodes.Add(node);
                }
            }

            if (__result && UndoMod.Instsance.ObservingOnlyBuildings > 0) {
                UndoMod.Instsance.WrappersDictionary.FixBuildingNode(node);
            }
        }
    }
}
