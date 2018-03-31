using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Fuse.Implementation
{
	/// <summary>
	/// Invokes a method or event when entering the <see cref="Lifecycle"/> phase.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Event)]
	public sealed class InvokeAttribute : Attribute, IFuseInjection<MethodInfo>, IFuseInjection<EventInfo>
	{
		public uint Order { get; [UsedImplicitly] private set; }
		public Lifecycle Lifecycle { get; [UsedImplicitly] private set; }

		public void Process(MethodInfo target, object instance)
		{
			target.Invoke(instance, null);
		}

		public void Process(EventInfo target, object instance)
		{
			target.GetRaiseMethod().Invoke(instance, null);
		}
	}
}