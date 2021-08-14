using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Signature
{
    public class Pool
    {
        private AutoResetEvent _autoResetEvent = new (false);
        private ConcurrentQueue<Action> _queue = new();
        private Thread[] _threads;

        public Pool(int size)
        {
            _threads = new Thread[size];

            _threads = Enumerable
                .Range(0, size)
                .Select(x => { var thread = new Thread(Work); thread.Start();  return thread;})
                .ToArray();
             
        }

        public void QueueUserWorkItem(Action action)
        {
            _queue.Enqueue(action);
            _autoResetEvent.Set();
        }

        private void Work(object obj)
        {
            while (_autoResetEvent.WaitOne())
            {
                while (_queue.TryDequeue(out var action))
                {
                    action?.Invoke();
                }
            }
        }
    }
}
