using UndoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SharedEnvironment
{
    public class WrappedBuilding : AbstractWrapper
    {
        public static WrappersDictionary dictionary;

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

        private WrappedBuilding _subBuilding;
        public WrappedBuilding SubBuilding
        {
            get => _subBuilding;
            set => _subBuilding = value;
        }

        private WrappedBuilding _parentBuilding;
        public WrappedBuilding ParentBuilding
        {
            get => _parentBuilding;
            set => _parentBuilding = value;
        }

        public ref Building Get
        {
            get => ref ManagerUtils.BuildingS(Id);
        }

        // methods


        protected void CheckConnectedBuildings(bool propagateDown = false, bool propagateUp = false)
        {
            if(IsCreated())
            {
                if (_subBuilding == null && ManagerUtils.ExistsBuilding(Get.m_subBuilding))
                {
                    _subBuilding = dictionary.RegisterBuilding(Get.m_subBuilding, false);
                }
                if (_parentBuilding == null && ManagerUtils.ExistsBuilding(Get.m_parentBuilding))
                {
                    _parentBuilding = dictionary.RegisterBuilding(Get.m_parentBuilding, false);
                }

                if (_subBuilding != null && _subBuilding.IsCreated() && Get.m_subBuilding == 0)
                {
                    Get.m_subBuilding = _subBuilding.Id;
                }
                if (_parentBuilding != null && _parentBuilding.IsCreated() && Get.m_parentBuilding == 0)
                {
                    Get.m_parentBuilding = _parentBuilding.Id;
                }

                if(propagateDown)
                {
                    _subBuilding?.CheckConnectedBuildings(propagateDown: true);
                }
                
                if(propagateUp)
                {
                    _parentBuilding?.CheckConnectedBuildings(propagateUp: true);
                }
            }
        }

        public override bool Create()
        {
            if (!IsCreated())
            {
                _id = ManagerUtils.CreateBuilding(_position, _angle, _buildingInfo);
                Get.m_flags = _flags;
                CheckConnectedBuildings(true, true);
            }

            return true;
        }

        public override bool Release()
        {
            if (IsCreated())
            {
                UpdateData();

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

        protected override void UpdateData()
        {
            _position = ManagerUtils.BuildingS(_id).m_position;
            _angle = ManagerUtils.BuildingS(_id).m_angle;
            _flags = ManagerUtils.BuildingS(_id).m_flags;
            _buildingInfo = ManagerUtils.BuildingS(_id).Info;

            CheckConnectedBuildings(true, true);
        }

        // Constructors

        public WrappedBuilding() { }

        public WrappedBuilding(ushort id, bool checkConnectedBuildings)
        {
            if (id != 0 && !ManagerUtils.ExistsBuilding(id))
            {
                throw new WrapperException("Cannot wrap nonexisting building");
            }
            _id = id;

            _position = ManagerUtils.BuildingS(_id).m_position;
            _angle = ManagerUtils.BuildingS(_id).m_angle;
            _flags = ManagerUtils.BuildingS(_id).m_flags;
            _buildingInfo = ManagerUtils.BuildingS(_id).Info;

            if(checkConnectedBuildings)
            {
                CheckConnectedBuildings(true, true);
            }
        }
    }
}