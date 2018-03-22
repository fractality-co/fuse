using System;
using System.Collections;
using Fuse.Core;
using JetBrains.Annotations;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NotAccessedField.Global
// ReSharper disable UnusedMember.Global
namespace Fuse.Implementation
{
	/// <summary>
	/// Invokes a method when the object is done loading.
	/// Supports return types of void and <see cref="IEnumerator"/> (will wait for completion).
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class SetupAttribute : Attribute
	{
		public readonly uint Order;

		public SetupAttribute(uint order = 100)
		{
			Order = order;
		}
	}

	/// <summary>
	/// Invokes a method when the object needs to be unloaded.
	/// Supports return types of void and <see cref="IEnumerator"/> (will wait for completion).
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class CleanupAttribute : Attribute
	{
		public readonly uint Order;

		public CleanupAttribute(uint order = 100)
		{
			Order = order;
		}
	}

	/// <summary>
	/// Process only when the build is in the specified environment.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.All)]
	public sealed class BuildModeAttribute : Attribute
	{
		public readonly BuildMode BuildMode;

		public BuildModeAttribute(BuildMode buildMode)
		{
			BuildMode = buildMode;
		}
	}
}