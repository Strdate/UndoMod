using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace UndoMod.Patches
{
    [HarmonyPatch(typeof(NetTool))]
    [HarmonyPatch("CreateNodeImpl", new Type[] { typeof(bool) })]
    class NetToolPatch
    {
        static void Prefix()
        {
            UndoMod.Instsance.BeginObserving("Build roads");
        }

        static void Finalizer(Exception __exception)
        {
            UndoMod.Instsance.FinalizeObserving(__exception);
        }
    }
}