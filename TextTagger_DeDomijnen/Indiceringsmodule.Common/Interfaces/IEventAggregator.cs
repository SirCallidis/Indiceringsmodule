using System;

namespace Indiceringsmodule.Common.Interfaces
{
    public interface IEventAggregator
    {
        void Publish<T>(T eventData);
        Subscription Subscribe<T>(Action<T> action);
    }
}