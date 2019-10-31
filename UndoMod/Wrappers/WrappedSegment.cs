﻿using ColossalFramework;
using UndoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UndoMod;

namespace SharedEnvironment
{
    public class WrappedSegment : AbstractWrapper
    {
        // Network skins
        internal static NS_Manager NS;

        // Properties

        private WrappedNode _startNode;
        public WrappedNode StartNode
        {
            get => _startNode;
            set => _startNode = IsCreated() ? throw new WrapperException("Cannot modify built segment") : value;
        }

        private WrappedNode _endNode;
        public WrappedNode EndNode
        {
            get => _endNode;
            set => _endNode = IsCreated() ? throw new WrapperException("Cannot modify built segment") : value;
        }

        private Vector3 _startDir;
        public Vector3 StartDirection
        {
            get => IsCreated() ? NetUtil.Segment(_id).m_startDirection : _startDir;
            set => _startDir = IsCreated() ? throw new WrapperException("Cannot modify built segment") : value;
        }

        private Vector3 _endDir;
        public Vector3 EndDirection
        {
            get => IsCreated() ? NetUtil.Segment(_id).m_endDirection : _endDir;
            set => _endDir = IsCreated() ? throw new WrapperException("Cannot modify built segment") : value;
        }

        private bool _invert;
        public bool Invert
        {
            get => IsCreated() ? ((NetUtil.Segment(_id).m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None) : _invert;
            set => _invert = IsCreated() ? throw new WrapperException("Cannot modify built segment") : value;
        }

        // confusion intensifies
        private bool _switchStartAndEnd;
        public bool SwitchStartAndEnd
        {
            get => _switchStartAndEnd;
            set => _switchStartAndEnd = IsCreated() ? throw new WrapperException("Cannot modify built segment") : value;
        }

        private bool _deployPlacementEffects;
        public bool DeployPlacementEffects
        {
            get => _deployPlacementEffects;
            set => _deployPlacementEffects = IsCreated() ? throw new WrapperException("Cannot modify built segment") : value;
        }

        private ushort _netInfoIndex;
        public NetInfo NetInfo
        {
            get => IsCreated() ? NetUtil.Segment(_id).Info : NetUtil.NetinfoFromIndex(_netInfoIndex);
            set => _netInfoIndex = IsCreated() ? throw new WrapperException("Cannot modify built segment") : NetUtil.NetinfoToIndex(value);
        }

        // Network skins integration
        public object NS_Modifiers { get; set; }

        // -- properties end

        public ref NetSegment Get
        {
            get => ref NetUtil.Segment(Id);
        }

        // methods

        public override bool Create()
        {
            if (!IsCreated())
            {
                _id = NetUtil.CreateSegment(_startNode.Id, _endNode.Id, _startDir, _endDir, NetInfo, _invert, _switchStartAndEnd, _deployPlacementEffects);
                try { NS.SetSegmentModifiers(_id, NS_Modifiers, NetInfo); } catch { }
            }

            return true;
        }

        public override bool Release()
        {
            if (IsCreated())
            {
                UpdateData();

                NetUtil.ReleaseSegment(_id);
                if (!NetUtil.ExistsSegment(_id))
                {
                    _id = 0;
                    return true;
                }
                return false;
            }
            return true; // ?? true or false
        }

        // not sure about this.. See BulldozeTool.GetSegmentRefundAmount(...)
        public int ComputeConstructionCost()
        {
            float height1 = Singleton<TerrainManager>.instance.SampleRawHeightSmooth(StartNode.Position);
            float elevation1 = (byte)Mathf.Clamp(Mathf.RoundToInt(StartNode.Position.y - height1), 1, 255);
            float height2 = Singleton<TerrainManager>.instance.SampleRawHeightSmooth(EndNode.Position);
            float elevation2 = (byte)Mathf.Clamp(Mathf.RoundToInt(EndNode.Position.y - height2), 1, 255);
            return NetInfo.m_netAI.GetConstructionCost(StartNode.Position, EndNode.Position, height1, height2);
        }

        public override int ConstructionCost()
        {
            return ComputeConstructionCost();
        }

        public override string ToString()
        {
            return "[WSEGMENT created:" + IsCreated() + ", id:" + _id + ", info:" + NetInfo + " {" + _startNode + ", " + _endNode + "}]";
        }

        protected override void UpdateData()
        {
            _startDir = NetUtil.Segment(_id).m_startDirection;
            _endDir = NetUtil.Segment(_id).m_endDirection;
            _netInfoIndex = NetUtil.Segment(_id).m_infoIndex;
            _invert = (NetUtil.Segment(_id).m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None;
            try { NS_Modifiers = NS.GetSegmentModifiers(_id); } catch { }
        }

        // Constructors

        public WrappedSegment() { }

        public WrappedSegment(WrappedNode startNode, WrappedNode endNode, ushort id)
        {
            if (id != 0 && !NetUtil.ExistsSegment(id))
            {
                throw new WrapperException("Cannot wrap nonexisting segment");
            }

            /*if(NetUtil.Segment(id).m_startNode != startNode.Id || NetUtil.Segment(id).m_endNode != endNode.Id)
            {
                throw new WrapperException("Cannot wrap segment - Nodes do not match");
            }*/
            _id = id;

            UpdateData();

            _startNode = startNode;
            _endNode = endNode;

            _switchStartAndEnd = false;
            _deployPlacementEffects = false;
        }
    }
}