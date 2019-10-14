using UndoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SharedEnvironment
{
    public class WrappedNode : AbstractWrapper
    {
        private Vector3 _position;
        public Vector3 Position
        {
            get => IsCreated() ? NetUtil.Node(_id).m_position : _position;
            set => _position = IsCreated() ? throw new WrapperException("Cannot modify built node") : value;
        }

        private ushort _netInfoIndex;
        public NetInfo NetInfo
        {
            get => IsCreated() ? NetUtil.Node(_id).Info : NetUtil.NetinfoFromIndex(_netInfoIndex);
            set => _netInfoIndex = IsCreated() ? throw new WrapperException("Cannot modify built node") : NetUtil.NetinfoToIndex( value );
        }

        public ref NetNode Get
        {
            get => ref NetUtil.Node(Id);
        }

        // methods

        public override bool Create()
        {
            if(!IsCreated())
            {
                _id = NetUtil.CreateNode( NetUtil.NetinfoFromIndex(_netInfoIndex) , _position);
            }

            return true;
        }

        public override bool Release()
        {
            if(IsCreated())
            {
                UpdateData();

                NetUtil.ReleaseNode(_id);
                if(!NetUtil.ExistsNode(_id))
                {
                    _id = 0;
                    return true;
                }
                return false;
            }
            return true; // ?? true or false
        }

        public override string ToString()
        {
            return "[WNODE created:" + IsCreated() + ", id:" + _id + ", pos:" + Position + ", info:" + NetInfo + (IsCreated() ? ", flags: " + Get.m_flags : "") + "]";
        }

        protected override void UpdateData()
        {
            _position = NetUtil.Node(_id).m_position;
            _netInfoIndex = NetUtil.Node(_id).m_infoIndex;
        }

        // Constructors

        public WrappedNode() { }

        public WrappedNode(ushort id)
        {
            if( id != 0 && !NetUtil.ExistsNode(id) )
            {
                throw new WrapperException("Cannot wrap nonexisting node");
            }
            _id = id;

            UpdateData();
        }
    }
}
