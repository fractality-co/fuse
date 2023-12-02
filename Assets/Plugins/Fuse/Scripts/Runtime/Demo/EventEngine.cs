using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Fuse
{
    /// <summary>
    /// This module will fire off events every second to act as a constant engine for literally anything.
    /// </summary>
    [Document("Demo",
        "This module will fire off events every second to act as a constant engine for powering literally anything. " + 
        "The event that the module fires off is 'engine.event', simply subscribe to this (see Events) and have fun.\n\n" +
        "To use this, assign it to either a state or global scope.")]
    public class EventEngine : Module
    {
        public const string Event = "engine.event";

        [Inject] private DelayedEvent _example;
        
        [Publish(Event)] private event EventHandler OnEvent;

        [Coroutine]
        [SuppressMessage("ReSharper", "IteratorNeverReturns")]
        private IEnumerator Active()
        {
            while (IsActive)
            {
                yield return new WaitForSeconds(1f);
                OnEvent?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}