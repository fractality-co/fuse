using System;
using System.Collections;
using UnityEngine;

namespace Fuse
{
    /// <summary>
    /// Simple event notifier that delays events by a configurable (with fallback set to 5) event publishing.
    /// </summary>
    [Document("Demo", 
        "Simple event notifier that delays events by a configurable (with fallback set to 5) event publishing. " + 
        "Assign this to any global or state scope, which will wait the delay then notify 'delayed.event' for you to utilize.\n\n" + 
        "You may override the timing by having using the 'Default' Content and setting a property named 'delayed.event' with the int seconds.")]
    public class DelayedEvent : Module
    {
        public const string EventId = "delayed.event";
        public const int DefaultDelay = 5;

        [Inject("Default")] private Content _content;

        [Publish(EventId)] private event EventHandler OnEvent;

        [Coroutine]
        private IEnumerator Active()
        {
            yield return new WaitForSeconds(_content.HasValue(EventId) ? _content.GetNumber(EventId) : DefaultDelay);
            OnEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}