using ColossalFramework.UI;
using Harmony;
using ICities;
using Redirection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UndoMod.Patches;

namespace UndoMod
{
    public class LoadingExtension : ILoadingExtension
    {
        private static LoadingExtension _instance;
        public static LoadingExtension Instsance { get => _instance; }

        public bool m_detoured = false;
        public bool m_inStandardGame = false;

        public static readonly string HarmonyID = "strad.undomod";

        private HarmonyInstance _harmony;

        public LoadingExtension()
        {
            _instance = this;
        }

        public void OnCreated(ILoading loading)
        {
            m_inStandardGame = loading.currentMode == AppMode.Game;
        }

        public void OnLevelLoaded(LoadMode mode)
        {
            try
            {
                if (_harmony == null)
                {
                    _harmony = HarmonyInstance.Create(HarmonyID);
                }

                /*NetToolPatch.Patch(_harmony);
                NetManagerPatch.Patch(_harmony);*/

                BulldozeToolPatch.Patch();
                TreeToolPatch.Patch();
                PropToolPatch.Patch();
                NetToolPatch.Patch();
                BuildingToolPatch.Patch();
                TreeManagerPatch.Patch();
                PropManagerPatch.Patch();
                NetManagerPatch.Patch(_harmony);
                BuildingManagerPatch.Patch(_harmony);

                m_detoured = true;
            }
            catch (Exception e)
            {
                ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                panel.SetMessage("Undo It!", "FATAL ERROR: Mod failed to detour some of the game methods. The game code is now in invalid state. Restart the game" +
                    " and if the problem persists, disable this mod, as it is probably in conflicit with some of your other mods.\n" +
                    e.StackTrace, true);
            }
        }

        public void OnLevelUnloading()
        {
            m_detoured = false;

            BuildingManagerPatch.Unpatch(_harmony);
            NetManagerPatch.Unpatch(_harmony);
            PropManagerPatch.Unpatch();
            BuildingToolPatch.Unpatch();
            TreeManagerPatch.Unpatch();
            NetToolPatch.Unpatch();
            PropToolPatch.Unpatch();
            TreeToolPatch.Unpatch();
            BulldozeToolPatch.Unpatch();
            /*NetManagerPatch.Unpatch(_harmony);
            NetToolPatch.Unpatch(_harmony);*/
            UndoMod.Instsance.InvalidateAll(false);
        }

        public void OnReleased()
        {
            
        }
    }
}
