using System.Collections.Generic;
using UndoMod;
using UndoMod.Utils;

namespace SharedEnvironment
{
    public class WrappersDictionary
    {
        public Dictionary<ushort, WrappedNode> RegisteredNodes;

        public Dictionary<ushort, WrappedSegment> RegisteredSegments;

        public Dictionary<ushort, WrappedBuilding> RegisteredBuildings;

        public Dictionary<ushort, WrappedProp> RegisteredProps;

        public Dictionary<uint, WrappedTree> RegisteredTrees;

        public WrappersDictionary()
        {
            Init();
        }

        private void Init()
        {
            RegisteredNodes = new Dictionary<ushort, WrappedNode>();
            RegisteredSegments = new Dictionary<ushort, WrappedSegment>();
            RegisteredBuildings = new Dictionary<ushort, WrappedBuilding>();
            RegisteredProps = new Dictionary<ushort, WrappedProp>();
            RegisteredTrees = new Dictionary<uint, WrappedTree>();
        }

        public void Clear()
        {
            Init();
        }

        public WrappedTree RegisterTree(uint id)
        {
            if(Invalidator.Instance.InvalidTrees.Contains(id))
            {
                UndoMod.UndoMod.Instsance.InvalidateQueue();
            }

            if (id == 0)
                return null;

            WrappedTree tree;
            if (!RegisteredTrees.TryGetValue(id, out tree))
            {
                tree = new WrappedTree(id);
                RegisteredTrees[id] = tree;
            }

            return tree;
        }

        public WrappedProp RegisterProp(ushort id)
        {
            if (Invalidator.Instance.InvalidProps.Contains(id))
            {
                UndoMod.UndoMod.Instsance.InvalidateQueue();
            }

            if (id == 0)
                return null;

            WrappedProp prop;
            if (!RegisteredProps.TryGetValue(id, out prop))
            {
                prop = new WrappedProp(id);
                RegisteredProps[id] = prop;
            }

            return prop;
        }

        public WrappedBuilding RegisterBuilding(ushort id)
        {
            if (Invalidator.Instance.InvalidBuildings.Contains(id))
            {
                UndoMod.UndoMod.Instsance.InvalidateQueue();
            }

            if (id == 0)
                return null;

            WrappedBuilding building;
            if (!RegisteredBuildings.TryGetValue(id, out building))
            {
                building = new WrappedBuilding(id);
                RegisteredBuildings[id] = building;
            }

            return building;
        }

        public WrappedNode RegisterNode(ushort id)
        {
            if (Invalidator.Instance.InvalidNodes.Contains(id))
            {
                UndoMod.UndoMod.Instsance.InvalidateQueue();
            }

            if (id == 0)
                return null;

            WrappedNode node;
            if (!RegisteredNodes.TryGetValue(id, out node))
            {
                node = new WrappedNode(id);
                RegisteredNodes[id] = node;
            }

            return node;
        }

        public WrappedSegment RegisterSegment(ushort id)
        {
            if (Invalidator.Instance.InvalidSegments.Contains(id))
            {
                UndoMod.UndoMod.Instsance.InvalidateQueue();
            }

            if (id == 0)
                return null;

            WrappedSegment segment;
            if (!RegisteredSegments.TryGetValue(id, out segment))
            {
                var node1 = RegisterNode(NetUtil.Segment(id).m_startNode);
                var node2 = RegisterNode(NetUtil.Segment(id).m_endNode);
                segment = new WrappedSegment(node1, node2, id);
                RegisteredSegments[id] = segment;
            }

            return segment;
        }

        //

        public void CollectGarbage(bool force = false)
        {
            List<IActionQueueItem> items = UndoMod.UndoMod.Instsance.Queue.AssembleAll();
            if(UndoMod.UndoMod.Instsance.ObservedItem != null)
            {
                items.Add(UndoMod.UndoMod.Instsance.ObservedItem);
            }

            Queue<IConstructable> constructables = new Queue<IConstructable>();
            foreach(var item in items)
            {
                ActionQueueItem item2 = item as ActionQueueItem;
                if(item2 != null)
                {
                    item2.Actions.ForEach((a) => {
                        var constructable = a as ConstructionAction;
                        if(a != null)
                        {
                            constructables.Enqueue(constructable.Item);
                        }
                    });
                }
            }

            //
        }
    }
}
