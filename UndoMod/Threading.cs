using ColossalFramework;
using ICities;
using SharedEnvironment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UndoMod
{
    public class Threading : ThreadingExtensionBase
    {
        private static bool _processed = false;

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.Z))
            {
                if(!_processed)
                {
                    UndoMod.Instsance.UndoLast();
                    _processed = true;
                }
            } else
            {
                _processed = false;
            }


        }
    }
}
