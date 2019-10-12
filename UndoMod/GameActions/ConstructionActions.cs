using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SharedEnvironment
{
    public interface ConstructionAction : IGameAction
    {
        IConstructable Item { get; }
    }

    public class ActionCreate : ConstructionAction
    {
        public IConstructable Item { get; private set; }

        public ActionCreate(IConstructable item)
        {
            Item = item;
        }

        public void Do()
        {
            Debug.Log("Action: Creating item: " + Item.ToString());
            Item.Create();
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            Debug.Log("Action: Releasing item: " + Item.ToString());
            Item.Release();
        }

        public int DoCost()
        {
            return Item.ConstructionCost();
        }

        public int UndoCost()
        {
            return - Item.ConstructionCost();
        }
    }

    public class ActionRelease : ConstructionAction
    {
        public IConstructable Item { get; private set; }

        public ActionRelease(IConstructable item)
        {
            Item = item;
        }

        public void Do()
        {
            Debug.Log("Action: Releasing item: " + Item.ToString());
            Item.Release();
        }

        public void Redo()
        {
            Do();
        }

        public void Undo()
        {
            Debug.Log("Action: Creating item: " + Item.ToString());
            Item.Create();
        }

        public int DoCost()
        {
            return -Item.ConstructionCost();
        }

        public int UndoCost()
        {
            return Item.ConstructionCost();
        }
    }
}
