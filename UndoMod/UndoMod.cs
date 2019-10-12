using ColossalFramework;
using ColossalFramework.UI;
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

        public bool Observing { get; set; }
        public bool ObservingOnlyBuildings { get; set; }
        public long ObservedCashBalance { get; set; }
        public ActionQueueItem ObservedItem { get; set; }
        
        public WrappersDictionary WrappersDictionary { get; set; } = new WrappersDictionary();

        public ActionQueue Queue { get; private set; }
        public bool PerformingAction { get; set; }
        public bool Invalidated { get; set; }

        public UndoMod()
        {
            ActionQueueItem.exceptionHandler = (a, e) => { InvalidateAll(); return false; };
            Queue = new ActionQueue(10);
        }

        public void ReportObservedAction(IGameAction action)
        {
            if (Observing)
            {
                ObservedItem.Actions.Add(action);
            }
        }

        public void BeginObserving(string actionName, bool onlyBuildings = false)
        {
            if(!LoadingExtension.Instsance.m_detoured)
            {
                return;
            }
            if(Observing)
            {
                EndObserving();
            }
            ObservedItem = new ActionQueueItem(actionName);
            ObservedCashBalance = EconomyManager.instance.InternalCashAmount;
            Observing = true;
            Invalidated = false;
            ObservingOnlyBuildings = onlyBuildings;
        }

        public void EndObserving()
        {
            if(Observing && ObservedItem.Actions.Count > 0)
            {
                long moneyDelta = ObservedCashBalance - EconomyManager.instance.InternalCashAmount;
                ObservedItem.DoCost = (int)moneyDelta;
                Queue.Push(ObservedItem);
                ObservedItem = null;
                Observing = false;
                ObservingOnlyBuildings = false;
            }
            WrappersDictionary.CollectGarbage();
        }

        public void InvalidateAll()
        {
            Debug.LogWarning("Invalidate all");
            Queue.Clear();
            WrappersDictionary.Clear();
            Invalidator.Instance.Clear();
            Observing = false;
            ObservingOnlyBuildings = false;
            Invalidated = true;
        }

        public void InvalidateQueue()
        {
            Debug.LogWarning("Invalidate queue");
            Queue.Clear();
            Invalidator.Instance.Clear();
            WrappersDictionary.CollectGarbage(force: true);
        }

        public void Undo()
        {
            IActionQueueItem item = Queue.Previous();
            if (item != null)
            {
                Debug.Log("Undo" + item);
                Singleton<SimulationManager>.instance.AddAction(() => {
                    PerformingAction = true;
                    if(!item.Undo())
                    {
                        InvalidateAll();
                        PlayDisabledSound();
                    }
                    else
                    {
                        PlayEnabledSound();
                    }
                    PerformingAction = false;
                });
            }
            else
            {
                PlayDisabledSound();
            }
        }

        public void Redo()
        {
            IActionQueueItem item = Queue.Next();
            if (item != null)
            {
                Debug.Log("Redo " + item);
                Singleton<SimulationManager>.instance.AddAction(() => {
                    PerformingAction = true;
                    if(!item.Redo())
                    {
                        InvalidateAll();
                        PlayDisabledSound();
                    }
                    else
                    {
                        PlayEnabledSound();
                    }
                    PerformingAction = false;
                });
            }
            else
            {
                PlayDisabledSound();
            }
        }

        private void PlayEnabledSound()
        {
            if (UIView.GetAView().defaultClickSound != null && UIView.playSoundDelegate != null)
            {
                UIView.playSoundDelegate(UIView.GetAView().defaultClickSound, 1f);
            }
        }

        private void PlayDisabledSound()
        {
            if (UIView.GetAView().defaultDisabledClickSound != null && UIView.playSoundDelegate != null)
            {
                UIView.playSoundDelegate(UIView.GetAView().defaultDisabledClickSound, 1f);
            }
        }
    }
}
