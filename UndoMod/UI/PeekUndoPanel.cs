using SharedEnvironment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UndoMod.UI
{
    public class PeekUndoPanel : MonoBehaviour
    {
        private static readonly float label_height = 28;
        private static readonly int max_length = 25;

        private bool _enabled;

        private static PeekUndoPanel _instance;
        public static PeekUndoPanel Instance
        {
            get
            {
                if(!_instance)
                {
                    _instance = FindObjectOfType<PeekUndoPanel>();
                }
                if (!_instance)
                {
                    _instance = new GameObject("Undo/Redo Panel").AddComponent<PeekUndoPanel>();
                }
                _instance._enabled = _instance.enabled;
                return _instance;
            }
        }

        public void Enable()
        {
            enabled = true;
            _enabled = enabled;
        }

        public void Disable()
        {
            if(_enabled)
            {
                enabled = false;
                _enabled = enabled;
            }
        }

        void OnGUI()
        {
            float Width = 400;
            float Height = Mathf.Clamp(UndoMod.Instsance.Queue.CurrentCount() * label_height + 20, 300, Mathf.Min( Screen.height - 100, label_height*max_length+20 ));

            Rect windowRect = new Rect((Screen.width - Width) / 2, (Screen.height - Height) / 2, Width, Height);
            GUI.Window(665, windowRect, _populateWindow, "Undo/Redo Queue");
        }

        private void _populateWindow(int num)
        {
            GUILayout.BeginVertical();

            int offset = UndoMod.Instsance.Queue.CurrentCount() - max_length;
            using (var sequenceEnum = UndoMod.Instsance.Queue.GetEnumerator())
            {
                while (sequenceEnum.MoveNext())
                {
                    if(offset > 0)
                    {
                        offset--;
                        continue;
                    }
                    var info = sequenceEnum.Current;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(info.text, GUILayout.Width(50));
                    GUILayout.Label(info.item.Name, GUILayout.Width(100));
                    GUILayout.Label(info.item.ModName, GUILayout.Width(100));
                    GUILayout.Label(info.item.InfoString, GUILayout.Width(100));
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
        }
    }
}
