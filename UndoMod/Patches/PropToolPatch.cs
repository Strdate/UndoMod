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
    public class PropToolPatch
    {
        public static MethodInfo createProp_original = typeof(PropTool).GetMethod("CreateProp", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { }, null);
        public static MethodInfo patch = typeof(PropToolPatch).GetMethod("CreateProp", BindingFlags.NonPublic | BindingFlags.Instance);
        public static RedirectCallsState state;

        public static void Patch()
        {
            state = RedirectionHelper.RedirectCalls(createProp_original, patch);
        }

        public static void Unpatch()
        {
            RedirectionHelper.RevertRedirect(createProp_original, state);
        }

        private IEnumerator CreateProp()
        {
            Debug.Log("CreateProp detour");
            UndoMod.Instsance.BeginObserving("Build prop", "Vanilla");
            IEnumerator result = null;

            RedirectionHelper.RevertRedirect(createProp_original, state);
            try
            {
                result = (IEnumerator)createProp_original.Invoke(ToolsModifierControl.GetTool<PropTool>(), new object[] { });
                AsyncTask asyncTask = new AsyncTask(result, null);
                asyncTask.Execute();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                UndoMod.Instsance.InvalidateAll();
            }
            state = RedirectionHelper.RedirectCalls(createProp_original, patch);

            //Singleton<SimulationManager>.instance.AddAction(() => {
            UndoMod.Instsance.EndObserving();
            //});

            yield return 0;
            yield break;
        }
    }
}
