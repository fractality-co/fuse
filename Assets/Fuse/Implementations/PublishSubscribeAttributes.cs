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
	public sealed class PublishAttribute : PublishSubscribeAttribute, IFuseLifecycle
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

		public void OnEnter(MemberInfo target, object instance)
		{
			EventInfo eventInfo = target as EventInfo;
			if (eventInfo != null)
				eventInfo.AddEventHandler(instance, Notify);
		}

		public void OnExit(MemberInfo target, object instance)
		{
			EventInfo eventInfo = target as EventInfo;
			if (eventInfo != null)
				eventInfo.RemoveEventHandler(instance, Notify);
		}
	}

	/// <summary>
	/// When events attached to <see cref="PublishAttribute"/> are invoked, and the type specified here matches it will be invoked.
	/// Events are only processed while in the Active phase of the <see cref="Lifecycle"/>.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Event)]
	public sealed class SubscribeAttribute : PublishSubscribeAttribute, IFuseLifecycle
	{
		public uint Order
		{
			get { return 0; }
		}

		public Lifecycle Lifecycle
		{
			get { return Lifecycle.Active; }
		}

		private Pair<MemberInfo, object> _reference;

		public SubscribeAttribute(Enum type) : base(type.ToString())
		{
		}

		public SubscribeAttribute(string type) : base(type)
		{
		}

		public void OnEnter(MemberInfo target, object instance)
		{
			_reference = AddListener(target, instance);
		}

		public void OnExit(MemberInfo target, object instance)
		{
			RemoveListener(_reference);
			_reference = null;
		}
	}

	[ComVisible(false)]
	public abstract class PublishSubscribeAttribute : Attribute, IFuseNotifier
	{
		protected static readonly Handler Notify = NotifyListeners;

		private static readonly List<Action<string>> StateListeners = new List<Action<string>>();

		private static readonly Dictionary<string, List<Pair<MemberInfo, object>>> Listeners =
			new Dictionary<string, List<Pair<MemberInfo, object>>>();

		protected delegate void Handler(string type);

		private readonly string _type;

		protected PublishSubscribeAttribute(string type)
		{
			_type = type;

			if (!Listeners.ContainsKey(_type))
				Listeners.Add(_type, new List<Pair<MemberInfo, object>>());
		}

		protected Pair<MemberInfo, object> AddListener(MemberInfo target, object instance)
		{
			Pair<MemberInfo, object> reference = new Pair<MemberInfo, object>(target, instance);
			Listeners[_type].Add(reference);
			return reference;
		}

		protected void RemoveListener(Pair<MemberInfo, object> reference)
		{
			Listeners[_type].Remove(reference);
		}

		private static void NotifyListeners(string type)
		{
			foreach (Pair<MemberInfo, object> reference in Listeners[type])
			{
				if (reference.A is MethodInfo)
					((MethodInfo) reference.A).Invoke(reference.B, null);
			}

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