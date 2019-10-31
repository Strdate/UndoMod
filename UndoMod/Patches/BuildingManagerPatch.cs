using ColossalFramework.Math;
using Harmony;
using Redirection;
using SharedEnvironment;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UndoMod.Utils;
using UnityEngine;

namespace UndoMod.Patches
{
    public class BuildingManagerPatch
    {
        private static MethodInfo releaseBuilding_original = typeof(BuildingManager).GetMethod("ReleaseBuilding");
        //private static MethodInfo releaseBuilding_patch = typeof(BuildingManagerPatch).GetMethod("ReleaseBuilding", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo releaseBuilding_prefix = typeof(BuildingManagerPatch).GetMethod("ReleaseBuilding_Prefix", BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo releaseBuilding_postfix = typeof(BuildingManagerPatch).GetMethod("ReleaseBuilding_Postfix", BindingFlags.NonPublic | BindingFlags.Static);
        //public static RedirectCallsState releaseBuildingState;

        private static MethodInfo createBuilding_original = typeof(BuildingManager).GetMethod("CreateBuilding");
        private static MethodInfo createBuilding_prefix = typeof(BuildingManagerPatch).GetMethod("CreateBuilding_Prefix", BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo createBuilding_postfix = typeof(BuildingManagerPatch).GetMethod("CreateBuilding_Postfix", BindingFlags.NonPublic | BindingFlags.Static);

        private static MethodInfo relocateBuilding_original = typeof(BuildingManager).GetMethod("RelocateBuilding");
        private static MethodInfo relocateBuilding_postfix = typeof(BuildingManagerPatch).GetMethod("RelocateBuilding_Postfix", BindingFlags.NonPublic | BindingFlags.Static);

        public static void Patch(HarmonyInstance _harmony)
        {
            _harmony.Patch(releaseBuilding_original, new HarmonyMethod(releaseBuilding_prefix), new HarmonyMethod(releaseBuilding_postfix));
            _harmony.Patch(createBuilding_original, new HarmonyMethod(createBuilding_prefix), new HarmonyMethod(createBuilding_postfix));
            _harmony.Patch(relocateBuilding_original, null, new HarmonyMethod(relocateBuilding_postfix));
        }

        public static void Unpatch(HarmonyInstance _harmony)
        {
            _harmony.Unpatch(relocateBuilding_original, relocateBuilding_postfix);
            _harmony.Unpatch(createBuilding_original, createBuilding_prefix);
            _harmony.Unpatch(createBuilding_original, createBuilding_postfix);
            _harmony.Unpatch(releaseBuilding_original, releaseBuilding_prefix);
            _harmony.Unpatch(releaseBuilding_original, releaseBuilding_postfix);
        }

        private static void ReleaseBuilding_Prefix(ushort building)
        {
            //Debug.Log("ReleaseBuilding Prefix! " + building);
            ref Building data = ref ManagerUtils.BuildingS(building);
            if ((data.m_flags != Building.Flags.None && (data.m_flags & Building.Flags.Deleted) == Building.Flags.None) && !UndoMod.Instsance.PerformingAction
                    && !UndoMod.Instsance.Invalidated)
            {
                if (UndoMod.Instsance.Observing)
                {
                    try
                    {
                        var constructable = UndoMod.Instsance.WrappersDictionary.RegisterBuilding(building);
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
                    //Invalidator.Instance.InvalidBuildings.Add(building);
                }
            }

            UndoMod.Instsance.ObservingOnlyBuildings++;
        }

        private static void ReleaseBuilding_Postfix()
        {
            UndoMod.Instsance.ObservingOnlyBuildings--;
        }

        private static void CreateBuilding_Prefix()
        {
            UndoMod.Instsance.ObservingOnlyBuildings++;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool CheckCaller()
        {
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
            System.Diagnostics.StackFrame[] frames = stackTrace.GetFrames();

            for(int i = 3; i <= 4; i++)
            {
                if(frames[i].GetMethod().Name == "UpdateBuilding" && frames[i].GetMethod().DeclaringType == typeof(NetNode))
                {
                    Debug.Log("Wrong caller!!");
                    return false;
                }
            }

            return true;
        }

        private static void CreateBuilding_Postfix(bool __result, ref ushort building)
        {
            UndoMod.Instsance.ObservingOnlyBuildings--;
            if (__result && !UndoMod.Instsance.PerformingAction && !UndoMod.Instsance.Invalidated && CheckCaller())
            {
                if (UndoMod.Instsance.Observing)
                {
                    try
                    {
                        var constructable = UndoMod.Instsance.WrappersDictionary.RegisterBuilding(building);
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
                    //Invalidator.Instance.InvalidBuildings.Add(building);
                }
            }
        }

        private static void RelocateBuilding_Postfix()
        {
            UndoMod.Instsance.InvalidateAll(false);
        }
    }
}
