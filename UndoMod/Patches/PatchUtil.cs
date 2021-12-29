using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UndoMod.Patches
{
    class PatchUtil
    {
        internal static bool CheckIfObserving()
        {
            return !UndoMod.Instsance.PerformingAction && !UndoMod.Instsance.Invalidated && UndoMod.Instsance.ObservingOnlyBuildings == 0;
        }
    }
}
