using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SharedEnvironment
{
    public class ActionQueueItem : IActionQueueItem
    {
        public string Name { get; set; }
        public string ModName { get; set; }

        public string InfoString { get => "Cost: " + DoCost; }

        public bool AutoObserving { get; set; }

        public List<IGameAction> Actions = new List<IGameAction>();

        public int DoCost { get; set; }

        public delegate bool ExceptionHandler(IGameAction action, Exception exception);
        public static ExceptionHandler exceptionHandler { get; set; }

        public ActionQueueItem(string name)
        {
            Name = name;
        }

        public bool Do()
        {
            if (!HandleMoney(DoCost))
            {
                return false;
            }
            foreach (IGameAction action in Actions)
            {
                try
                {
                    action.Do();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    if(exceptionHandler != null)
                    {
                        if (!exceptionHandler.Invoke(action, e))
                            break;
                    }
                }
            }
            return true;
        }

        public bool Redo()
        {
            if (!HandleMoney(DoCost))
            {
                return false;
            }
            foreach (IGameAction action in Actions)
            {
                try
                {
                    action.Redo();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    if (exceptionHandler != null)
                    {
                        if (!exceptionHandler.Invoke(action, e))
                            break;
                    }
                }
            }
            return true;
        }

        public bool Undo()
        {
            if(!HandleMoney(-DoCost))
            {
                return false;
            }
            for (int i = Actions.Count - 1; i >= 0; i--)
            {
                try
                {
                    Actions[i].Undo();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    if (exceptionHandler != null)
                    {
                        if (!exceptionHandler.Invoke(Actions[i], e))
                            break;
                    }
                }
            }
            return true;
        }

        private static bool HandleMoney(int amount)
        {
            if (amount == 0)
                return true;
            else if(amount < 0)
            {
                Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.RefundAmount, -amount, PrefabCollection<NetInfo>.GetPrefab(0).m_class);
                return true;
            } else if(Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Construction, amount, PrefabCollection<NetInfo>.GetPrefab(0).m_class) != amount)
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return "[Action " + Name + (ModName != null ? "(" + ModName + ")" : "") + ", count: " + Actions.Count + ", cost: " +  DoCost + "]";
        }
    }
}
