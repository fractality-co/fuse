using System;
using JetBrains.Annotations;

namespace iMVC
{
	/// <summary>
	/// Invokes a method when the object is done loading.
	/// Supports void, bool and IEnumerator (will wait for completion) return types (no arguments).
	/// Before this invokes, <see cref="InjectAttribute"/> will have been resolved.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class SetupAttribute : Attribute
	{
		public readonly uint Order;

		/// <summary>
		/// Invokes method when the object is done loading. Order passed defines invocation order.
		/// </summary>
		public SetupAttribute(uint order = 0)
		{
			Order = order;
		}
	}

	/// <summary>
	/// Invokes a method when the object is about to be unloaded, injections are still valid at this step.
	/// <see cref="InjectAttribute"/> will be unloaded after cleanup.
	/// Order passed defines invocation order.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class CleanupAttribute : Attribute
	{
		public readonly uint Order;

		/// <summary>
		/// Invokes method when the object is about to be unloaded, <see cref="InjectAttribute"/>s are still valid.
		/// Order passed defines invocation order.
		/// </summary>
		public CleanupAttribute(uint order = 0)
		{
			Order = order;
		}
	}

	/// <summary>
	/// Attempts to resolve the dependency, and is assigned before <see cref="SetupAttribute"/> is invoked.
	/// For an implementation of iMVC (Model, View or Controller) we return a loaded instance, if none exist we load one.
	/// For a <see cref="UnityEngine.GameObject"/> or <see cref="UnityEngine.MonoBehaviour"/>, we search within Scene(s).
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class InjectAttribute : Attribute
	{
		public readonly string Name;
		public readonly bool ExactMatch;

		public bool HasName
		{
			get { return Name == string.Empty; }
		}

		/// <summary>
		/// Injects the first / default dependency that matches the assigned type.
		/// </summary>
		public InjectAttribute()
		{
		}

		/// <summary>
		/// Injects the dependency that matches the passed name and assigned type.
		/// By default, it will check full equality but you may have it do a contains check instead.
		/// </summary>
		public InjectAttribute(string name, bool exactMatch = true)
		{
			Name = name;
			ExactMatch = exactMatch;
		}
	}
}