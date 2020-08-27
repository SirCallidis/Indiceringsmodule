using Indiceringsmodule.Common.EventModels;
using Indiceringsmodule.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indiceringsmodule.Common
{
    //TODO - remove link
    //https://www.youtube.com/watch?v=CGeD0LySt_Q

    public class EventAggregator : IEventAggregator
    {
        private readonly object Locker = new object();
        private readonly List<(Type eventType, Delegate methodToCall)> EventRegistrations =
            new List<(Type eventType, Delegate methodToCall)>();

        public Subscription Subscribe<T>(Action<T> action)
        {
            if (action != null)
            {
                lock (Locker)
                {
                    EventRegistrations.Add((typeof(T), action));
                    return new Subscription(() =>
                    {
                        EventRegistrations.Remove((typeof(T), action));
                    });
                }
            }
            return new Subscription(() => { });
        }

        public void Publish<T>(T eventData)
        {
            List<(Type eventType, Delegate methodToCall)> regs = null;
            lock (Locker)
            {
                regs = new List<(Type eventType, Delegate methodToCall)>(EventRegistrations);
            }
            foreach (var (eventType, methodToCall) in regs)
            {
                if (eventType == typeof(T))
                    ((Action<T>)methodToCall)(eventData);
            }
        }
    }
}
