using ColossalFramework;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace UndoMod.Patches
{
    [HarmonyPatch(typeof(BuildingTool))]
    [HarmonyPatch("CreateBuilding", new Type[] { })]
    public class BuildingToolPatch
    {
        static void Prefix()
        {
            UndoMod.Instsance.BeginObserving("Build building", autoTerminate: true);
        }
    }
}
