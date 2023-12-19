/*
 * Copyright (2020) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System;
using JetBrains.Annotations;

namespace Fuse
{
    /// <summary>
    /// Encapsulates the settings for a transition to another <see cref="State"/>.
    /// </summary>
    [Serializable]
    [Document("Core",
        "Transitions are controlled by the configuration within a state that assigns an event with a state to transition to. " +
        "Fire events from a publisher either via the Publisher component, the PublisherAttribute in a module or the Relay static class." +
        "\n\nOnce a transition has been triggered, FUSE will load/unload dependencies for you.")]
    public class Transition
    {
        /// <summary>
        /// What event needs to be fired off from <see cref="PublishAttribute"/> for this transition to become active.
        /// </summary>
        [UsedImplicitly] public string Event;

        /// <summary>
        /// What state to transition to once all Events are fired off.
        /// </summary>
        [AssetReference(typeof(State))] public string To;

        public Transition(string to, string @event)
        {
            To = to;
            Event = @event;
        }
    }
}