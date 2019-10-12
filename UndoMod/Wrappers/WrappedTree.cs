using UndoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SharedEnvironment
{
    public class WrappedTree : IConstructable
    {
        private Vector3 _position;
        public Vector3 Position
        {
            get => IsCreated() ? ManagerUtils.Tree(_id).Position : _position;
            set => _position = IsCreated() ? throw new WrapperException("Cannot modify built tree") : value;
        }

        private TreeInfo _treeInfo;
        public TreeInfo Info
        {
            get => IsCreated() ? ManagerUtils.Tree(_id).Info : _treeInfo;
            set => _treeInfo = IsCreated() ? throw new WrapperException("Cannot modify built tree") : value;
        }

        private ushort _flags;
        public ushort Flags
        {
            get => IsCreated() ? ManagerUtils.Tree(_id).m_flags : _flags;
            set => _flags = IsCreated() ? throw new WrapperException("Cannot modify built tree") : value;
        }

        public bool Single
        {
            get
            {
                return (Flags & 16) != 0;
            }
            set
            {
                if (value)
                {
                    Flags |= 16;
                }
                else
                {
                    Flags = (ushort)((int)Flags & -17);
                }
            }
        }

        public ref TreeInstance Get
        {
            get => ref ManagerUtils.Tree(Id);
        }

        // methods

        public /*override*/ bool Create()
        {
            if (!IsCreated())
            {
                _id = ManagerUtils.CreateTree(_position, _treeInfo, Single);
                Get.m_flags = _flags;
            }

            return true;
        }

        public /*override*/ bool Release()
        {
            if (IsCreated())
            {
                _position = ManagerUtils.Tree(_id).Position;
                _flags = ManagerUtils.Tree(_id).m_flags;
                _treeInfo = ManagerUtils.Tree(_id).Info;

                ManagerUtils.ReleaseTree(_id);
                /*if (!NetUtil.ExistssNode(_id))
                {
                    _id = 0;
                    return true;
                }
                return false;*/
                _id = 0;
            }
            return true;
        }

        // TODO!!
        public int ConstructionCost()
        {
            return 0;
        }

        //

        protected uint _id;
        public uint Id { get => _id == 0 ? throw new WrapperException("Tree is not created") : _id; }

        public bool IsCreated()
        {
            return _id != 0;
        }

        public void ForceSetId(uint id)
        {
            _id = id;
        }

        // Constructors

        public WrappedTree() { }

        public WrappedTree(uint id)
        {
            if (id != 0 && (ManagerUtils.Tree(id).m_flags == 0))
            {
                throw new WrapperException("Cannot wrap nonexisting tree");
            }
            _id = id;

            _position = ManagerUtils.Tree(_id).Position;
            _flags = ManagerUtils.Tree(_id).m_flags;
            _treeInfo = ManagerUtils.Tree(_id).Info;
        }
    }
}