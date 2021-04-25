using System;
using System.Collections.Generic;
using System.Threading;

namespace ThreadPool
{
    public class SimpleLockThreadPool : IThreadPool
    {
        private readonly Thread[] _threads;
        private readonly Queue<Action> _tasks = new();
        private bool _isWorking = true;
        
        public SimpleLockThreadPool(int concurrency)
        {
            _threads = new Thread[concurrency];
            for (int i = 0; i < concurrency; i++)
            {
                _threads[i] = new Thread(ThreadBody) {IsBackground = true};
                _threads[i].Start();
            }
        }

        private void ThreadBody()
        {
            Action currentTask;
            while (true)
            {
                lock (_tasks)
                {
                    while (_tasks.Count == 0)
                    {
                        if (!_isWorking)
                            return;
                        Monitor.Wait(_tasks);
                    }

                    currentTask = _tasks.Dequeue();
                }
                currentTask();   
            }
        }

        public void EnqueueAction(Action action)
        {
            if (!_isWorking)
                throw new ObjectDisposedException("Object was disposed");
            
            lock (_tasks)
            {
                _tasks.Enqueue(action);
                Monitor.Pulse(_tasks);
            }
        }

        public void Dispose()
        {
            lock (_tasks)
            {
                _isWorking = false;
                Monitor.PulseAll(_tasks);
            }
            
            //todo wait all threads
        }
    }
}