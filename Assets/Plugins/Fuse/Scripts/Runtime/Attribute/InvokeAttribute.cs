using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Fuse
{
    /// <summary>
    /// Invokes a method or event when entering the <see cref="Lifecycle" /> phase.
    /// By default, this executes on the Active <see cref="Lifecycle" />.
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Event)]
    [DefaultLifecycle(Lifecycle.Active)]
    [Document("Module",
        "Executes a method (arbitrary name) at the Lifecycle specified (default is Active). " + 
        "It is quite common though to create a setup and/or cleanup method to initialize run-time needs." + 
        "\n\n[Invoke] private void Active() { ... }" + 
        "\n[Invoke(Lifecycle.Setup)] private void Setup() { }" +
        "\n[Invoke(Lifecycle.Cleanup)] ... void Cleanup() { }")]
    public sealed class InvokeAttribute : Attribute, IFusibleInvoke
    {
        public uint Order { get; [UsedImplicitly] set; }

        public Lifecycle Lifecycle { get; [UsedImplicitly] set; }

        public void Invoke(MemberInfo target, object instance)
        {
            switch (target)
            {
                case MethodInfo methodInfo:
                    methodInfo.Invoke(instance, null);
                    break;
                case EventInfo eventInfo:
                    eventInfo.GetRaiseMethod().Invoke(instance, null);
                    break;
            }
        }
    }
}