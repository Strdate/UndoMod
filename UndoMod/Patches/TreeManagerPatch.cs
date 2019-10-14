using ColossalFramework.Math;
using Redirection;
using SharedEnvironment;
using System;
using System.Reflection;
using UndoMod.Utils;
using UnityEngine;

namespace UndoMod.Patches
{
    public class TreeManagerPatch
    {
        private static MethodInfo releaseTree_original = typeof(TreeManager).GetMethod("ReleaseTree");
        private static MethodInfo releaseTree_patch = typeof(TreeManagerPatch).GetMethod("ReleaseTree", BindingFlags.NonPublic | BindingFlags.Instance);
        public static RedirectCallsState releaseTreeState;

        private static MethodInfo createTree_original = typeof(TreeManager).GetMethod("CreateTree");
        private static MethodInfo createTree_patch = typeof(TreeManagerPatch).GetMethod("CreateTree", BindingFlags.NonPublic | BindingFlags.Instance);
        public static RedirectCallsState createTreeState;

        public static void Patch()
        {
            releaseTreeState = RedirectionHelper.RedirectCalls(releaseTree_original, releaseTree_patch);
            createTreeState = RedirectionHelper.RedirectCalls(createTree_original, createTree_patch);

        }

        public static void Unpatch()
        {
            RedirectionHelper.RevertRedirect(releaseTree_original, releaseTreeState);
            RedirectionHelper.RevertRedirect(createTree_original, createTreeState);
        }

        private bool CheckIfObserving()
        {
            return !UndoMod.Instsance.PerformingAction && !UndoMod.Instsance.Invalidated && UndoMod.Instsance.ObservingOnlyBuildings == 0;
        }

        private void ReleaseTree(uint tree)
        {
            //Debug.Log("redirect");
            ref TreeInstance data = ref ManagerUtils.Tree(tree);
            if (data.m_flags != 0 && CheckIfObserving())
            {
                if (UndoMod.Instsance.Observing)
                {
                    try
                    {
                        var constructable = UndoMod.Instsance.WrappersDictionary.RegisterTree(tree);
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
                    //Invalidator.Instance.InvalidTrees.Add(tree);
                }
            }

            RedirectionHelper.RevertRedirect(releaseTree_original, releaseTreeState);
            try
            {
                TreeManager.instance.ReleaseTree(tree);
            }
            finally
            {
                releaseTreeState = RedirectionHelper.RedirectCalls(releaseTree_original, releaseTree_patch);
            }
        }

        private bool CreateTree(out uint tree, ref Randomizer randomizer, TreeInfo info, Vector3 position, bool single)
        {
            //Debug.Log("redirect");
            bool result;
            RedirectionHelper.RevertRedirect(createTree_original, createTreeState);
            try
            {
                result = TreeManager.instance.CreateTree(out tree, ref randomizer, info, position, single);
            }
            finally
            {
                createTreeState = RedirectionHelper.RedirectCalls(createTree_original, createTree_patch);
            }

            if (result && CheckIfObserving())
            {
                if (UndoMod.Instsance.Observing)
                {
                    try
                    {
                        var constructable = UndoMod.Instsance.WrappersDictionary.RegisterTree(tree);
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
                    //Invalidator.Instance.InvalidTrees.Add(tree);
                }
            }

            return result;
        }
    }
}
