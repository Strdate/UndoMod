using ColossalFramework.Math;
using HarmonyLib;
using SharedEnvironment;
using System;
using System.Reflection;
using UndoMod.Utils;
using UnityEngine;

namespace UndoMod.Patches
{
    //[HarmonyPatch(typeof(NetManager))]
    //[HarmonyPatch("PreReleaseSegmentImplementation", new Type[] { typeof(ushort), typeof(NetSegment)/*.MakeByRefType()*/, typeof(bool) })]
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

        private static MethodInfo releaseSegment_original = typeof(NetManager).GetMethod("PreReleaseSegmentImplementation", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(NetSegment).MakeByRefType(), typeof(bool) }, null);
        private static MethodInfo releaseSegment_prefix = typeof(NetManagerPatch_PreReleaseSegmentImplementation).GetMethod("Prefix", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

        internal static void ManualPatch(Harmony _harmony)
        {
            _harmony.Patch(releaseSegment_original, new HarmonyMethod(releaseSegment_prefix));
        }

        internal static void ManualUnpatch(Harmony _harmony)
        {
            _harmony.Unpatch(releaseSegment_original, releaseSegment_prefix);
        }
    }

    //[HarmonyPatch(typeof(NetManager))]
    //[HarmonyPatch("PreReleaseNodeImplementation", new Type[] { typeof(ushort), typeof(NetNode)/*.MakeByRefType()*/, typeof(bool), typeof(bool) })]
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

        private static MethodInfo releaseNode_original = typeof(NetManager).GetMethod("PreReleaseNodeImplementation", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(NetNode).MakeByRefType(), typeof(bool), typeof(bool) }, null);
        private static MethodInfo releaseNode_prefix = typeof(NetManagerPatch_PreReleaseNodeImplementation).GetMethod("Prefix", BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo releaseNode_postfix = typeof(NetManagerPatch_PreReleaseNodeImplementation).GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Static);

        internal static void ManualPatch(Harmony _harmony)
        {
            _harmony.Patch(original: releaseNode_original, prefix: new HarmonyMethod(releaseNode_prefix), finalizer: new HarmonyMethod(releaseNode_postfix));
        }

        internal static void ManualUnpatch(Harmony _harmony)
        {
            _harmony.Unpatch(releaseNode_original, releaseNode_prefix);
            _harmony.Unpatch(releaseNode_original, releaseNode_postfix);
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

    //[HarmonyPatch(typeof(NetManager))]
    //[HarmonyPatch("CreateSegment")]
    class NetManagerPatch_CreateSegment
    {
        private static MethodInfo createSegment_original = typeof(NetManager).GetMethod("CreateSegment", BindingFlags.Public | BindingFlags.Instance, null,
            new Type[] { typeof(ushort).MakeByRefType(), typeof(Randomizer).MakeByRefType(), typeof(NetInfo), typeof(TreeInfo), typeof(ushort), typeof(ushort), typeof(Vector3), typeof(Vector3), typeof(uint), typeof(uint), typeof(bool) }, null);
        private static MethodInfo createSegment_postfix = typeof(NetManagerPatch_CreateSegment).GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Static);

        internal static void ManualPatch(Harmony _harmony)
        {
            _harmony.Patch(original: createSegment_original, postfix: new HarmonyMethod(createSegment_postfix));
        }

        internal static void ManualUnpatch(Harmony _harmony)
        {
            _harmony.Unpatch(createSegment_original, createSegment_postfix);
        }

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
