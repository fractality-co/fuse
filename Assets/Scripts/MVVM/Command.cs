using System;

namespace MVVM
{
    /// <summary>
    /// Generic base event type for async communication throughout MVC.
    /// Use either <see cref="Fuse.Events"/> static entry-point, or in <see cref="ViewModel{T,TJ}"/>,
    /// the injection attributes (<see cref="Fuse.PublishAttribute"/> / <see cref="Fuse.SubscribeAttribute"/>).
    /// </summary>
    public abstract class Command : EventArgs
    {
        /// <summary>
        /// Determines whether or not this event has been consumed or not.
        /// It is up to the <see cref="ViewModel{T,TJ}"/> how this is interpreted.
        /// </summary>
        public bool Processed { get; private set; }

        /// <summary>
        /// Consumes the event and so other <see cref="ViewModel{T,TJ}"/>s can avoid processing it.
        /// It is up to the implementer to handle whether or not their logic should handle this.
        /// </summary>
        public bool Process()
        {
            if (Processed) return false;
            Processed = true;
            return true;
        }
    }
}