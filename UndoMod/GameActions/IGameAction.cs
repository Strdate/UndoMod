using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SharedEnvironment
{
    public interface IGameAction
    {
        void Do();
        void Undo();
        void Redo();

        int DoCost();
        int UndoCost();
    }

    /*public class ActionException : Exception
    {
        public GameAction Action { get; private set; }

        public ActionException(string description, GameActionExtended action) : base(ModifyMessage(description,action))
        {
            this.Action = action;
        }

        private static string ModifyMessage(string description, GameActionExtended action)
        {
            return "Action " + action.Name + ": " + description;
        }
    }*/
}
