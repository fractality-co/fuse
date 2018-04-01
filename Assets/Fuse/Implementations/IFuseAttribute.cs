using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace Fuse.Implementation
{
	public interface IFuseAttribute
	{
		uint Order { get; }
		Lifecycle Lifecycle { get; }
	}

	public interface IFuseNotifier
	{
		void AddListener(Action<string> callback);
		void RemoveListener(Action<string> callback);
	}

	/// <summary>
	/// <see cref="Fuse"/> will process all attributes that implement this interface.
	/// </summary>
	public interface IFuseInjection<in T> : IFuseAttribute where T : MemberInfo
	{
		void Process(T target, object instance);
	}

	/// <summary>
	/// <see cref="Fuse"/> will process on a new coroutine all attributes that implement this interface.
	/// </summary>
	public interface IFuseInjectionAsync<in T> : IFuseAttribute where T : MemberInfo
	{
		IEnumerator Process(T target, object instance);
	}

	public interface IFuseLifecycle<in T> : IFuseAttribute where T : MemberInfo
	{
		void OnEnter(T target, object instance);
		void OnExit(T target, object instance);
	}

	/// <summary>
	/// Represents the individual phases that a <see cref="ImplementationAttribute"/> can be in. 
	/// </summary>
	[DefaultValue(Active)]
	public enum Lifecycle
	{
		None,
		Load,
		Setup,
		Active,
		Cleanup,
		Unload
	}
}