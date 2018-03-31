using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Fuse.Implementation
{
	/// <summary>
	/// Adds hook to an <code>event</code> defined, that will call <see cref="SubscribeAttribute"/> with corresponding type."/>
	/// Events are only processed while in the Active phase of the <see cref="Lifecycle"/>.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Event)]
	public sealed class PublishAttribute : Attribute, IFuseLifecycle<EventInfo>
	{
		public uint Order
		{
			get { return 0; }
		}

		public Lifecycle Lifecycle
		{
			get { return Lifecycle.Active; }
		}

		public readonly string Type;

		public PublishAttribute(string type)
		{
			Type = type;
		}

		public void OnEnter(EventInfo target, object instance)
		{
			throw new NotImplementedException();
		}

		public void OnExit(EventInfo target, object instance)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// When events attached to <see cref="PublishAttribute"/> are invoked, and the type specified here matches it will be invoked.
	/// Events are only processed while in the Active phase of the <see cref="Lifecycle"/>.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class SubscribeAttribute : Attribute, IFuseLifecycle<MethodInfo>
	{
		public uint Order
		{
			get { return 0; }
		}

		public Lifecycle Lifecycle
		{
			get { return Lifecycle.Active; }
		}

		public readonly string Type;

		public SubscribeAttribute(string type)
		{
			Type = type;
		}

		public void OnEnter(MethodInfo target, object instance)
		{
			throw new NotImplementedException();
		}

		public void OnExit(MethodInfo target, object instance)
		{
			throw new NotImplementedException();
		}
	}
}