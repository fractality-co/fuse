using System;
using JetBrains.Annotations;
using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NotAccessedField.Global
// ReSharper disable UnusedMember.Global
namespace Fuse.Feature
{
	/// <summary>
	/// Marks a class as an feature for processing by the framework (<see cref="Fuse"/>).
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Class)]
	[BaseTypeRequired(typeof(ScriptableObject))]
	public class FeatureAttribute : Attribute
	{
		public override string ToString()
		{
			return GetType().Name.Replace(typeof(Attribute).Name, string.Empty);
		}
	}
}