using System;
using JetBrains.Annotations;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NotAccessedField.Global
// ReSharper disable UnusedMember.Global
namespace Fuse.Implementation
{
	/// <summary>
	/// Finds any <see cref="UnityEngine.Object"/>(s) by name and is assigned before <see cref="SetupAttribute"/> is invoked.
	/// Passing type will restirct it's search to those with that type.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public sealed class FindByNameAttribute : Attribute
	{
		public readonly string Name;
		public readonly Type Type;

		public FindByNameAttribute(string name)
		{
			Name = name;
		}

		public FindByNameAttribute(string name, Type type)
		{
			Name = name;
			Type = type;
		}
	}

	/// <summary>
	/// Finds <see cref="UnityEngine.GameObject"/>(s) with tag and is assigned before <see cref="SetupAttribute"/> is invoked.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public sealed class FindByTagAttribute : Attribute
	{
		public readonly string Tag;

		public FindByTagAttribute(string tag)
		{
			Tag = tag;
		}
	}

	/// <summary>
	/// Preloads <see cref="ImplementationAttribute"/>(s) and is assigned before <see cref="SetupAttribute"/> is invoked.
	/// This supports single instances, arrays and lists.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public sealed class PreloadAttribute : Attribute
	{
		public readonly string Name;

		public bool HasName
		{
			get { return Name == string.Empty; }
		}

		public PreloadAttribute()
		{
		}

		public PreloadAttribute(string name)
		{
			Name = name;
		}
	}

	/// <summary>
	/// Mark the event to initiate loading of a single dependency with the assigned type (and optionally name).
	/// Use the corresponding <see cref="LoadCompleteAttribute"/> to receive the loaded dependency.
	/// In addition there is: <see cref="LoadErrorAttribute"/> and <see cref="LoadProgressAttribute"/>.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Event)]
	public sealed class LoadAttribute : Attribute
	{
		public readonly Type Type;
		public readonly string Name;

		public LoadAttribute(Type type)
		{
			Type = type;
		}

		public LoadAttribute(Type type, string name)
		{
			Type = type;
			Name = name;
		}
	}

	/// <summary>
	/// Mark the event to initiate loading of all dependencies with the assigned type.
	/// Use the corresponding <see cref="LoadCompleteAttribute"/> to receive the loaded dependency.
	/// In addition there is: <see cref="LoadErrorAttribute"/> and <see cref="LoadProgressAttribute"/>.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Event)]
	public sealed class LoadAllAttribute : Attribute
	{
		public readonly Type Type;

		public LoadAllAttribute(Type type)
		{
			Type = type;
		}
	}

	/// <summary>
	/// Injects the dependenc(y/ies) that matches corresponding <see cref="LoadCompleteAttribute"/> or <see cref="LoadAllAttribute"/>.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class LoadCompleteAttribute : Attribute
	{
		public readonly string Name;

		public LoadCompleteAttribute()
		{
		}

		public LoadCompleteAttribute(string name)
		{
			Name = name;
		}
	}

	/// <summary>
	/// Updates an assigned <see cref="float"/>.
	/// Matches for the corresponding <see cref="LoadAllAttribute"/> or <see cref="LoadAttribute"/>.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Event)]
	public sealed class LoadProgressAttribute : Attribute
	{
		public readonly Type Type;
		public readonly string Name;

		public LoadProgressAttribute(Type type)
		{
			Type = type;
		}

		/// <summary>
		/// Only supported by <see cref="LoadAttribute"/>.
		/// </summary>
		public LoadProgressAttribute(Type type, string name)
		{
			Type = type;
			Name = name;
		}
	}

	/// <summary>
	/// Assigns or passes a <see cref="string"/> message for why the load failed.
	/// Matches for the corresponding <see cref="LoadAllAttribute"/> or <see cref="LoadAttribute"/>.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Event)]
	public sealed class LoadErrorAttribute : Attribute
	{
		public readonly Type Type;
		public readonly string Name;

		public LoadErrorAttribute(Type type)
		{
			Type = type;
		}

		/// <summary>
		/// Only supported by <see cref="LoadAttribute"/>.
		/// </summary>
		public LoadErrorAttribute(Type type, string name)
		{
			Type = type;
			Name = name;
		}
	}
}