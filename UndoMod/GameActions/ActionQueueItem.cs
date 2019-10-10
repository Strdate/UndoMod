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

        public List<GameAction> Actions = new List<GameAction>();

        public ActionQueueItem(string name)
        {
            Name = name;
        }

        public void Do()
        {
            foreach (GameAction action in Actions)
            {
                try
                {
                    action.Do();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        public void Redo()
        {
            foreach (GameAction action in Actions)
            {
                try
                {
                    action.Redo();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        public void Undo()
        {
            for (int i = Actions.Count - 1; i >= 0; i--)
            {
                try
                {
                    Actions[i].Undo();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }
    }
}
