using System;
using JetBrains.Annotations;

namespace Fuse.Implementation
{
	/// <summary>
	/// Assigns a dependency based on its type and is assigned before <see cref="SetupAttribute"/>(s) are invoked.
	/// This supports single instances, arrays and lists.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public sealed class InjectAttribute : Attribute
	{
		[UsedImplicitly] public readonly string Name;

		[UsedImplicitly]
		public bool HasName
		{
			get { return !string.IsNullOrEmpty(Name); }
		}

		public InjectAttribute()
		{
		}

		public InjectAttribute(string name)
		{
			Name = name;
		}
	}
}