using ColossalFramework;
using ColossalFramework.UI;
using SharedEnvironment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UndoMod.Patches;
using UndoMod.Utils;
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

        public int ObservingOnlyBuildings { get; set; }

        public long ObservedCashBalance { get; private set; }
        public ActionQueueItem ObservedItem { get; private set; }
        
        public WrappersDictionary WrappersDictionary { get; private set; } 

        public ActionQueue Queue { get; private set; }
        public bool PerformingAction { get; private set; }
        public bool Invalidated { get; private set; }

        public UndoMod()
        {
            ActionQueueItem.exceptionHandler = (a, e) => { InvalidateAll(); return false; };
            Queue = new ActionQueue(ModInfo.sa_queueCapacity);
            WrappersDictionary = new WrappersDictionary();
            WrappedBuilding.dictionary = WrappersDictionary;
        }

        public void ChangeQueueCapacity(int val)
        {
            InvalidateAll();
            Queue = new ActionQueue(val);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ReportObservedAction(IGameAction action)
        {
            if (Observing)
            {
                ObservedItem.Actions.Add(action);
                if(ObservedItem.ModName == "")
                {
                    try
                    {
                        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
                        System.Diagnostics.StackFrame[] stackFrames = stackTrace.GetFrames();
                        ObservedItem.ModName = stackFrames[2].GetMethod().DeclaringType.Assembly.GetName().Name;
                    } catch { Debug.LogError("Failed to retrieve calling assembly name"); }
                }
            }
        }

        public void BeginObserving(string actionName/*, bool onlyBuildings = false*/, string modname = "Vanilla", bool autoObserving = false)
        {
            if(!LoadingExtension.Instsance.m_detoured)
            {
                return;
            }
            if(autoObserving && Observing && ObservedItem != null && !ObservedItem.AutoObserving)
            {
                return;
            }
            if(Observing)
            {
                EndObserving();
            }
            ObservedItem = new ActionQueueItem(actionName);
            ObservedItem.ModName = modname;
            ObservedItem.AutoObserving = autoObserving;
            ObservedCashBalance = EconomyManager.instance.InternalCashAmount;
            Observing = true;
            Invalidated = false;
            //ObservingOnlyBuildings = onlyBuildings;
        }

        public void FinalizeObserving(Exception e)
        {
            if (e != null) {
                Debug.LogError(e);
                InvalidateAll();
            }
            EndObserving();
        }

        public void EndObserving()
        {
            if(Observing)
            {
                if(ObservedItem.Actions.Count > 0)
                {
                    long moneyDelta = ObservedCashBalance - EconomyManager.instance.InternalCashAmount;
                    ObservedItem.DoCost = (int)moneyDelta;
                    Queue.Push(ObservedItem);
                    //Debug.Log("Pushing: " + ObservedItem);
                }
                ObservedItem = null;
                Observing = false;
                ObservingOnlyBuildings = 0;
            }
        }

        public void InvalidateAll(bool error = true)
        {
            if(error)
            {
                Debug.LogWarning("Error: Invalidate all");
                Singleton<SimulationManager>.instance.AddAction(() => CleanGhostNodes());
            }
            Queue.Clear();
            WrappersDictionary.Clear();
            //Invalidator.Instance.Clear();
            Observing = false;
            ObservingOnlyBuildings = 0;
            Invalidated = true;
        }

        public void Undo()
        {
            IActionQueueItem item = Queue.Previous();
            UndoRedoImpl(item, false);
        }

        public void Redo()
        {
            IActionQueueItem item = Queue.Next();
            UndoRedoImpl(item, true);
        }

        private void UndoRedoImpl(IActionQueueItem item, bool redo)
        {
            if (item != null)
            {
                if (ModInfo.sa_ignoreCosts.value || !LoadingExtension.Instsance.m_inStandardGame)
                {
                    var aitem = item as ActionQueueItem;
                    if (aitem != null)
                    {
                        aitem.DoCost = 0;
                    }
                }
                Singleton<SimulationManager>.instance.AddAction(() => {
                    Debug.Log("Action " + item);
                    PerformingAction = true;
                    if (!(redo ? item.Redo() : item.Undo()))
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

        private void CleanGhostNodes()
        {
            int count = 0;

            // From moveit
            for (ushort nodeId = 0; nodeId < NetManager.instance.m_nodes.m_buffer.Length && count < 10; nodeId++)
            {
                NetNode node = NetManager.instance.m_nodes.m_buffer[nodeId];
                if ((node.m_flags & NetNode.Flags.Created) == NetNode.Flags.None) continue;
                if ((node.m_flags & NetNode.Flags.Untouchable) != NetNode.Flags.None) continue;
                bool hasSegments = false;

                for (int i = 0; i < 8; i++)
                {
                    if (node.GetSegment(i) > 0)
                    {
                        hasSegments = true;
                        break;
                    }
                }

                if (!hasSegments)
                {
                    count++;
                    NetUtil.ReleaseNode(nodeId);
                }
            }

            if (count > 0)
            {
                Debug.Log($"Removed {count} ghost nodes");
            }
        }
    }
}
