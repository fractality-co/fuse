/*
 * Copyright (2021) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Fuse
{
    /// <summary>
    /// Simple subscriber component that listens to events from FUSE's <see cref="Events"/>.
    /// </summary>
    [Document("Events",
        "Simple implementation of a component that subscribes to events coming from the Relay." +
        "\n\nAttach this to a GameObject in your scene or prefab, simply assign the event you want then add your custom reactions.")]
    public class Subscriber : MonoBehaviour
    {
        public string @event = "event.id";
        public SubscribeEvent onEvent;
        public Action<EventArgs> onPublished;

        private void OnEnable()
        {
            Events.Subscribe(@event, OnPublished);
        }

        private void OnDisable()
        {
            Events.Unsubscribe(@event, OnPublished);
        }

        private void OnPublished(EventArgs eventArgs)
        {
            onPublished?.Invoke(eventArgs);
            onEvent?.Invoke(eventArgs);
        }
    }

    /// <summary>
    /// Custom UnityEvent with encapsulated <see cref="EventArgs"/>.
    /// </summary>
    [Serializable]
    public class SubscribeEvent : UnityEvent<EventArgs>
    {
    }
}