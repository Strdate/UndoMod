using ColossalFramework;
using ICities;
using SharedEnvironment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UndoMod.UI;
using UnityEngine;

namespace UndoMod
{
    public class Threading : ThreadingExtensionBase
    {
        private static bool _processed = false;

        private static DateTime _lastBeginObserving = DateTime.Now;

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (ModInfo.sc_undo.IsPressed())
            {
                if(!_processed)
                {
                    if(CheckCurrentTool())
                        Singleton<SimulationManager>.instance.AddAction(() => {
                            UndoMod.Instsance.Undo();
                        });
                    _processed = true;
                }
            } else if (ModInfo.sc_redo.IsPressed())
            {
                if (!_processed)
                {
                    if (CheckCurrentTool())
                        Singleton<SimulationManager>.instance.AddAction(() => {
                            UndoMod.Instsance.Redo();
                        });
                    _processed = true;
                }
            }
            else if (ModInfo.sc_peek.IsPressed())
            {
                if (!_processed)
                {
                    if (CheckCurrentTool())
                        PeekUndoPanel.Instance.Enable();
                    _processed = true;
                }
            }
            else
            {
                PeekUndoPanel.Instance.Disable();
                _processed = false;
            }

            ScheduledObserving();
        }

        private bool CheckCurrentTool()
        {
            if(!ModInfo.sa_disableShortcuts.value)
            {
                return true;
            }

            ToolBase tool = ToolsModifierControl.toolController.CurrentTool;
            string toolName = tool.GetType().Name;
            if (
                tool is DefaultTool ||
                tool is NetTool ||
                tool is BuildingTool ||
                tool is PropTool ||
                tool is TreeTool ||
                toolName == "ForestTool")
            { return true; }

            return false;
        }

        private static void ScheduledObserving()
        {
            if(LoadingExtension.Instsance.m_detoured)
            {
                if(_lastBeginObserving.AddSeconds(1) < DateTime.Now)
                {
                    _lastBeginObserving = DateTime.Now;
                    UndoMod.Instsance.BeginObserving("<unknown>", "", true);
                }
            }
        }
    }
}
