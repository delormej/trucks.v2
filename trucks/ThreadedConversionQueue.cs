using System.Threading;
using System.Collections.Generic;

namespace Trucks
{
    internal class CallbackState
    {
        internal ConvertState State { get; set; }
        internal Func<ConvertState, Task> OnConverted { get; set; }
    }

    public class ThreadedConversionQueue : IConversionQueue    
    {
        List<Timer> _timers;


        public ThreadedConversionQueue()
        {
            _timers = new List<Timer>();
        }

        public void Add(ConvertState state, Func<ConvertState, Task> onConverted)
        {
            int timeoutMs = 1000 * 60 * 5; // 5 minutes
            
            var callbackState = new CallbackState() 
            { 
                State = state, OnConverted = onConverted 
            };
            
            _timers.Add(new Timer(TimerCallback, callbackState, timeoutMs, -1));
        }

        private void TimerCallback(object state)
        {
            CallbackState callbackState = (CallbackState)state;
            ConvertState convertState = callbackState.State;
            
            callbackState.OnConverted(convertState);
        }
    }
}
