using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace Fuse.Feature
{
	/// <summary>
	/// Base interface that <see cref="Fuse"/> uses to mark attributes for processing within a specific <see cref="Lifecycle"/>.
	/// </summary>
	public interface IFuseAttribute
	{
		uint Order { get; }
		Lifecycle Lifecycle { get; }
	}

	/// <summary>
	/// Allows for communication of events to <see cref="Fuse"/>.
	/// </summary>
	public interface IFuseNotifier
	{
		void AddListener(Action<string> callback);
		void RemoveListener(Action<string> callback);
	}

	/// <summary>
	/// <see cref="N:Fuse" /> will process all attributes that implement this interface.
	/// </summary>
	/// <inheritdoc />
	public interface IFuseExecutor : IFuseAttribute
	{
		void Execute(MemberInfo target, object instance);
	}

	/// <summary>
	/// <see cref="N:Fuse" /> will process on a new coroutine all attributes that implement this interface.
	/// </summary>
	/// <inheritdoc />
	public interface IFuseExecutorAsync : IFuseAttribute
	{
		IEnumerator Execute(MemberInfo target, object instance);
	}

	/// <summary>
	/// <see cref="N:Fuse" /> invokes the attribute when enterring or exitting the assigned <see cref="T:Fuse.Feature.Lifecycle" />.
	/// </summary>
	/// <inheritdoc />
	public interface IFuseLifecycle : IFuseAttribute
	{
		void OnEnter(MemberInfo target, object instance);
		void OnExit(MemberInfo target, object instance);
	}

	/// <summary>
	/// Represents the individual phases that a <see cref="FeatureAttribute"/> can be in.
	/// By default, we use the Active phase.
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