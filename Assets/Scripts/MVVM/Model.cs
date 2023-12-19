using System;
using Fuse;

namespace MVVM
{
    public abstract class Model : Module
    {
        public abstract bool IsReady { get; }

        [Invoke]
        protected abstract void Setup();

        [Invoke(Lifecycle = Lifecycle.Cleanup)]
        protected abstract void Cleanup();

        protected static void Notify(ModelEvent modelEvent)
        {
            Events.Publish(modelEvent);
        }
    }

    public abstract class ModelEvent : EventArgs
    {
    }
}