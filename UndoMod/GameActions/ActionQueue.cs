using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedEnvironment
{
    public class ActionQueue
    {
        private IActionQueueItem[] _queue;
        private int _pointer = -1;
        private int _head = -1;

        public int Capacity { get => _queue.Length; }

        public ActionQueue(int length)
        {
            _queue = new IActionQueueItem[length];
        }

        public int Length()
        {
            return _queue.Length;
        }

        public void Push(IActionQueueItem item)
        {
            _pointer++;
            if(_pointer == Capacity)
            {
                ShiftLeft();
                _pointer--;
            }

            _head = _pointer;
            _queue[_pointer] = item;
        }

        public void Clear()
        {
            _pointer = -1;
            _head = -1;
        }

        public IActionQueueItem Previous()
        {
            if (_pointer < 0)
                return null;

            var item = _queue[_pointer];
            _pointer--;

            return item;
        }

        public IActionQueueItem Next()
        {
            if (_pointer == _head)
                return null;

            _pointer++;
            return _queue[_pointer];
        }

        public List<IActionQueueItem> AssembleAll()
        {
            var items = new List<IActionQueueItem>();

            for(int i = 0; i <= _head; i++)
            {
                items.Add(_queue[i]);
            }

            return items;
        }

        private void ShiftLeft()
        {
            for(int i = 0; i < (Capacity - 1); i++)
            {
                _queue[i] = _queue[i + 1];
            }
        }

    }
}
