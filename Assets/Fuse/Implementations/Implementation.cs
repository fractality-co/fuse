﻿using System;
using JetBrains.Annotations;
using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NotAccessedField.Global
// ReSharper disable UnusedMember.Global
namespace Fuse.Implementation
{
	/// <summary>
	/// Marks a class as an implementation for processing by the framework (<see cref="Fuse"/>).
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Class)]
	[BaseTypeRequired(typeof(ScriptableObject))]
	public class ImplementationAttribute : Attribute
	{
		public override string ToString()
		{
			return GetType().Name.Replace(typeof(Attribute).Name, string.Empty);
		}
	}
}