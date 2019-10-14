using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedEnvironment
{
    public interface IActionQueueItem
    {
        string Name { get; }
        string ModName { get; }

        bool Do();
        bool Undo();
        bool Redo();
    }
}
