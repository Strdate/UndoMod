using System.Collections.Generic;
using System.Linq;
using UndoMod;
using UndoMod.Utils;
using UnityEngine;

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

        public WrappedBuilding RegisterBuilding(ushort id, bool checkConnectedBuildings = true)
        {
            if (id == 0)
                return null;

            WrappedBuilding building;
            if (!RegisteredBuildings.TryGetValue(id, out building))
            {
                building = new WrappedBuilding(id, checkConnectedBuildings);
                RegisteredBuildings[id] = building;
            }

            return building;
        }

        public WrappedNode RegisterNode(ushort id)
        {
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

        public void CollectGarbage()
        {
            /*Debug.Log("Collect garbage");

            if((RegisteredNodes.Count + RegisteredSegments.Count + RegisteredBuildings.Count + RegisteredProps.Count + RegisteredTrees.Count)
                //< UndoMod.UndoMod.Instsance.Queue.Length() * 30)
                < 50)
            {
                return;
            }

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

            HashSet<WrappedNode> collectedNodes = new HashSet<WrappedNode>();
            HashSet<WrappedSegment> collectedSegments = new HashSet<WrappedSegment>();
            HashSet<WrappedBuilding> collectedBuildings = new HashSet<WrappedBuilding>();
            HashSet<WrappedProp> collectedProps = new HashSet<WrappedProp>();
            HashSet<WrappedTree> collectedTrees = new HashSet<WrappedTree>();

            while(constructables.Count > 0)
            {
                IConstructable item = constructables.Dequeue();
                var node = item as WrappedNode;
                var segment = item as WrappedSegment;
                var building = item as WrappedBuilding;
                var prop = item as WrappedProp;
                var tree = item as WrappedTree;
                if (node != null)
                {
                    collectedNodes.Add(node);
                }
                else if (segment != null)
                {
                    if(collectedSegments.Add(segment))
                    {
                        try
                        {
                            collectedNodes.Add(segment.StartNode);
                            collectedNodes.Add(segment.EndNode);
                        } catch { Debug.LogWarning("Garbage collection: invalid segment"); }
                    }
                }
                else if (building != null)
                {
                    collectedBuildings.Add(building);
                }
                else if (prop != null)
                {
                    collectedProps.Add(prop);
                }
                else if (tree != null)
                {
                    collectedTrees.Add(tree);
                }
            }

            RegisteredNodes = RegisteredNodes.Where(i => collectedNodes.Contains(i.Value)).ToDictionary(i => i.Key, i => i.Value);
            RegisteredSegments = RegisteredSegments.Where(i => collectedSegments.Contains(i.Value)).ToDictionary(i => i.Key, i => i.Value);
            RegisteredBuildings = RegisteredBuildings.Where(i => collectedBuildings.Contains(i.Value)).ToDictionary(i => i.Key, i => i.Value);
            RegisteredProps = RegisteredProps.Where(i => collectedProps.Contains(i.Value)).ToDictionary(i => i.Key, i => i.Value);
            RegisteredTrees = RegisteredTrees.Where(i => collectedTrees.Contains(i.Value)).ToDictionary(i => i.Key, i => i.Value);*/
        }
    }
}
