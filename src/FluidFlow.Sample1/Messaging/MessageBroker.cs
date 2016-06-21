using System;
using System.Collections.Concurrent;
using System.Linq;

namespace FluidFlow.Sample1.Messaging
{
    public class MessageBroker
    {
        private static readonly ConcurrentDictionary<Type, Action<BrokerEvent>> Handlers = new ConcurrentDictionary<Type, Action<BrokerEvent>>();

        /// <summary>
        /// Subscribes the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="handler">The handler.</param>
        public static void Subscribe(Type args, Action<BrokerEvent> handler)
        {
            if (!Handlers.Any(h => h.Key == args && h.Value.Equals(handler)))
                Handlers.TryAdd(args, handler);
        }

        /// <summary>
        /// Unsubscribes the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="handler">The handler.</param>
        public static void Unsubscribe(Type args, Action<BrokerEvent> handler)
        {
            Action<BrokerEvent> removed;
            Handlers.TryRemove(args, out removed);
        }

        /// <summary>
        /// Broadcasts the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Broadcast(BrokerEvent args)
        {
            var handlers = Handlers.Where(h => h.Key == args.GetType());
            foreach (var handler in handlers)
            {
                handler.Value(args);
            }
        }

        /// <summary>
        /// Clears the subscribers.
        /// </summary>
        public void ClearSubscribers()
        {
            Handlers.Clear();
        }
    }
}
