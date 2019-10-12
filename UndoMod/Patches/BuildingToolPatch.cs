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
    public class BuildingToolPatch
    {
        public static MethodInfo createBuilding_original = typeof(BuildingTool).GetMethod("CreateBuilding", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] {}, null);
        public static MethodInfo patch = typeof(BuildingToolPatch).GetMethod("CreateBuilding", BindingFlags.NonPublic | BindingFlags.Instance);
        public static RedirectCallsState state;

        public static void Patch()
        {
            state = RedirectionHelper.RedirectCalls(createBuilding_original, patch);
        }

        public static void Unpatch()
        {
            RedirectionHelper.RevertRedirect(createBuilding_original, state);
        }

        private IEnumerator CreateBuilding()
        {
            Debug.Log("CreateBuilding detour");
            UndoMod.Instsance.BeginObserving("Build building", true);
            IEnumerator result = null;

            RedirectionHelper.RevertRedirect(createBuilding_original, state);
            try
            {
                // This is a genius piece of code. No joking!
                result = (IEnumerator) createBuilding_original.Invoke(ToolsModifierControl.GetTool<BuildingTool>(), new object[] {});
                AsyncTask asyncTask = new AsyncTask(result, null);
                asyncTask.Execute();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                UndoMod.Instsance.InvalidateAll();
            }
            state = RedirectionHelper.RedirectCalls(createBuilding_original, patch);

            //Singleton<SimulationManager>.instance.AddAction(() => {
                UndoMod.Instsance.EndObserving();
            //});

            yield return 0;
            yield break;
        }
    }
}
