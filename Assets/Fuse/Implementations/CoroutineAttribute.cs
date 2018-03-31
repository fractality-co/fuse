using System;
using System.Collections;
using System.Reflection;
using JetBrains.Annotations;

namespace Fuse.Implementation
{
	/// <summary>
	/// Starts a asynchronous coroutine on a method that has a return type of <see cref="T:System.Collections.IEnumerator" />.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class CoroutineAttribute : Attribute, IFuseInjectionAsync<MethodInfo>
	{
		public uint Order { get; [UsedImplicitly] private set; }
		public Lifecycle Lifecycle { get; [UsedImplicitly] private set; }

		public IEnumerator Process(MethodInfo target, object instance)
		{
			if (target.ReturnType != typeof(IEnumerator))
				throw new NotImplementedException("Only a " + typeof(IEnumerator) + " method is supported.");

			yield return target.Invoke(instance, null);
		}
	}
}