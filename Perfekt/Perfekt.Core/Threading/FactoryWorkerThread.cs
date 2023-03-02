using System.Collections.Concurrent;

namespace Perfekt.Core.Threading
{
    public interface IFactoryWorker<T>
    {
        T Tag { get; }

        DateTime QueuedTime { get; }
    }

    public abstract class FactoryWorkerBase : IFactoryWorker<object>
    {
        public object Tag { get; set; }

        public DateTime QueuedTime { get; }

        public FactoryWorkerBase()
        {
            this.QueuedTime = DateTime.Now;
            this.Tag = new object();
        }
    }


    public abstract class Factory<T>
    {
        public int WorkerCount => this.Workers.Count;

        public IFactoryWorker<T>[] WorkerArray => this.Workers.ToArray();

        public int ProcessWorkerCount { get; set; } = Environment.ProcessorCount;


        protected ConcurrentQueue<IFactoryWorker<T>> Workers { get; set; }


        public Factory()
        {
            this.Workers = new ConcurrentQueue<IFactoryWorker<T>>();
        }

        protected abstract IFactoryWorker<T> CreateWorker(EventArgs eventArgs);

        /// <summary>
        /// Returns the time it took to empty the queue.
        /// </summary>
        /// <param name="callback"></param>
        public async Task<TimeSpan> CompleteAsync(Action<IFactoryWorker<T>> callback)
        {
            var result = TimeSpan.Zero;
            var startTime = DateTime.Now;

            while (this.WorkerCount > 0)
            {
                if (this.Workers.TryDequeue(out var worker))
                {
                    result = await Task.Run(() =>
                    {
                        callback?.Invoke(worker);

                        return DateTime.Now - startTime;
                    });
                }
            }

            return DateTime.Now - startTime;
        }
    }
}
