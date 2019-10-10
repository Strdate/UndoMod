﻿using UndoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SharedEnvironment
{
    public class WrappedBuilding : AbstractWrapper
    {
        private Vector3 _position;
        public Vector3 Position
        {
            get => IsCreated() ? ManagerUtils.BuildingS(_id).m_position : _position;
            set => _position = IsCreated() ? throw new WrapperException("Cannot modify built building") : value;
        }

        private BuildingInfo _buildingInfo;
        public BuildingInfo Info
        {
            get => IsCreated() ? ManagerUtils.BuildingS(_id).Info : _buildingInfo;
            set => _buildingInfo = IsCreated() ? throw new WrapperException("Cannot modify built building") : value;
        }

        private float _angle;
        public float Angle
        {
            get => IsCreated() ? ManagerUtils.BuildingS(_id).m_angle : _angle;
            set => _angle = IsCreated() ? throw new WrapperException("Cannot modify built building") : value;
        }

        private Building.Flags _flags;
        public Building.Flags Flags
        {
            get => IsCreated() ? ManagerUtils.BuildingS(_id).m_flags : _flags;
            set => _flags = IsCreated() ? throw new WrapperException("Cannot modify built building") : value;
        }

        public ref Building Get
        {
            get => ref ManagerUtils.BuildingS(Id);
        }

        // methods

        public override void Create()
        {
            if (!IsCreated())
            {
                _id = ManagerUtils.CreateBuilding(_position, _angle, _buildingInfo);
                Get.m_flags = _flags;
            }
        }

        public override bool Release()
        {
            if (IsCreated())
            {
                _position = ManagerUtils.BuildingS(_id).m_position;
                _angle = ManagerUtils.BuildingS(_id).m_angle;
                _flags = ManagerUtils.BuildingS(_id).m_flags;
                _buildingInfo = ManagerUtils.BuildingS(_id).Info;

                ManagerUtils.ReleaseBuilding(_id);
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

        public WrappedBuilding() { }

        public WrappedBuilding(ushort id)
        {
            if (id != 0 && ((ManagerUtils.BuildingS(id).m_flags & Building.Flags.Deleted) == Building.Flags.None))
            {
                throw new WrapperException("Cannot wrap nonexisting building");
            }
            _id = id;
        }
    }
}