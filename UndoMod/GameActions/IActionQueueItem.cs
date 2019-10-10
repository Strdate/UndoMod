using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedEnvironment
{
    public interface IActionQueueItem
    {
        string Name { get; }

        void Do();
        void Undo();
        void Redo();
    }
}
