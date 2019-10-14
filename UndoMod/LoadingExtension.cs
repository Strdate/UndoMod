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

        public static readonly string HarmonyID = "strad.undomod";

        private HarmonyInstance _harmony;

        public LoadingExtension()
        {
            _instance = this;
        }

        public void OnCreated(ILoading loading)
        {
            
        }

        public void OnLevelLoaded(LoadMode mode)
        {
            if(_harmony == null)
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
            NetManagerPatch.Patch();
            BuildingManagerPatch.Patch(_harmony);

            m_detoured = true;
        }

        public void OnLevelUnloading()
        {
            m_detoured = false;

            BuildingManagerPatch.Unpatch(_harmony);
            NetManagerPatch.Unpatch();
            PropManagerPatch.Unpatch();
            BuildingToolPatch.Unpatch();
            TreeManagerPatch.Unpatch();
            NetToolPatch.Unpatch();
            PropToolPatch.Unpatch();
            TreeToolPatch.Unpatch();
            BulldozeToolPatch.Unpatch();
            /*NetManagerPatch.Unpatch(_harmony);
            NetToolPatch.Unpatch(_harmony);*/

        }

        public void OnReleased()
        {
            
        }
    }
}
