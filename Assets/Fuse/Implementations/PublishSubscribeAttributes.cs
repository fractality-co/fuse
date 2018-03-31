using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Fuse.Core;
using JetBrains.Annotations;

namespace Fuse.Implementation
{
	/// <summary>
	/// Adds hook to an <code>event</code> defined, that will call <see cref="SubscribeAttribute"/> with corresponding type."/>
	/// Events are only processed while in the Active phase of the <see cref="Lifecycle"/>.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Event)]
	public sealed class PublishAttribute : PublishSubscribeAttribute, IFuseLifecycle<EventInfo>
	{
		public uint Order
		{
			get { return 0; }
		}

		public Lifecycle Lifecycle
		{
			get { return Lifecycle.Active; }
		}

		public PublishAttribute(Enum type) : base(type.ToString())
		{
		}

		public PublishAttribute(string type) : base(type)
		{
		}

		public void OnEnter(EventInfo target, object instance)
		{
			target.AddEventHandler(instance, Notify);
		}

		public void OnExit(EventInfo target, object instance)
		{
			target.RemoveEventHandler(instance, Notify);
		}
	}

	/// <summary>
	/// When events attached to <see cref="PublishAttribute"/> are invoked, and the type specified here matches it will be invoked.
	/// Events are only processed while in the Active phase of the <see cref="Lifecycle"/>.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Event)]
	public sealed class SubscribeAttribute : PublishSubscribeAttribute, IFuseLifecycle<MethodInfo>,
		IFuseLifecycle<EventInfo>
	{
		public uint Order
		{
			get { return 0; }
		}

		public Lifecycle Lifecycle
		{
			get { return Lifecycle.Active; }
		}

		private Pair<MethodInfo, object> _reference;

		public SubscribeAttribute(Enum type) : base(type.ToString())
		{
		}

		public SubscribeAttribute(string type) : base(type)
		{
		}

		public void OnEnter(MethodInfo target, object instance)
		{
			_reference = AddListener(target, instance);
		}

		public void OnExit(MethodInfo target, object instance)
		{
			RemoveListener(_reference);
			_reference = null;
		}

		public void OnEnter(EventInfo target, object instance)
		{
			OnEnter(target.GetRaiseMethod(), instance);
		}

		public void OnExit(EventInfo target, object instance)
		{
			OnExit(target.GetRaiseMethod(), instance);
		}
	}

	[ComVisible(false)]
	public abstract class PublishSubscribeAttribute : Attribute, IFuseNotifier
	{
		protected static readonly Handler Notify = NotifyListeners;

		private static readonly List<Action<string>> StateListeners = new List<Action<string>>();

		private static readonly Dictionary<string, List<Pair<MethodInfo, object>>> Listeners =
			new Dictionary<string, List<Pair<MethodInfo, object>>>();

		protected delegate void Handler(string type);

		private readonly string _type;

		protected PublishSubscribeAttribute(string type)
		{
			_type = type;

			if (!Listeners.ContainsKey(_type))
				Listeners.Add(_type, new List<Pair<MethodInfo, object>>());
		}

		protected Pair<MethodInfo, object> AddListener(MethodInfo target, object instance)
		{
			Pair<MethodInfo, object> reference = new Pair<MethodInfo, object>(target, instance);
			Listeners[_type].Add(reference);
			return reference;
		}

		protected void RemoveListener(Pair<MethodInfo, object> reference)
		{
			Listeners[_type].Remove(reference);
		}

		private static void NotifyListeners(string type)
		{
			foreach (Pair<MethodInfo, object> reference in Listeners[type])
				reference.A.Invoke(reference.B, null);

			StateListeners.ForEach(callback => callback(type));
		}

		public void AddListener(Action<string> callback)
		{
			StateListeners.Add(callback);
		}

		public void RemoveListener(Action<string> callback)
		{
			StateListeners.Remove(callback);
		}
	}
}