using UndoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SharedEnvironment
{
    public class WrappedTree : GameAction
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

        private bool _single;
        public bool Single
        {
            get => IsCreated() ? ManagerUtils.Tree(_id).Single : _single;
            set => _single = IsCreated() ? throw new WrapperException("Cannot modify built tree") : value;
        }

        public ref TreeInstance Get
        {
            get => ref ManagerUtils.Tree(Id);
        }

        // methods

        public /*override*/ void Create()
        {
            if (!IsCreated())
            {
                _id = ManagerUtils.CreateTree(_position, _treeInfo, _single);
            }
        }

        public /*override*/ bool Release()
        {
            if (IsCreated())
            {
                _position = ManagerUtils.Tree(_id).Position;
                _single = ManagerUtils.Tree(_id).Single;
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

        //

        protected uint _id;
        public uint Id { get => _id == 0 ? throw new WrapperException("Tree is not created") : _id; }

        protected bool _isBuildAction = true;
        public bool IsBuildAction { get => _isBuildAction; set => _isBuildAction = value; }

        public bool IsCreated()
        {
            return _id != 0;
        }

        public override void Do()
        {
            if (_isBuildAction)
            {
                Create();
            }
            else
            {
                Release();
            }
        }

        public override void Undo()
        {
            if (_isBuildAction)
            {
                Release();
            }
            else
            {
                Create();
            }
        }

        public override void Redo()
        {
            Do();
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
        }
    }
}