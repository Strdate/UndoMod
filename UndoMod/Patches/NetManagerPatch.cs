using ColossalFramework.Math;
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
        private static MethodInfo releaseSegment_prefix = typeof(NetManagerPatch).GetMethod("PreReleaseSegmentImplementation", BindingFlags.NonPublic | BindingFlags.Instance);
        public static RedirectCallsState releaseSegmentState;

        //private static MethodInfo releaseNode_original = typeof(NetManager).GetMethod("ReleaseNode");
        private static MethodInfo releaseNode_original = typeof(NetManager).GetMethod("PreReleaseNodeImplementation", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ushort), typeof(NetNode).MakeByRefType(), typeof(bool), typeof(bool)}, null);
        private static MethodInfo releaseNode_prefix = typeof(NetManagerPatch).GetMethod("PreReleaseNodeImplementation", BindingFlags.NonPublic | BindingFlags.Instance);
        public static RedirectCallsState releaseNodeState;

        private static MethodInfo createSegment_original = typeof(NetManager).GetMethod("CreateSegment");
        private static MethodInfo createSegment_postfix = typeof(NetManagerPatch).GetMethod("CreateSegment", BindingFlags.NonPublic | BindingFlags.Instance);
        public static RedirectCallsState createSegmentState;

        private static MethodInfo createNode_original = typeof(NetManager).GetMethod("CreateNode");
        private static MethodInfo createNode_postfix = typeof(NetManagerPatch).GetMethod("CreateNode", BindingFlags.NonPublic | BindingFlags.Instance);
        public static RedirectCallsState createNodeState;

        public static void Patch()
        {

            releaseSegmentState = RedirectionHelper.RedirectCalls(releaseSegment_original, releaseSegment_prefix);
            releaseNodeState = RedirectionHelper.RedirectCalls(releaseNode_original, releaseNode_prefix);
            createSegmentState = RedirectionHelper.RedirectCalls(createSegment_original, createSegment_postfix);
            createNodeState = RedirectionHelper.RedirectCalls(createNode_original, createNode_postfix);

        }

        public static void Unpatch()
        {
            RedirectionHelper.RevertRedirect(releaseSegment_original, releaseSegmentState);
            RedirectionHelper.RevertRedirect(releaseNode_original, releaseNodeState);
            RedirectionHelper.RevertRedirect(createSegment_original, createSegmentState);
            RedirectionHelper.RevertRedirect(createNode_original, createNodeState);
        }

        private bool CheckIfObserving()
        {
            return !UndoMod.Instsance.PerformingAction && !UndoMod.Instsance.Invalidated && UndoMod.Instsance.ObservingOnlyBuildings == 0;
        }

        private void PreReleaseSegmentImplementation(ushort segment, ref NetSegment data, bool keepNodes)
        {
            //Debug.Log("redirect");
            if ((NetUtil.Segment(segment).m_flags != NetSegment.Flags.None) && CheckIfObserving())
            {
                if(UndoMod.Instsance.Observing)
                {
                    try
                    {
                        var constructable = UndoMod.Instsance.WrappersDictionary.RegisterSegment(segment);
                        constructable.ForceSetId(0);
                        UndoMod.Instsance.ReportObservedAction(new ActionRelease(constructable));
                    } catch(Exception e)
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

            RedirectionHelper.RevertRedirect(releaseSegment_original, releaseSegmentState);
            releaseSegmentState = RedirectionHelper.RedirectCalls(releaseSegment_prefix, releaseSegment_original);
            try
            {
                PreReleaseSegmentImplementation(segment, ref data, keepNodes);
            }
            finally
            {
                RedirectionHelper.RevertRedirect(releaseSegment_prefix, releaseSegmentState);
                releaseSegmentState = RedirectionHelper.RedirectCalls(releaseSegment_original, releaseSegment_prefix);
            }
        }

        // semi stock code
        private bool PreReleaseNodeImplementation_check(ushort node, ref NetNode data, bool checkDeleted, bool checkTouchable)
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

        private void PreReleaseNodeImplementation(ushort node, ref NetNode data, bool checkDeleted, bool checkTouchable)
        {
            //Debug.Log("redirect");
            WrappedNode constructable = null;
            if ((NetUtil.Node(node).m_flags != NetNode.Flags.None) && !UndoMod.Instsance.Invalidated && PreReleaseNodeImplementation_check(node, ref data, checkDeleted, checkTouchable))
            {
                if (UndoMod.Instsance.Observing)
                {
                    try
                    {
                        if(NetUtil.Node(node).m_building == 0 || UndoMod.Instsance.ObservingOnlyBuildings == 0)
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

            RedirectionHelper.RevertRedirect(releaseNode_original, releaseNodeState);
            releaseNodeState = RedirectionHelper.RedirectCalls(releaseNode_prefix, releaseNode_original);

            try
            {
                PreReleaseNodeImplementation(node, ref data, checkDeleted, checkTouchable);
            }
            finally
            {
                RedirectionHelper.RevertRedirect(releaseNode_prefix, releaseNodeState);
                releaseNodeState = RedirectionHelper.RedirectCalls(releaseNode_original, releaseNode_prefix);
            }

            try
            {
                if (constructable != null)
                    if (UndoMod.Instsance.ObservingOnlyBuildings == 0)
                    {
                        if(!UndoMod.Instsance.PerformingAction)
                        {
                            UndoMod.Instsance.ReportObservedAction(new ActionRelease(constructable));
                        }
                    } else
                    {
                        UndoMod.Instsance.WrappersDictionary.AddBuildingNode(constructable);
                    }
                
            } catch(Exception e)
            {
                Debug.Log(e);
                UndoMod.Instsance.InvalidateAll();
            }
        }

        private bool CreateSegment(out ushort segment, ref Randomizer randomizer, NetInfo info, ushort startNode, ushort endNode, Vector3 startDirection, Vector3 endDirection, uint buildIndex, uint modifiedIndex, bool invert)
        {
            //Debug.Log("redirect");
            bool result;
            RedirectionHelper.RevertRedirect(createSegment_original, createSegmentState);
            try {
                result = NetManager.instance.CreateSegment(out segment, ref randomizer, info, startNode, endNode, startDirection, endDirection, buildIndex, modifiedIndex, invert);
            } finally
            {
                createSegmentState = RedirectionHelper.RedirectCalls(createSegment_original, createSegment_postfix);
            }

            if (result && CheckIfObserving())
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

            return result;
        }

        private bool CreateNode(out ushort node, ref Randomizer randomizer, NetInfo info, Vector3 position, uint buildIndex)
        {
            //Debug.Log("redirect");
            bool result;
            RedirectionHelper.RevertRedirect(createNode_original, createNodeState);
            try
            {
                result = NetManager.instance.CreateNode(out node, ref randomizer, info, position, buildIndex);
            }
            finally
            {
                createNodeState = RedirectionHelper.RedirectCalls(createNode_original, createNode_postfix);
            }

            if (result && CheckIfObserving())
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

            if(result && UndoMod.Instsance.ObservingOnlyBuildings > 0)
            {
                UndoMod.Instsance.WrappersDictionary.FixBuildingNode(node);
            }

            return result;
        }

        /*private static MethodInfo releaseSegment_original = typeof(NetManager).GetMethod("ReleaseSegment");
        private static MethodInfo releaseSegment_prefix = typeof(NetManagerPatch).GetMethod("ReleaseSegment_prefix", BindingFlags.NonPublic);

        private static MethodInfo releaseNode_original = typeof(NetManager).GetMethod("ReleaseNode");
        private static MethodInfo releaseNode_prefix = typeof(NetManagerPatch).GetMethod("ReleaseNode_prefix", BindingFlags.NonPublic);

        private static MethodInfo createSegment_original = typeof(NetManager).GetMethod("CreateSegment");
        private static MethodInfo createSegment_postfix = typeof(NetManagerPatch).GetMethod("CreateSegment_postfix", BindingFlags.NonPublic);

        private static MethodInfo createNode_original = typeof(NetManager).GetMethod("CreateNode");
        private static MethodInfo createNode_postfix = typeof(NetManagerPatch).GetMethod("CreateNode_postfix", BindingFlags.NonPublic);

        public static void Patch(HarmonyInstance _harmony)
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

        /*private static void ReleaseSegment_prefix(ushort segment)
        {
            if ((NetUtil.Segment(segment).m_flags != NetSegment.Flags.None) && !UndoMod.Instsance.PerformingAction && (UndoMod.Instsance.ObservedItem != null))
            {
                var action = UndoMod.Instsance.WrappersDictionary.RegisterSegment(segment);
                action.IsBuildAction = false;
                action.ForceSetId(0);
                UndoMod.Instsance.ObservedItem.Actions.Add(action);
            }
        }

        private static void ReleaseNode_prefix(ushort node)
        {
            if((NetUtil.Node(node).m_flags != NetNode.Flags.None) && !UndoMod.Instsance.PerformingAction && (UndoMod.Instsance.ObservedItem != null))
            {
                var action = UndoMod.Instsance.WrappersDictionary.RegisterNode(node);
                action.IsBuildAction = false;
                action.ForceSetId(0);
                UndoMod.Instsance.ObservedItem.Actions.Add(action);
            }
        }

        private static void CreateSegment_postfix(bool __result, ushort segment)
        {
            if(__result && !UndoMod.Instsance.PerformingAction && (UndoMod.Instsance.ObservedItem != null))
            {
                var action = UndoMod.Instsance.WrappersDictionary.RegisterSegment(segment);
                UndoMod.Instsance.ObservedItem.Actions.Add(action);
            }
        }

        private static void CreateNode_postfix(bool __result, ushort node)
        {
            if (__result && !UndoMod.Instsance.PerformingAction && (UndoMod.Instsance.ObservedItem != null))
            {
                var action = UndoMod.Instsance.WrappersDictionary.RegisterNode(node);
                UndoMod.Instsance.ObservedItem.Actions.Add(action);
            }
        }*/
    }
}
