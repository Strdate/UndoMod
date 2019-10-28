using System.Collections.Generic;
using System.Linq;
using System;
using UndoMod.Utils;
using UnityEngine;

namespace SharedEnvironment
{
    public class WrappersDictionary
    {
        private HashSet<WeakReference> BuildingOwnedNodes;

        private Dictionary<ushort, WeakReference> RegisteredNodes;

        private Dictionary<ushort, WeakReference> RegisteredSegments;

        private Dictionary<ushort, WeakReference> RegisteredBuildings;

        private Dictionary<ushort, WeakReference> RegisteredProps;

        private Dictionary<uint, WeakReference> RegisteredTrees;

        public WrappersDictionary()
        {
            Init();
        }

        private void Init()
        {
            RegisteredNodes = new Dictionary<ushort, WeakReference>();
            RegisteredSegments = new Dictionary<ushort, WeakReference>();
            RegisteredBuildings = new Dictionary<ushort, WeakReference>();
            RegisteredProps = new Dictionary<ushort, WeakReference>();
            RegisteredTrees = new Dictionary<uint, WeakReference>();

            BuildingOwnedNodes = new HashSet<WeakReference>();
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
            WeakReference reference;
            if (!RegisteredTrees.TryGetValue(id, out reference) || (tree = (WrappedTree)reference.Target) == null)
            {
                tree = new WrappedTree(id);
                RegisteredTrees[id] = new WeakReference(tree);
            }

            return tree;
        }

        public WrappedProp RegisterProp(ushort id)
        {
            if (id == 0)
                return null;

            WrappedProp prop;
            WeakReference reference;
            if (!RegisteredProps.TryGetValue(id, out reference) || (prop = (WrappedProp)reference.Target) == null)
            {
                prop = new WrappedProp(id);
                RegisteredProps[id] = new WeakReference(prop);
            }

            return prop;
        }

        public WrappedBuilding RegisterBuilding(ushort id, bool checkConnectedBuildings = true)
        {
            if (id == 0)
                return null;

            WrappedBuilding building;
            WeakReference reference;
            if (!RegisteredBuildings.TryGetValue(id, out reference) || (building = (WrappedBuilding)reference.Target) == null)
            {
                building = new WrappedBuilding(id, checkConnectedBuildings);
                RegisteredBuildings[id] = new WeakReference(building);
            }

            return building;
        }

        public WrappedNode RegisterNode(ushort id)
        {
            if (id == 0)
                return null;

            WrappedNode node;
            WeakReference reference;
            if (!RegisteredNodes.TryGetValue(id, out reference) || (node = (WrappedNode)reference.Target) == null)
            {
                node = new WrappedNode(id);
                RegisteredNodes[id] = new WeakReference(node);
            }

            return node;
        }

        public WrappedSegment RegisterSegment(ushort id)
        {
            if (id == 0)
                return null;

            WrappedSegment segment;
            WeakReference reference;
            if (!RegisteredSegments.TryGetValue(id, out reference) || (segment = (WrappedSegment)reference.Target) == null)
            {
                var node1 = RegisterNode(NetUtil.Segment(id).m_startNode);
                var node2 = RegisterNode(NetUtil.Segment(id).m_endNode);
                segment = new WrappedSegment(node1, node2, id);
                RegisteredSegments[id] = new WeakReference(segment);
            }

            return segment;
        }

        public void AddBuildingNode(WrappedNode node)
        {
            BuildingOwnedNodes.Add(new WeakReference(node));
        }

        public void FixBuildingNode(ushort node)
        {
            foreach(var reference in BuildingOwnedNodes)
            {
                WrappedNode wnode = (WrappedNode)reference.Target;
                if(wnode != null)
                {
                    wnode.Check();
                    if (!wnode.IsCreated() && wnode.Position == NetUtil.Node(node).m_position && wnode.Position != default(Vector3) && wnode.NetInfo == NetUtil.Node(node).Info)
                    {
                        wnode.ForceSetId(node);
                        break;
                    }
                }  
            }
        }

        /*public void CollectGarbage(int queueLength)
        {
            if ((RegisteredNodes.Count + RegisteredSegments.Count + RegisteredBuildings.Count + RegisteredProps.Count + RegisteredTrees.Count)
                //< UndoMod.UndoMod.Instsance.Queue.Length() * 30)
                < 50)
            {
                return;
            }

            RegisteredNodes = RegisteredNodes.Where(kvp => kvp.Value.IsAlive).ToDictionary(kvp => kvp.Key, kvp => kvp.Value );
            RegisteredSegments = RegisteredSegments.Where(kvp => kvp.Value.IsAlive).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            RegisteredProps = RegisteredProps.Where(kvp => kvp.Value.IsAlive).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            RegisteredBuildings = RegisteredBuildings.Where(kvp => kvp.Value.IsAlive).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            RegisteredTrees = RegisteredTrees.Where(kvp => kvp.Value.IsAlive).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            BuildingOwnedNodes.RemoveWhere(i => !i.IsAlive);
        }*/
    }
}
