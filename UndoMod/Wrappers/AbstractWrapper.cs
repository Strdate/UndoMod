using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedEnvironment
{
    public abstract class AbstractWrapper : IConstructable
    {
        protected ushort _id;
        public ushort Id { get => _id == 0 ? throw new WrapperException("Item is not created") : _id; }

        public void ForceSetId(ushort id)
        {
            _id = id;
        }

        public bool IsCreated()
        {
            return _id != 0;
        }

        public abstract bool Create();

        public abstract bool Release();

        public virtual int ConstructionCost()
        {
            return 0;
        }
    }

    public class WrapperException : Exception
    {
        public WrapperException(string message) : base(message)
        {

        }
    }
}
