using HarmonyLib;
using System;
using UnityEngine;

namespace UndoMod.Patches
{
    [HarmonyPatch(typeof(TreeTool))]
    [HarmonyPatch("CreateTree", new Type[] { })]
    class TreeToolPatch
    {
        static void Prefix()
        {
            UndoMod.Instsance.BeginObserving("Build tree", autoTerminate: true);
        }
    }
}
