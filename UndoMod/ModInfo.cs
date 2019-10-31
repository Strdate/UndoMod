using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UndoMod.UI;
using UnityEngine;

namespace UndoMod
{
    public class ModInfo : IUserMod
    {
        public static readonly string VERSION = "BETA 0.2.0";
        public const string settingsFileName = "UndoMod";

        public string Name => "Undo It!";

        public string Description => "CTRL+Z to Undo, CTRL+Y to Redo [" + VERSION + "]";

        public static readonly SavedInputKey sc_undo = new SavedInputKey("sc_undo", settingsFileName, SavedInputKey.Encode(KeyCode.Z, true, false, false), true);
        public static readonly SavedInputKey sc_redo = new SavedInputKey("sc_redo", settingsFileName, SavedInputKey.Encode(KeyCode.Y, true, false, false), true);
        public static readonly SavedInputKey sc_peek = new SavedInputKey("sc_peek", settingsFileName, SavedInputKey.Encode(KeyCode.Y, false, false, true), true);

        public static readonly SavedBool sa_disableShortcuts = new SavedBool("sa_disableShortcuts", settingsFileName, true, true);
        public static readonly SavedBool sa_ignoreCosts = new SavedBool("sa_ignoreCosts", settingsFileName, false, true);
        public static readonly SavedInt sa_queueCapacity = new SavedInt("sa_queueCapacity", settingsFileName, 10, true);

        public ModInfo()
        {
            try
            {
                // Creating setting file - from SamsamTS
                if (GameSettings.FindSettingsFileByName(settingsFileName) == null)
                {
                    GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = settingsFileName } });
                }
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't load/create the setting file.");
                Debug.LogException(e);
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            try
            {
                UIHelper group = helper.AddGroup(Name) as UIHelper;
                UIPanel panel = group.self as UIPanel;

                panel.gameObject.AddComponent<OptionsKeymapping>();

                group.AddSpace(10);

                UICheckBox checkBox = (UICheckBox)group.AddCheckbox("Disable Undo/Redo shortcuts when using unsupported tools", sa_disableShortcuts.value, (b) =>
                {
                    sa_disableShortcuts.value = b;
                });
                checkBox.tooltip = "Shortcuts are inactive when using unsupported tools (eg. zoning tool) or tools with their own undo implementation (eg. Move It!)\n" +
                    "Warning: Although the shortcuts are disabled, all actions are still saved in undo queue!";

                checkBox = (UICheckBox)group.AddCheckbox("Ignore costs", sa_ignoreCosts.value, (b) =>
                {
                    sa_ignoreCosts.value = b;
                });
                checkBox.tooltip = "Undo/Redo won't change account balance";

                group.AddSpace(10);

                UITextField tf = null;
                tf = (UITextField)group.AddTextfield("Undo queue capacity (hit enter to submit): ", sa_queueCapacity.value.ToString(), (text) => { }, (text) => {
                    int val;
                    if (int.TryParse(text, out val) && val > 0)
                    {
                        UndoMod.Instsance.ChangeQueueCapacity(val);
                        sa_queueCapacity.value = val;
                    }
                    else
                    {
                        tf.text = "10";
                    }
                });
                tf.tooltip = "Warning: Current queue will be discarded\nDefault value is 10";
            }
            catch (Exception e)
            {
                Debug.Log("OnSettingsUI failed");
                Debug.LogException(e);
            }
        }
    }
}
