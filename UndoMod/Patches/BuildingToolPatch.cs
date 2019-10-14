using ColossalFramework;
using Harmony;
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
        /*public static MethodInfo createBuilding_originalLong = typeof(BuildingTool).GetMethod("CreateBuilding", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] {
            typeof(BuildingInfo).MakeByRefType(),
            typeof(Vector3),
            typeof(float),
            typeof(int),
            typeof(bool),
            typeof(bool),
        }, null);*/
        public static MethodInfo createBuilding_originalShort = typeof(BuildingTool).GetMethod("CreateBuilding", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] {}, null);

        public static MethodInfo shortPatch = typeof(BuildingToolPatch).GetMethod("CreateBuilding_Short", BindingFlags.NonPublic | BindingFlags.Instance);

        public static RedirectCallsState state;

        public static void Patch()
        {
            state = RedirectionHelper.RedirectCalls(createBuilding_originalShort, shortPatch);
            //_harmony.Patch(createBuilding_originalLong, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
        }

        public static void Unpatch()
        {
            RedirectionHelper.RevertRedirect(createBuilding_originalShort, state);
            //_harmony.Unpatch(createBuilding_originalLong, prefix);
            //_harmony.Unpatch(createBuilding_originalLong, postfix);
        }

        private IEnumerator CreateBuilding_Short()
        {
            //Debug.Log("CreateBuilding detour");
            UndoMod.Instsance.BeginObserving("Build building", "Vanilla");
            IEnumerator result = null;

            RedirectionHelper.RevertRedirect(createBuilding_originalShort, state);
            state = RedirectionHelper.RedirectCalls(shortPatch, createBuilding_originalShort);
            try
            {
                // This is a genius piece of code. No joking!
                //result = (IEnumerator) createBuilding_originalShort.Invoke(ToolsModifierControl.GetTool<BuildingTool>(), new object[] {});
                result = CreateBuilding_Short();
                AsyncTask asyncTask = new AsyncTask(result, null);
                asyncTask.Execute();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                UndoMod.Instsance.InvalidateAll();
            } finally {
                RedirectionHelper.RevertRedirect(shortPatch, state);
                state = RedirectionHelper.RedirectCalls(createBuilding_originalShort, shortPatch);
            } 

            //Singleton<SimulationManager>.instance.AddAction(() => {
                UndoMod.Instsance.EndObserving();
            //});

            yield return 0;
            yield break;
        }

        /*
        public static MethodInfo prefix = typeof(BuildingToolPatch).GetMethod("CreateBuilding_Prefix", BindingFlags.NonPublic | BindingFlags.Static);
        public static MethodInfo postfix = typeof(BuildingToolPatch).GetMethod("CreateBuilding_Postfix", BindingFlags.NonPublic | BindingFlags.Static);

        // long
        private static void CreateBuilding_Prefix(ref BuildingInfo info, Vector3 position, float angle, int relocating)
        {
            if (relocating != 0)
            {
                if (UndoMod.Instsance.ObservedItem != null)
                {
                    UndoMod.Instsance.ObservedItem.Name = "Relocating building";
                }
                UndoMod.Instsance.RelocatingBuilding = true;
            }
        }

        // long
        private static void CreateBuilding_Postfix(int relocating)
        {
            if (relocating != 0 && UndoMod.Instsance.RelocatingBuilding)
            {
                UndoMod.Instsance.RelocatingBuilding = false;
            }
        }*/
    }
}
