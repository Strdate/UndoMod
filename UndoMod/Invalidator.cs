using System.Collections.Generic;

namespace UndoMod
{
    public class Invalidator
    {
        private static Invalidator _instance;
        public static Invalidator Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Invalidator();
                return _instance;
            }
        }

        public Invalidator()
        {
            Clear();
        }

        public void Clear()
        {
            InvalidSegments = new HashSet<ushort>();
            InvalidNodes = new HashSet<ushort>();
            InvalidBuildings = new HashSet<ushort>();
            InvalidProps = new HashSet<ushort>();
            InvalidTrees = new HashSet<uint>();
        }

        public HashSet<ushort> InvalidSegments;
        public HashSet<ushort> InvalidNodes;
        public HashSet<ushort> InvalidBuildings;
        public HashSet<ushort> InvalidProps;
        public HashSet<uint> InvalidTrees;
    }
}
