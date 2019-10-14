﻿using ColossalFramework;
using Redirection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace UndoMod.Patches
{
    public class NetToolPatch
    {
        public static MethodInfo createNode_original = typeof(NetTool).GetMethod("CreateNodeImpl", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(bool) }, null);
        public static MethodInfo patch = typeof(NetToolPatch).GetMethod("CreateNodeImpl", BindingFlags.NonPublic | BindingFlags.Instance);
        public static RedirectCallsState state;
        /*private static MethodInfo createNode_prefix = typeof(NetToolPatch).GetMethod("CreateNode_prefix", BindingFlags.NonPublic);
        private static MethodInfo createNode_postfix = typeof(NetToolPatch).GetMethod("CreateNode_postfix", BindingFlags.NonPublic);
        private static MethodInfo createNode_patch = typeof(NetToolPatch).GetMethod("CreateNode_postfix", BindingFlags.NonPublic);

        public static void Patch(HarmonyInstance _harmony)
        {
            _harmony.Patch(createNode_original, new HarmonyMethod(createNode_prefix), new HarmonyMethod(createNode_postfix));
        }

        public static void Unpatch(HarmonyInstance _harmony)
        {
            _harmony.Unpatch(createNode_original, createNode_prefix);
            _harmony.Unpatch(createNode_original, createNode_postfix);
        }*/

        public static void Patch()
        {
            state = RedirectionHelper.RedirectCalls(createNode_original, patch);
        }

        public static void Unpatch()
        {
            RedirectionHelper.RevertRedirect(createNode_original, state);
        }

        private bool CreateNodeImpl(bool switchDirection)
        {
            //Debug.Log("CreateNode detour");
            UndoMod.Instsance.BeginObserving("Build roads", "Vanilla");
            bool result = false;

            RedirectionHelper.RevertRedirect(createNode_original, state);
            state = RedirectionHelper.RedirectCalls(patch, createNode_original);
            try
            {
                //result = (bool)createNode_original.Invoke(ToolsModifierControl.GetTool<NetTool>(), new object[] { switchDirection });
                result = CreateNodeImpl(switchDirection);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                UndoMod.Instsance.InvalidateAll();
            }
            RedirectionHelper.RevertRedirect(patch, state);
            state = RedirectionHelper.RedirectCalls(createNode_original, patch);

            UndoMod.Instsance.EndObserving();
            return result;
        }

        /*private static void CreateNode_prefix()
        {
            UndoMod.Instsance.BeginObserving("Build roads");
        }

        private static void CreateNode_postfix()
        {
            UndoMod.Instsance.EndObserving();
        }*/
    }
}
