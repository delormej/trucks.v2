using System.Threading;
using System.Collections.Generic;

namespace Trucks
{
    public class ThreadedConversionQueue : IConversionQueue    
    {
        const int DelayMins = 1;
        Queue<ConvertState> _queue;
        Timer _timer;
        int _activeCount = 0;
        Func<ConvertState, Task> _onConverted;

        public ThreadedConversionQueue(Func<ConvertState, Task> onConverted)
        {
            _queue = new Queue<ConvertState>();
            _onConverted = onConverted;
        }

        public event EventHandler OnFinished;

        public void Add(ConvertState state)
        {
            _queue.Enqueue(state);
            
            if (_timer == null)
            {
                _timer = new Timer(Dequeue, null, 
                    TimeSpan.FromMinutes(DelayMins),
                    TimeSpan.FromSeconds(10));
            }
        }

        private void Dequeue(object _)
        {
            ConvertState convertState;
            
            if (!_queue.TryPeek(out convertState))
            {
                Console.WriteLine("Nothing left in the queue.");
                Finish();
                return;
            }

            try 
            {
                Interlocked.Increment(ref _activeCount);

                DateTime readyTime = convertState.UploadTimestampUtc
                    .AddMinutes(DelayMins);

                // Don't dequeue if not ready.
                if (DateTime.UtcNow < readyTime || 
                    !_queue.TryDequeue(out convertState)) 
                {
                    return;
                }

                var task = _onConverted(convertState);
                task.Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error converting: {e.Message}");
            }
            finally
            {
                Interlocked.Decrement(ref _activeCount);
            }
        }

        private bool IsActive() 
        {
            return _queue.Count > 0 || _activeCount > 0;
        }

        private void Finish()
        {
            if (!IsActive())
            {
                Console.WriteLine("No active jobs.");
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer = null;
            }

             if (OnFinished != null)
             {
                Console.WriteLine("Signaling finished.");
                OnFinished(this, null);                 
             }
        }
    }
}
