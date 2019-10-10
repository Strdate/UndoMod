using ColossalFramework.Math;
using Redirection;
using System.Reflection;
using UndoMod.Utils;
using UnityEngine;

namespace UndoMod.Patches
{
    public class NetManagerPatch
    {
        private static MethodInfo releaseSegment_original = typeof(NetManager).GetMethod("ReleaseSegment");
        private static MethodInfo releaseSegment_prefix = typeof(NetManagerPatch).GetMethod("ReleaseSegment", BindingFlags.NonPublic | BindingFlags.Instance);
        public static RedirectCallsState releaseSegmentState;

        private static MethodInfo releaseNode_original = typeof(NetManager).GetMethod("ReleaseNode");
        private static MethodInfo releaseNode_prefix = typeof(NetManagerPatch).GetMethod("ReleaseNode", BindingFlags.NonPublic | BindingFlags.Instance);
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

        private void ReleaseSegment(ushort segment, bool keepNodes)
        {
            Debug.Log("redirect");
            if ((NetUtil.Segment(segment).m_flags != NetSegment.Flags.None) && !UndoMod.Instsance.PerformingAction && (UndoMod.Instsance.ObservedItem != null))
            {
                var action = UndoMod.Instsance.WrappersDictionary.RegisterSegment(segment);
                action.IsBuildAction = false;
                action.ForceSetId(0);
                UndoMod.Instsance.ObservedItem.Actions.Add(action);
            }

            RedirectionHelper.RevertRedirect(releaseSegment_original, releaseSegmentState);
            NetManager.instance.ReleaseSegment(segment, keepNodes);
            releaseSegmentState = RedirectionHelper.RedirectCalls(releaseSegment_original, releaseSegment_prefix);
        }

        private void ReleaseNode(ushort node)
        {
            Debug.Log("redirect");
            if ((NetUtil.Node(node).m_flags != NetNode.Flags.None) && !UndoMod.Instsance.PerformingAction && (UndoMod.Instsance.ObservedItem != null))
            {
                var action = UndoMod.Instsance.WrappersDictionary.RegisterNode(node);
                action.IsBuildAction = false;
                action.ForceSetId(0);
                UndoMod.Instsance.ObservedItem.Actions.Add(action);
            }

            RedirectionHelper.RevertRedirect(releaseNode_original, releaseNodeState);
            NetManager.instance.ReleaseNode(node);
            releaseNodeState = RedirectionHelper.RedirectCalls(releaseNode_original, releaseNode_prefix);
        }

        private bool CreateSegment(out ushort segment, ref Randomizer randomizer, NetInfo info, ushort startNode, ushort endNode, Vector3 startDirection, Vector3 endDirection, uint buildIndex, uint modifiedIndex, bool invert)
        {
            Debug.Log("redirect");
            RedirectionHelper.RevertRedirect(createSegment_original, createSegmentState);
            bool result = NetManager.instance.CreateSegment(out segment, ref randomizer, info, startNode, endNode, startDirection, endDirection, buildIndex, modifiedIndex, invert);
            createSegmentState = RedirectionHelper.RedirectCalls(createSegment_original, createSegment_postfix);

            if (result && !UndoMod.Instsance.PerformingAction && (UndoMod.Instsance.ObservedItem != null))
            {
                var action = UndoMod.Instsance.WrappersDictionary.RegisterSegment(segment);
                UndoMod.Instsance.ObservedItem.Actions.Add(action);
            }

            return result;
        }

        private bool CreateNode(out ushort node, ref Randomizer randomizer, NetInfo info, Vector3 position, uint buildIndex)
        {
            Debug.Log("redirect");
            RedirectionHelper.RevertRedirect(createNode_original, createNodeState);
            bool result = NetManager.instance.CreateNode(out node, ref randomizer, info, position, buildIndex);
            createNodeState = RedirectionHelper.RedirectCalls(createNode_original, createNode_postfix);

            if (result && !UndoMod.Instsance.PerformingAction && (UndoMod.Instsance.ObservedItem != null))
            {
                var action = UndoMod.Instsance.WrappersDictionary.RegisterNode(node);
                UndoMod.Instsance.ObservedItem.Actions.Add(action);
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
