using ColossalFramework;
using Redirection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace UndoMod.Patches
{
    public class TreeToolPatch
    {
        public static MethodInfo createTree_original = typeof(TreeTool).GetMethod("CreateTree", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { }, null);
        public static MethodInfo patch = typeof(TreeToolPatch).GetMethod("CreateTree", BindingFlags.NonPublic | BindingFlags.Instance);
        public static RedirectCallsState state;

        public static void Patch()
        {
            state = RedirectionHelper.RedirectCalls(createTree_original, patch);
        }

        public static void Unpatch()
        {
            RedirectionHelper.RevertRedirect(createTree_original, state);
        }

        private IEnumerator CreateTree()
        {
            Debug.Log("CreateTree detour");
            UndoMod.Instsance.BeginObserving("Build tree");
            IEnumerator result = null;

            RedirectionHelper.RevertRedirect(createTree_original, state);
            try
            {
                result = (IEnumerator)createTree_original.Invoke(ToolsModifierControl.GetTool<TreeTool>(), new object[] { });
                AsyncTask asyncTask = new AsyncTask(result, null);
                asyncTask.Execute();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                UndoMod.Instsance.InvalidateAll();
            }
            state = RedirectionHelper.RedirectCalls(createTree_original, patch);

            //Singleton<SimulationManager>.instance.AddAction(() => {
            UndoMod.Instsance.EndObserving();
            //});

            yield return 0;
            yield break;
        }
    }
}
