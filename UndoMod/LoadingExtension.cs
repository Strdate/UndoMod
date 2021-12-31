using CitiesHarmony.API;
using ColossalFramework.UI;
using ICities;
using SharedEnvironment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UndoMod.Patches;
using UnityEngine;

namespace UndoMod
{
    public class LoadingExtension : ILoadingExtension
    {
        private static LoadingExtension _instance;
        public static LoadingExtension Instsance { get => _instance; }

        public bool m_detoured = false;
        public bool m_inStandardGame = false;

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
            if(!HarmonyHelper.IsHarmonyInstalled) {
                ThrowModal("The Undo It mod is missing a dependency! Please subscribe to 'Harmony' mod in steam workshop.");
                return;
            }

            try
            {
                Patcher.PatchAll(EMLCompatibility());

                if(WrappedSegment.NS == null)
                {
                    try { WrappedSegment.NS = new NS_Manager(); } catch(Exception e) { Debug.LogError(e); }
                }

                m_detoured = true;
            }
            catch (Exception e)
            {
                ThrowModal("FATAL ERROR: Mod failed to detour some of the game methods. The game code is now in invalid state. Restart the game" +
                    " and if the problem persists, disable this mod, as it is probably in conflicit with some of your other mods.\n" +
                    e.StackTrace);
            }
        }

        private static void ThrowModal(string msg)
        {
            ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
            panel.SetMessage("Undo It!", msg, true);
        }

        private static bool EMLCompatibility()
        {
            return Type.GetType("EManagersLib.EModule") != null;
        }

        public void OnLevelUnloading()
        {
            if(HarmonyHelper.IsHarmonyInstalled) {
                Patcher.UnpatchAll();
            }
            m_detoured = false;
            UndoMod.Instsance.InvalidateAll(false);
        }

        public void OnReleased()
        {
            
        }
    }
}
