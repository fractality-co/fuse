using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fuse
{
    /// <summary>
    /// Centralized publish / subscribe for <see cref="Fuse"/>.
    /// </summary>
    [Document("Events",
        "Relay is a static utility class that manages the centralized publish / subscribe pattern for Fuse. " +
        "You may pass custom event arguments through the system to encapsulate state or results." +
        "\n\nPublish or subscribe events from anywhere, and it is globally broadcast to modules, state and all other listeners.")]
    public static class Relay
    {
        private static readonly Dictionary<string, HashSet<Tuple<MethodBase, object>>> SubscriberMethods =
            new Dictionary<string, HashSet<Tuple<MethodBase, object>>>();

        private static readonly Dictionary<string, HashSet<Action<EventArgs>>> Subscribers =
            new Dictionary<string, HashSet<Action<EventArgs>>>();

        private static readonly HashSet<Action<string, EventArgs>> Global =
            new HashSet<Action<string, EventArgs>>();

        /// <summary>
        /// Publishes an event to subscribers with optional parameters passed.
        /// </summary>
        public static void Publish(string id)
        {
            if (Subscribers.ContainsKey(id))
            {
                foreach (var subscriber in Subscribers[id])
                    subscriber.Invoke(EventArgs.Empty);
            }

            if (SubscriberMethods.ContainsKey(id))
            {
                var arg = new object[] {EventArgs.Empty};
                foreach (var (method, instance) in SubscriberMethods[id])
                {
                    if (instance != null)
                        method.Invoke(instance, arg);
                }
            }

            foreach (var global in Global)
                global.Invoke(id, EventArgs.Empty);
        }

        /// <summary>
        /// Publishes an event to subscribers with optional parameters passed.
        /// </summary>
        public static void Publish(string id, EventArgs args)
        {
            if (Subscribers.ContainsKey(id))
            {
                var subscribers = Subscribers[id];
                foreach (var subscriber in subscribers)
                    subscriber.Invoke(args);
            }

            if (SubscriberMethods.ContainsKey(id))
            {
                var arg = new object[] {args};
                foreach (var (method, instance) in SubscriberMethods[id])
                {
                    if (instance != null)
                        method.Invoke(instance, arg);
                }
            }

            foreach (var global in Global)
                global.Invoke(id, args);
        }

        /// <summary>
        /// Subscribe to an asynchronous event with parameters.
        /// </summary>
        public static void Subscribe(string id, Tuple<MethodBase, object> onPublished)
        {
            if (!SubscriberMethods.ContainsKey(id))
                SubscriberMethods.Add(id, new HashSet<Tuple<MethodBase, object>>());

            SubscriberMethods[id].Add(onPublished);
        }

        /// <summary>
        /// Subscribe to an asynchronous event with parameters.
        /// </summary>
        public static void Subscribe(string id, Action<EventArgs> onPublished)
        {
            if (!Subscribers.ContainsKey(id))
                Subscribers.Add(id, new HashSet<Action<EventArgs>>());

            Subscribers[id].Add(onPublished);
        }

        /// <summary>
        /// Subscribe to an asynchronous event with parameters.
        /// </summary>
        public static void SubscribeAll(Action<string, EventArgs> onPublished)
        {
            Global.Add(onPublished);
        }

        /// <summary>
        /// Unsubscribe from an event.
        /// </summary>
        public static void Unsubscribe(string id, Action<EventArgs> onPublished)
        {
            if (!Subscribers.ContainsKey(id))
                return;

            Subscribers[id].Remove(onPublished);
        }

        /// <summary>
        /// Unsubscribe from an event.
        /// </summary>
        public static void Unsubscribe(string id, Tuple<MethodBase, object> onPublished)
        {
            if (!SubscriberMethods.ContainsKey(id))
                return;

            SubscriberMethods[id].Remove(onPublished);
        }

        /// <summary>
        /// Subscribe to an asynchronous event with parameters.
        /// </summary>
        public static void UnsubscribeAll(Action<string, EventArgs> onPublished)
        {
            Global.Remove(onPublished);
        }
    }
}