using UndoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SharedEnvironment
{
    public class WrappedProp : AbstractWrapper
    {
        private Vector3 _position;
        public Vector3 Position
        {
            get => IsCreated() ? ManagerUtils.Prop(_id).Position : _position;
            set => _position = IsCreated() ? throw new WrapperException("Cannot modify built prop") : value;
        }

        private PropInfo _propInfo;
        public PropInfo Info
        {
            get => IsCreated() ? ManagerUtils.Prop(_id).Info : _propInfo;
            set => _propInfo = IsCreated() ? throw new WrapperException("Cannot modify built prop") : value;
        }

        private float _angle;
        public float Angle
        {
            get => IsCreated() ? ManagerUtils.Prop(_id).m_angle : _angle;
            set => _angle = IsCreated() ? throw new WrapperException("Cannot modify built prop") : value;
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

        public ref PropInstance Get
        {
            get => ref ManagerUtils.Prop(Id);
        }

        // methods

        public override bool Create()
        {
            if (!IsCreated())
            {
                _id = ManagerUtils.CreateProp(_position, _angle, _propInfo, Single);
                Get.m_flags = _flags;
            }

            return true;
        }

        public override bool Release()
        {
            if (IsCreated())
            {
                _position = ManagerUtils.Prop(_id).Position;
                _angle = ManagerUtils.Prop(_id).m_angle;
                _flags = ManagerUtils.Prop(_id).m_flags;
                _propInfo = ManagerUtils.Prop(_id).Info;

                ManagerUtils.ReleaseProp(_id);
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

        // Constructors

        public WrappedProp() { }

        public WrappedProp(ushort id)
        {
            if (id != 0 && (ManagerUtils.Prop(id).m_flags == 0))
            {
                throw new WrapperException("Cannot wrap nonexisting prop");
            }
            _id = id;

            _position = ManagerUtils.Prop(_id).Position;
            _angle = ManagerUtils.Prop(_id).m_angle;
            _flags = ManagerUtils.Prop(_id).m_flags;
            _propInfo = ManagerUtils.Prop(_id).Info;
        }
    }
}