using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Fuse.Feature
{
	/// <summary>
	/// Invokes a method or event when entering the <see cref="Lifecycle"/> phase.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Event)]
	public sealed class InvokeAttribute : Attribute, IFuseExecutor
	{
		public uint Order
		{
			get;
			[UsedImplicitly]
			private set;
		}

		public Lifecycle Lifecycle
		{
			get;
			[UsedImplicitly]
			private set;
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