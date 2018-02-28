using System;
using JetBrains.Annotations;

namespace iMVC
{
    /// <summary>
    /// Invokes a method based off a timed interval passed (millseconds).
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TickAttribute : Attribute
    {
        [UsedImplicitly]
        public readonly TimeSpan Interval;

        public TickAttribute(double milliseconds)
        {
            Interval = TimeSpan.FromMilliseconds(milliseconds);
        }
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class InputAttribute : Attribute
    {
        [UsedImplicitly]
        public readonly string Button;

        public InputAttribute(string button)
        {
            Button = button;
        }
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PublishAttribute : PublishSubcribeAttribute
    {
        public PublishAttribute(string type) : base(type)
        {
        }
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SubscribeAttribute : PublishSubcribeAttribute
    {
        public SubscribeAttribute(string type) : base(type)
        {
        }
    }

    public abstract class PublishSubcribeAttribute : Attribute
    {
        [UsedImplicitly]
        public readonly string Type;

        protected PublishSubcribeAttribute(string type)
        {
            Type = type;
        }
    }
}