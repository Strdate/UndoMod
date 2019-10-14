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
    public class BuildingManagerPatch
    {
        private static MethodInfo releaseBuilding_original = typeof(BuildingManager).GetMethod("ReleaseBuilding");
        //private static MethodInfo releaseBuilding_patch = typeof(BuildingManagerPatch).GetMethod("ReleaseBuilding", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo releaseBuilding_prefix = typeof(BuildingManagerPatch).GetMethod("ReleaseBuilding_Prefix", BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo releaseBuilding_postfix = typeof(BuildingManagerPatch).GetMethod("ReleaseBuilding_Postfix", BindingFlags.NonPublic | BindingFlags.Static);
        //public static RedirectCallsState releaseBuildingState;

        private static MethodInfo createBuilding_original = typeof(BuildingManager).GetMethod("CreateBuilding");
        private static MethodInfo createBuilding_patch = typeof(BuildingManagerPatch).GetMethod("CreateBuilding", BindingFlags.NonPublic | BindingFlags.Instance);
        public static RedirectCallsState createBuildingState;

        private static MethodInfo relocateBuilding_original = typeof(BuildingManager).GetMethod("RelocateBuilding");
        private static MethodInfo relocateBuilding_patch = typeof(BuildingManagerPatch).GetMethod("RelocateBuilding", BindingFlags.NonPublic | BindingFlags.Instance);
        public static RedirectCallsState relocateBuildingState;

        public static void Patch(HarmonyInstance _harmony)
        {
            //releaseBuildingState = RedirectionHelper.RedirectCalls(releaseBuilding_original, releaseBuilding_patch);
            _harmony.Patch(releaseBuilding_original, new HarmonyMethod(releaseBuilding_prefix), new HarmonyMethod(releaseBuilding_postfix));
            createBuildingState = RedirectionHelper.RedirectCalls(createBuilding_original, createBuilding_patch);
            relocateBuildingState = RedirectionHelper.RedirectCalls(relocateBuilding_original, relocateBuilding_patch);
        }

        public static void Unpatch(HarmonyInstance _harmony)
        {
            RedirectionHelper.RevertRedirect(relocateBuilding_original, relocateBuildingState);
            RedirectionHelper.RevertRedirect(createBuilding_original, createBuildingState);
            _harmony.Unpatch(releaseBuilding_original, releaseBuilding_prefix);
            _harmony.Unpatch(releaseBuilding_original, releaseBuilding_postfix);
            //RedirectionHelper.RevertRedirect(releaseBuilding_original, releaseBuildingState);
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

        private bool CreateBuilding(out ushort building, ref Randomizer randomizer, BuildingInfo info, Vector3 position, float angle, int length, uint buildIndex)
        {
            bool result;
            //Debug.Log("redirect");
            UndoMod.Instsance.ObservingOnlyBuildings++;
            RedirectionHelper.RevertRedirect(createBuilding_original, createBuildingState);
            try
            {
                result = BuildingManager.instance.CreateBuilding(out building, ref randomizer, info, position, angle, length, buildIndex);
            } finally
            {
                createBuildingState = RedirectionHelper.RedirectCalls(createBuilding_original, createBuilding_patch);
                UndoMod.Instsance.ObservingOnlyBuildings--;
            }

            if (result && !UndoMod.Instsance.PerformingAction && !UndoMod.Instsance.Invalidated)
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

            return result;
        }

        private void RelocateBuilding(ushort building, Vector3 position, float angle)
        {
            RedirectionHelper.RevertRedirect(relocateBuilding_original, relocateBuildingState);
            relocateBuildingState = RedirectionHelper.RedirectCalls(relocateBuilding_patch, relocateBuilding_original);
            try
            {
                RelocateBuilding(building, position, angle);
            } finally
            {
                RedirectionHelper.RevertRedirect(relocateBuilding_patch, relocateBuildingState);
                relocateBuildingState = RedirectionHelper.RedirectCalls(relocateBuilding_original, relocateBuilding_patch);
            }
            UndoMod.Instsance.InvalidateAll();
        }

        /*private void ReleaseBuilding(ushort building)
        {
            //Debug.Log("redirect");
            ref Building data = ref ManagerUtils.BuildingS(building);
            if ((data.m_flags != Building.Flags.None && (data.m_flags & Building.Flags.Deleted) == Building.Flags.None) && !UndoMod.Instsance.PerformingAction && !UndoMod.Instsance.Invalidated)
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

            UndoMod.Instsance.ObservingOnlyBuildings = true;
            //RedirectionHelper.RevertRedirect(releaseBuilding_original, releaseBuildingState);
            try
            {
                //BuildingManager.instance.ReleaseBuilding(building);
                ReleaseBuildingOriginal(building);
            }
            finally
            {
                //releaseBuildingState = RedirectionHelper.RedirectCalls(releaseBuilding_original, releaseBuilding_patch);
                UndoMod.Instsance.ObservingOnlyBuildings = false;
            }
        }*/
    }
}
