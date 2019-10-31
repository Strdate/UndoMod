using ColossalFramework.Math;
using Harmony;
using Redirection;
using SharedEnvironment;
using System;
using System.Reflection;
using UndoMod.Utils;
using UnityEngine;

namespace UndoMod.Patches
{
    public class NetManagerPatch
    {
        private static MethodInfo releaseSegment_original = typeof(NetManager).GetMethod("PreReleaseSegmentImplementation", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(NetSegment).MakeByRefType(), typeof(bool) }, null);

        private static MethodInfo releaseNode_original = typeof(NetManager).GetMethod("PreReleaseNodeImplementation", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(NetNode).MakeByRefType(), typeof(bool), typeof(bool)}, null);

        private static MethodInfo createSegment_original = typeof(NetManager).GetMethod("CreateSegment");

        private static MethodInfo createNode_original = typeof(NetManager).GetMethod("CreateNode");

        public static void Patch(HarmonyInstance _harmony)
        {
            //Debug.LogWarning(releaseNode_prefix + " " + releaseNode_postfix);
            _harmony.Patch(releaseNode_original, new HarmonyMethod(releaseNode_prefix), new HarmonyMethod(releaseNode_postfix));
            _harmony.Patch(createNode_original, null, new HarmonyMethod(createNode_postfix));
            _harmony.Patch(releaseSegment_original, new HarmonyMethod(releaseSegment_prefix));
            _harmony.Patch(createSegment_original, null, new HarmonyMethod(createSegment_postfix));
        }

        public static void Unpatch(HarmonyInstance _harmony)
        {
            _harmony.Unpatch(releaseNode_original, releaseNode_prefix);
            _harmony.Unpatch(releaseNode_original, releaseNode_postfix);

            _harmony.Unpatch(createNode_original, createNode_postfix);
            _harmony.Unpatch(releaseSegment_original, releaseSegment_prefix);
            _harmony.Unpatch(createSegment_original, createSegment_postfix);
        }

        private static bool CheckIfObserving()
        {
            return !UndoMod.Instsance.PerformingAction && !UndoMod.Instsance.Invalidated && UndoMod.Instsance.ObservingOnlyBuildings == 0;
        }

        // semi stock code
        private static bool PreReleaseNodeImplementation_check(ushort node, ref NetNode data, bool checkDeleted, bool checkTouchable)
        {
            if (checkTouchable && (data.m_flags & NetNode.Flags.Untouchable) != NetNode.Flags.None)
            {
                return false;
            }
            if (checkDeleted)
            {
                for (int i = 0; i < 8; i++)
                {
                    ushort segment = data.GetSegment(i);
                    if (segment != 0 && (NetUtil.Segment(segment).m_flags & NetSegment.Flags.Deleted) == NetSegment.Flags.None)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        //private static MethodInfo releaseSegment_original = typeof(NetManager).GetMethod("ReleaseSegment");
        private static MethodInfo releaseSegment_prefix = typeof(NetManagerPatch).GetMethod("ReleaseSegment_prefix", BindingFlags.NonPublic | BindingFlags.Static);

        //private static MethodInfo releaseNode_original = typeof(NetManager).GetMethod("ReleaseNode");
        private static MethodInfo releaseNode_prefix = typeof(NetManagerPatch).GetMethod("ReleaseNode_prefix", BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo releaseNode_postfix = typeof(NetManagerPatch).GetMethod("ReleaseNode_postfix", BindingFlags.NonPublic | BindingFlags.Static);

        //private static MethodInfo createSegment_original = typeof(NetManager).GetMethod("CreateSegment");
        private static MethodInfo createSegment_postfix = typeof(NetManagerPatch).GetMethod("CreateSegment_postfix", BindingFlags.NonPublic | BindingFlags.Static);

        //private static MethodInfo createNode_original = typeof(NetManager).GetMethod("CreateNode");
        private static MethodInfo createNode_postfix = typeof(NetManagerPatch).GetMethod("CreateNode_postfix", BindingFlags.NonPublic | BindingFlags.Static);

        /*public static void Patch(HarmonyInstance _harmony)
        {
            _harmony.Patch(releaseSegment_original, new HarmonyMethod( releaseSegment_prefix ));
            _harmony.Patch(releaseNode_original, new HarmonyMethod(releaseNode_prefix));
            _harmony.Patch(createSegment_original, null, new HarmonyMethod(createSegment_postfix));
            _harmony.Patch(createNode_original, null, new HarmonyMethod(createNode_postfix));
        }

        public static void Unpatch(HarmonyInstance _harmony)
        {
            _harmony.Unpatch(releaseSegment_original, releaseSegment_prefix);
            _harmony.Unpatch(releaseNode_original, releaseNode_prefix);
            _harmony.Unpatch(createSegment_original, createSegment_postfix);
            _harmony.Unpatch(createNode_original, createNode_postfix);
        }*/

        private static void ReleaseSegment_prefix(ushort segment, ref NetSegment data, bool keepNodes)
        {
            //Debug.Log("Harmony patch: release segment prefix");
            if ((NetUtil.Segment(segment).m_flags != NetSegment.Flags.None) && CheckIfObserving())
            {
                if (UndoMod.Instsance.Observing)
                {
                    try
                    {
                        var constructable = UndoMod.Instsance.WrappersDictionary.RegisterSegment(segment);
                        constructable.ForceSetId(0);
                        UndoMod.Instsance.ReportObservedAction(new ActionRelease(constructable));
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                        UndoMod.Instsance.InvalidateAll();
                    }
                }
                else
                {
                    //Invalidator.Instance.InvalidSegments.Add(segment);
                }
            }
        }

        private static WrappedNode __state;
        private static void ReleaseNode_prefix(/*WrappedNode __state,*/ ushort node, ref NetNode data, bool checkDeleted, bool checkTouchable)
        {
            Debug.Log("Prefix release node " + node);
            __state = null;
            WrappedNode constructable = null;
            if ((NetUtil.Node(node).m_flags != NetNode.Flags.None) && !UndoMod.Instsance.Invalidated && PreReleaseNodeImplementation_check(node, ref data, checkDeleted, checkTouchable))
            {
                if (UndoMod.Instsance.Observing)
                {
                    try
                    {
                        if (NetUtil.Node(node).m_building == 0 || UndoMod.Instsance.ObservingOnlyBuildings == 0)
                        {
                            constructable = UndoMod.Instsance.WrappersDictionary.RegisterNode(node);
                            constructable.ForceSetId(0);
                        }
                        // We cannot report the node here because there still might be segments on it
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                        UndoMod.Instsance.InvalidateAll();
                    }
                }
                else
                {
                    //Invalidator.Instance.InvalidNodes.Add(node);
                }
            }
            __state = constructable;
        }

        private static void ReleaseNode_postfix(/*WrappedNode __state,*/ ushort node)
        {
            Debug.Log("Postfix " + node + ": " + __state);
            WrappedNode constructable = __state;
            try
            {
                if (constructable != null)
                    if (UndoMod.Instsance.ObservingOnlyBuildings == 0)
                    {
                        if (!UndoMod.Instsance.PerformingAction)
                        {
                            UndoMod.Instsance.ReportObservedAction(new ActionRelease(constructable));
                        }
                    }
                    else
                    {
                        UndoMod.Instsance.WrappersDictionary.AddBuildingNode(constructable);
                    }

            }
            catch (Exception e)
            {
                Debug.Log(e);
                UndoMod.Instsance.InvalidateAll();
            }
        }

        private static void CreateSegment_postfix(ref ushort segment, bool __result)
        {
            //Debug.Log("Harmony patch: create segment postfix");
            if (__result && CheckIfObserving())
            {
                if (UndoMod.Instsance.Observing)
                {
                    try
                    {
                        var constructable = UndoMod.Instsance.WrappersDictionary.RegisterSegment(segment);
                        UndoMod.Instsance.ReportObservedAction(new ActionCreate(constructable));
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                        UndoMod.Instsance.InvalidateAll();
                    }
                }
                else
                {
                    //Invalidator.Instance.InvalidSegments.Add(segment);
                }
            }
        }

        private static void CreateNode_postfix(ref ushort node, bool __result)
        {
            if (__result && CheckIfObserving())
            {
                if (UndoMod.Instsance.Observing)
                {
                    try
                    {
                        var constructable = UndoMod.Instsance.WrappersDictionary.RegisterNode(node);
                        UndoMod.Instsance.ReportObservedAction(new ActionCreate(constructable));
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                        UndoMod.Instsance.InvalidateAll();
                    }
                }
                else
                {
                    //Invalidator.Instance.InvalidNodes.Add(node);
                }
            }

            if (__result && UndoMod.Instsance.ObservingOnlyBuildings > 0)
            {
                UndoMod.Instsance.WrappersDictionary.FixBuildingNode(node);
            }
        }
    }
}
