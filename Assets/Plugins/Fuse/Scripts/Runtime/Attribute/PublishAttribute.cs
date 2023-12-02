using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Fuse
{
    /// <summary>
    /// Publish an event through <see cref="Relay"/>, which will notify all subscribers.
    /// Events are only processed while in the Active phase of the <see cref="Lifecycle" />.
    /// Requires a type of <see cref="EventHandler" />, and when invoking pass (this, EventArgs.empty ?? yourEventArgs).
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Event)]
    [Document("Events",
        "Publish an event through the Relay, which will notify all subscribers. " +
        "This is leveraged using the C# delegate system, so requires a return type of EventHandler and passing event args. " + 
        "You may pass any custom event args through the system to encapsulate results or state." + 
        "\n\n[Publish(\"event.id\")] private event EventHandler _onContinue")]
    public sealed class PublishAttribute : Attribute, IFusibleLifecycle
    {
        private readonly string _id;
        private readonly object[] _handler;

        /// <summary>
        /// Publish an event through <see cref="Relay"/>, which will notify all subscribers.
        /// Events are only processed while in the Active phase of the <see cref="Lifecycle" />.
        /// Requires a type of <see cref="EventHandler" />, and when invoking pass (this, EventArgs.empty ?? yourEventArgs).
        /// </summary>
        public PublishAttribute(string type)
        {
            _id = type;
            _handler = new object[] {new EventHandler(OnEvent)};
        }

        public uint Order => 0;

        public Lifecycle Lifecycle => Lifecycle.Active;

        public void OnEnter(MemberInfo target, object instance)
        {
            if (target is EventInfo eventInfo)
            {
                var addMethod = eventInfo.GetAddMethod(true) ?? eventInfo.GetAddMethod(false);
                addMethod.Invoke(instance, _handler);
            }
        }

        public void OnExit(MemberInfo target, object instance)
        {
            if (target is EventInfo eventInfo)
            {
                var removeMethod = eventInfo.GetRemoveMethod(true) ?? eventInfo.GetRemoveMethod(false);
                removeMethod.Invoke(instance, _handler);
            }
        }

        private void OnEvent(object sender, EventArgs eventArgs)
        {
            Relay.Publish(_id, eventArgs);
        }
    }
}