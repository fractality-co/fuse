using System;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;

namespace Fuse.Feature
{
	/// <summary>
	/// Invokes the method into the <see cref="ThreadPool"/>.
	/// By default, this executes on the Active <see cref="Lifecycle"/>.
	/// WARNING: You are responsible for maintaining scope. Make sure what you are doing is thread-safe (Unity is not).
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method)]
	[DefaultLifecycle(Lifecycle.Active)]
	public class ThreadAttribute : Attribute, IFuseExecutor
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
			ThreadPool.QueueUserWorkItem(context => { ((MethodInfo) target).Invoke(instance, null); });
		}
	}
}