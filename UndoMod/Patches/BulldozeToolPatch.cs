using ColossalFramework;
using Redirection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace UndoMod.Patches
{
    public class PatchInst
    {
        public string mehtodName;
        public MethodInfo original;
        public MethodInfo patch;
        public RedirectCallsState state;

        public PatchInst(string name)
        {
            mehtodName = name;
            original = typeof(BulldozeTool).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
            patch = typeof(BulldozeToolPatch).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void Patch()
        {
            state = RedirectionHelper.RedirectCalls(original, patch);
        }

        public void Unpatch()
        {
            RedirectionHelper.RevertRedirect(original, state);
        }

        public void Swap()
        {
            Unpatch();
            state = RedirectionHelper.RedirectCalls(patch, original);
        }

        public void Unswap()
        {
            RedirectionHelper.RevertRedirect(patch, state);
            Patch();
        }

        public static implicit operator PatchInst(string name)
        {
            var instance = new PatchInst(name);

            return instance;
        }
    }

    public class BulldozeToolPatch
    {
        public static PatchInst[] patches = { "DeleteSegmentImpl", "DeleteNodeImpl", "DeleteBuildingImpl", "DeleteTreeImpl", "DeletePropImpl" };

        public static void Patch()
        {
            foreach(var patch in patches)
            {
                patch.Patch();
            }
        }

        public static void Unpatch()
        {
            foreach (var patch in patches)
            {
                patch.Unpatch();
            }
        }

        private void DeleteSegmentImpl(ushort segment)
        {
            var patch = patches[0];

            UndoMod.Instsance.BeginObserving("Remove segment");
            patch.Swap();
            DeleteSegmentImpl(segment);
            patch.Unswap();
            UndoMod.Instsance.EndObserving();
        }

        private void DeleteNodeImpl(ushort node)
        {
            var patch = patches[1];

            UndoMod.Instsance.BeginObserving("Remove node");
            patch.Swap();
            DeleteNodeImpl(node);
            patch.Unswap();
            UndoMod.Instsance.EndObserving();
        }

        private void DeleteBuildingImpl(ushort building)
        {
            var patch = patches[2];

            UndoMod.Instsance.BeginObserving("Remove building", true);
            patch.Swap();
            DeleteBuildingImpl(building);
            patch.Unswap();
            UndoMod.Instsance.EndObserving();
        }

        private void DeleteTreeImpl(uint tree)
        {
            var patch = patches[3];

            UndoMod.Instsance.BeginObserving("Remove tree");
            patch.Swap();
            DeleteTreeImpl(tree);
            patch.Unswap();
            UndoMod.Instsance.EndObserving();
        }

        private void DeletePropImpl(ushort prop)
        {
            var patch = patches[4];

            UndoMod.Instsance.BeginObserving("Remove prop");
            patch.Swap();
            DeletePropImpl(prop);
            patch.Unswap();
            UndoMod.Instsance.EndObserving();
        }

    }
}
