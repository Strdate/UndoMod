using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedEnvironment
{
    public class ActionQueue : IEnumerable<QueueItemInfo>
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

        public int CurrentCount()
        {
            return _head + 1;
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

        /*public List<QueueItemInfo> ToList()
        {
        // not finished
            return _queue.Select(x =>
            {
                QueueItemInfo info = new QueueItemInfo();
                return info;
            }).ToList();
        }*/

        public IEnumerator<QueueItemInfo> GetEnumerator()
        {
            for(int i = 0; i <= _head; i++)
            {
                var qinfo = new QueueItemInfo();
                qinfo.item = _queue[i];
                if(i == _pointer)
                {
                    qinfo.text = "UNDO";
                } else if(i == _pointer + 1)
                {
                    qinfo.text = "REDO";
                }
                yield return qinfo;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class QueueItemInfo
    {
        public IActionQueueItem item;
        public string text = "";
    }
}
