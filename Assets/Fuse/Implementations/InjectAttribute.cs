using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace Fuse.Implementation
{
	/// <summary>
	/// Assigns a dependency based on its type and is assigned before <code>"Lifecycle.Setup"</code>(s) are invoked.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public sealed class InjectAttribute : Attribute, IFuseInjection<PropertyInfo>, IFuseInjection<FieldInfo>,
		IFuseInjection<MethodInfo>
	{
		public uint Order { get; [UsedImplicitly] private set; }
		public Lifecycle Lifecycle { get; [UsedImplicitly] private set; }

		private readonly Regex _regex;

		public InjectAttribute(string regex = "*")
		{
			_regex = new Regex(regex);
		}

		public void Process(MethodInfo target, object instance)
		{
			ParameterInfo[] parameters = target.GetParameters();
			target.Invoke(instance, parameters.Select(parameter => GetValue(parameter.ParameterType)).ToArray());
		}

		public void Process(PropertyInfo target, object instance)
		{
			target.SetValue(instance, GetValue(target.PropertyType), null);
		}

		public void Process(FieldInfo target, object instance)
		{
			target.SetValue(instance, GetValue(target.FieldType));
		}

		private object GetValue(Type valueType)
		{
			if (valueType.IsAssignableFrom(typeof(IList)))
			{
				Object[] values = Object.FindObjectsOfType(valueType);
				IList value = (IList) Convert.ChangeType(Activator.CreateInstance(valueType), valueType);

				if (value != null)
				{
					foreach (Object obj in values)
					{
						if (_regex.IsMatch(obj.name))
							value.Add(obj);
					}
				}

				return value;
			}

			if (!valueType.IsAssignableFrom(typeof(IEnumerable)))
			{
				return Object.FindObjectsOfType(valueType).First(current => _regex.IsMatch(current.name));
			}

			throw new NotImplementedException(valueType.Name + " is not supported.");
		}
	}
}