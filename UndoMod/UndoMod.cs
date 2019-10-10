using ColossalFramework;
using SharedEnvironment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UndoMod.Patches;
using UnityEngine;

namespace UndoMod
{
    public class UndoMod
    {
        private static UndoMod _instance;
        public static UndoMod Instsance { get
            {
                if (_instance == null)
                    _instance = new UndoMod();
                return _instance;
            }
        }

        public ActionQueueItem ObservedItem { get; set; }
        public bool PerformingAction { get; set; }
        public WrappersDictionary WrappersDictionary { get; set; } = new WrappersDictionary();

        public IActionQueueItem ActionQueueItem; // todo change to queue

        public void BeginObserving(string actionName)
        {
            ObservedItem = new ActionQueueItem(actionName);
        }

        public void EndObserving()
        {
            if(ObservedItem.Actions.Count > 0)
            {
                ActionQueueItem = ObservedItem;
                ObservedItem = null;
            }
        }

        public void UndoLast()
        {
            Debug.Log(NetToolPatch.createNode_original);
            if (ActionQueueItem != null)
            {
                Singleton<SimulationManager>.instance.AddAction(() => {
                    PerformingAction = true;
                    ActionQueueItem.Undo();
                    ActionQueueItem = null;
                    PerformingAction = false;
                });
            }
        }
    }
}
