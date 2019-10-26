using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Fuse.Feature
{
	/// <summary>
	/// Invokes a method or event when entering the <see cref="Lifecycle"/> phase.
	/// By default, this executes on the Active <see cref="Lifecycle"/>.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Event)]
	[DefaultLifecycle(Lifecycle.Active)]
	public sealed class InvokeAttribute : Attribute, IFuseExecutor
	{
		public uint Order
		{
			get;
			[UsedImplicitly]
			set;
		}

		public Lifecycle Lifecycle
		{
			get;
			[UsedImplicitly]
			set;
		}

		public void Execute(MemberInfo target, object instance)
		{
			if (target is MethodInfo)
				((MethodInfo) target).Invoke(instance, null);
			else if (target is EventInfo)
				((EventInfo) target).GetRaiseMethod().Invoke(instance, null);
		}
	}
}