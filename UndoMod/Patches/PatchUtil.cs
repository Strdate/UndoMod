using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UndoMod.Patches
{
    class PatchUtil
    {
        internal static bool CheckIfObserving()
        {
            return !UndoMod.Instsance.PerformingAction && !UndoMod.Instsance.Invalidated && UndoMod.Instsance.ObservingOnlyBuildings == 0;
        }

        internal static MethodInfo Method(Type type, string name)
        {
            return type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        }
    }
}
