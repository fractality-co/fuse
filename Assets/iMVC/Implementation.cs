using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace iMVC
{
	[UsedImplicitly]
	public sealed class ViewAttribute : ImplementationAttribute
	{
		public ViewAttribute()
		{
		}

		public ViewAttribute(string[] instances = null) : base(instances)
		{
		}
	}

	[UsedImplicitly]
	public sealed class ModelAttribute : ImplementationAttribute
	{
		public ModelAttribute()
		{
		}

		public ModelAttribute(string[] instances = null) : base(instances)
		{
		}
	}

	[UsedImplicitly]
	public sealed class ControllerAttribute : ImplementationAttribute
	{
		public ControllerAttribute()
		{
		}

		public ControllerAttribute(string[] instances = null) : base(instances)
		{
		}
	}

	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Class)]
	[BaseTypeRequired(typeof(ScriptableObject))]
	public abstract class ImplementationAttribute : Attribute
	{
		[UsedImplicitly] public readonly string[] Instances;

		protected ImplementationAttribute()
		{
		}

		protected ImplementationAttribute(string[] instances = null)
		{
			Instances = instances;
		}

		public IEnumerable<string> GetAssets(Type type)
		{
			return Instances == null || Instances.Length == 0 ? new[] {"Instance"} : Instances;
		}

		public override string ToString()
		{
			return GetType().Name.Replace(typeof(Attribute).Name, string.Empty);
		}
	}
}