using ColossalFramework.Math;
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
        private static MethodInfo releaseBuilding_patch = typeof(BuildingManagerPatch).GetMethod("ReleaseBuilding", BindingFlags.NonPublic | BindingFlags.Instance);
        public static RedirectCallsState releaseBuildingState;

        private static MethodInfo createBuilding_original = typeof(BuildingManager).GetMethod("CreateBuilding");
        private static MethodInfo createBuilding_patch = typeof(BuildingManagerPatch).GetMethod("CreateBuilding", BindingFlags.NonPublic | BindingFlags.Instance);
        public static RedirectCallsState createBuildingState;

        public static void Patch()
        {
            releaseBuildingState = RedirectionHelper.RedirectCalls(releaseBuilding_original, releaseBuilding_patch);
            createBuildingState = RedirectionHelper.RedirectCalls(createBuilding_original, createBuilding_patch);

        }

        public static void Unpatch()
        {
            RedirectionHelper.RevertRedirect(releaseBuilding_original, releaseBuildingState);
            RedirectionHelper.RevertRedirect(createBuilding_original, createBuildingState);
        }

        private void ReleaseBuilding(ushort building)
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
                    Invalidator.Instance.InvalidBuildings.Add(building);
                }
            }

            RedirectionHelper.RevertRedirect(releaseBuilding_original, releaseBuildingState);
            BuildingManager.instance.ReleaseBuilding(building);
            releaseBuildingState = RedirectionHelper.RedirectCalls(releaseBuilding_original, releaseBuilding_patch);
        }

        private bool CreateBuilding(out ushort building, ref Randomizer randomizer, BuildingInfo info, Vector3 position, float angle, int length, uint buildIndex)
        {
            //Debug.Log("redirect");
            RedirectionHelper.RevertRedirect(createBuilding_original, createBuildingState);
            bool result = BuildingManager.instance.CreateBuilding(out building, ref randomizer, info, position, angle, length, buildIndex);
            createBuildingState = RedirectionHelper.RedirectCalls(createBuilding_original, createBuilding_patch);

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
                    Invalidator.Instance.InvalidBuildings.Add(building);
                }
            }

            return result;
        }
    }
}
