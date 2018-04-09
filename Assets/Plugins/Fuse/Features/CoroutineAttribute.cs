using System;
using System.Collections;
using System.Reflection;
using Fuse.Core;
using JetBrains.Annotations;

namespace Fuse.Feature
{
	/// <summary>
	/// Starts a asynchronous coroutine on a method that has a return type of <see cref="T:System.Collections.IEnumerator" />.
	/// The coroutine is started at the <see cref="Lifecycle"/> specified, but stopped when <see cref="Feature"/> is unloaded.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method)]
	[DefaultLifecycle(Lifecycle.Active)]
	public sealed class CoroutineAttribute : Attribute, IFuseExecutorAsync
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

		public IEnumerator Execute(MemberInfo target, object instance)
		{
			if (target is MethodInfo)
			{
				MethodInfo methodInfo = target as MethodInfo;
				if (methodInfo.ReturnType != typeof(IEnumerator))
					throw new NotImplementedException("Only a " + typeof(IEnumerator) + " method is supported.");

				yield return methodInfo.Invoke(instance, null);
			}
			else
				Logger.Warn("Unsupported target (" + target.GetType().Name + ")");
		}
	}
}