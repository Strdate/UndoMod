using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedEnvironment
{
    public abstract class AbstractWrapper : GameAction
    {
        protected ushort _id;
        public ushort Id { get => _id == 0 ? throw new WrapperException("Item is not created") : _id; }

        public void ForceSetId(ushort id)
        {
            _id = id;
        }

        protected bool _isBuildAction = true;
        public bool IsBuildAction { get => _isBuildAction; set => _isBuildAction = value; }

        public bool IsCreated()
        {
            return _id != 0;
        }

        public abstract void Create();

        public abstract bool Release();

        public override void Do()
        {
            if(_isBuildAction)
            {
                Create();
            }
            else
            {
                Release();
            }
        }

        public override void Undo()
        {
            if (_isBuildAction)
            {
                Release();
            }
            else
            {
                Create();
            }
        }

        public override void Redo()
        {
            Do();
        }
    }

    public class WrapperException : Exception
    {
        public WrapperException(string message) : base(message)
        {

        }
    }
}
