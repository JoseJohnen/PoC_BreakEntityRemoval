using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Interfaz.Utilities
{
    public class BlockingQueue<T> where T : class
    {
        private bool closing;
        private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

        public int Count
        {
            get
            {
                lock (queue)
                {
                    return queue.Count;
                }
            }
        }

        public BlockingQueue()
        {
            lock (queue)
            {
                closing = false;
                Monitor.PulseAll(queue);
            }
        }

        public bool Enqueue(T item)
        {
            lock (queue)
            {
                if (closing || null == item)
                {
                    return false;
                }

                queue.Enqueue(item);

                if (queue.Count == 1)
                {
                    // wake up any blocked dequeue
                    Monitor.PulseAll(queue);
                }

                return true;
            }
        }

        public void Close()
        {
            lock (queue)
            {
                if (!closing)
                {
                    closing = true;
                    queue.Clear();
                    Monitor.PulseAll(queue);
                }
            }
        }

        public bool TryDequeue(out T value, int timeout = Timeout.Infinite)
        {
            lock (queue)
            {
                while (queue.Count == 0)
                {
                    if (closing || timeout < Timeout.Infinite || !Monitor.Wait(queue, timeout))
                    {
                        value = default;
                        return false;
                    }
                }

                //value = queue.Dequeue();
                //return true;
                bool result = queue.TryDequeue(out value);
                Console.WriteLine("\nDequeuing: " + value);
                return result;
            }
        }

        public void Clear()
        {
            lock (queue)
            {
                queue.Clear();
                Monitor.Pulse(queue);
            }
        }
    }
}