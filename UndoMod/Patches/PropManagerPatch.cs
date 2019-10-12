using ColossalFramework.Math;
using Redirection;
using SharedEnvironment;
using System;
using System.Reflection;
using UndoMod.Utils;
using UnityEngine;

namespace UndoMod.Patches
{
    public class PropManagerPatch
    {
        private static MethodInfo releaseProp_original = typeof(PropManager).GetMethod("ReleaseProp");
        private static MethodInfo releaseProp_patch = typeof(PropManagerPatch).GetMethod("ReleaseProp", BindingFlags.NonPublic | BindingFlags.Instance);
        public static RedirectCallsState releasePropState;

        private static MethodInfo createProp_original = typeof(PropManager).GetMethod("CreateProp");
        private static MethodInfo createProp_patch = typeof(PropManagerPatch).GetMethod("CreateProp", BindingFlags.NonPublic | BindingFlags.Instance);
        public static RedirectCallsState createPropState;

        public static void Patch()
        {
            releasePropState = RedirectionHelper.RedirectCalls(releaseProp_original, releaseProp_patch);
            createPropState = RedirectionHelper.RedirectCalls(createProp_original, createProp_patch);

        }

        public static void Unpatch()
        {
            RedirectionHelper.RevertRedirect(releaseProp_original, releasePropState);
            RedirectionHelper.RevertRedirect(createProp_original, createPropState);
        }

        private bool CheckIfObserving()
        {
            return !UndoMod.Instsance.PerformingAction && !UndoMod.Instsance.Invalidated && !UndoMod.Instsance.ObservingOnlyBuildings;
        }

        private void ReleaseProp(ushort Prop)
        {
            //Debug.Log("redirect");
            ref PropInstance data = ref ManagerUtils.Prop(Prop);
            if (data.m_flags != 0 && CheckIfObserving())
            {
                if (UndoMod.Instsance.Observing)
                {
                    try
                    {
                        var constructable = UndoMod.Instsance.WrappersDictionary.RegisterProp(Prop);
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
                    Invalidator.Instance.InvalidProps.Add(Prop);
                }
            }

            RedirectionHelper.RevertRedirect(releaseProp_original, releasePropState);
            PropManager.instance.ReleaseProp(Prop);
            releasePropState = RedirectionHelper.RedirectCalls(releaseProp_original, releaseProp_patch);
        }

        private bool CreateProp(out ushort prop, ref Randomizer randomizer, PropInfo info, Vector3 position, float angle, bool single)
        {
            //Debug.Log("redirect");
            RedirectionHelper.RevertRedirect(createProp_original, createPropState);
            bool result = PropManager.instance.CreateProp(out prop, ref randomizer, info, position, angle, single);
            createPropState = RedirectionHelper.RedirectCalls(createProp_original, createProp_patch);

            if (result && CheckIfObserving())
            {
                if (UndoMod.Instsance.Observing)
                {
                    try
                    {
                        var constructable = UndoMod.Instsance.WrappersDictionary.RegisterProp(prop);
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
                    Invalidator.Instance.InvalidProps.Add(prop);
                }
            }

            return result;
        }
    }
}
