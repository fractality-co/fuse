using System;
using JetBrains.Annotations;

namespace iMVC
{
	/// <summary>
	/// Invokes a method when the object is being loaded.
	/// Before this invokes, injections will have been loaded.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class SetupAttribute : Attribute
	{
		public readonly uint Order;

		public SetupAttribute(uint order = 0)
		{
			Order = order;
		}
	}

	/// <summary>
	/// Invokes a method when the object is being unloaded.
	/// Injections will be automatically unloaded after cleanup.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class CleanupAttribute : Attribute
	{
		public readonly uint Order;

		public CleanupAttribute(uint order = 0)
		{
			Order = order;
		}
	}


	/// <summary>
	/// Injects a dependency by setting a field/property or invoking a method.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public sealed class InjectAttribute : Attribute
	{
		public readonly string Name;

		public bool HasName
		{
			get { return Name == string.Empty; }
		}

		/// <summary>
		/// Injects the first / default dependency.
		/// </summary>
		public InjectAttribute()
		{
		}

		/// <summary>
		/// Injects the dependency that matches the passed name.
		/// </summary>
		public InjectAttribute(string name)
		{
			Name = name;
		}
	}
}