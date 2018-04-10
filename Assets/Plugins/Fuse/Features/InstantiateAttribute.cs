using System;
using System.Reflection;
using Fuse.Core;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace Fuse.Feature
{
	/// <summary>
	/// Instantiates a reference to a <see cref="UnityEngine.Object"/> when entering lifecyle, and destroys it upon exit.
	/// </summary>
	[MeansImplicitUse]
	[DefaultLifecycle(Lifecycle.Active)]
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class InstantiateAttribute : Attribute, IFuseLifecycle
	{
		public uint Order { get; set; }
		public Lifecycle Lifecycle { get; set; }

		public void OnEnter(MemberInfo target, object instance)
		{
			Object prefab = (Object) target.GetValue(instance);
			if (prefab != null)
			{
				target.SetValue(instance, Object.Instantiate(prefab));
			}
		}

		public void OnExit(MemberInfo target, object instance)
		{
			Object.Destroy((Object) target.GetValue(instance));
			target.SetValue(instance, null);
		}
	}
}