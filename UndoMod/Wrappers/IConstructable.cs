using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedEnvironment
{
    public interface IConstructable
    {
        bool Create();
        bool Release();
        int ConstructionCost();
    }
}
