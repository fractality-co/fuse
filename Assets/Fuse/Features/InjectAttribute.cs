using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Fuse.Core;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace Fuse.Feature
{
	/// <summary>
	/// Assigns a dependency based on its type and is assigned before <code>"Lifecycle.Setup"</code>(s) are invoked.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	public sealed class InjectAttribute : Attribute, IFuseExecutor
	{
		public uint Order
		{
			get;
			[UsedImplicitly]
			private set;
		}

		public Lifecycle Lifecycle
		{
			get { return Lifecycle.Load; }
		}

		private readonly Regex _regex;

		public InjectAttribute(string regex = "*")
		{
			_regex = new Regex(regex);
		}

		public void Execute(MemberInfo target, object instance)
		{
			if (target is MethodInfo)
			{
				MethodInfo methodInfo = (MethodInfo) target;
				ParameterInfo[] parameters = methodInfo.GetParameters();
				methodInfo.Invoke(instance, parameters.Select(parameter => GetValue(parameter.ParameterType)).ToArray());
			}
			else if (target is PropertyInfo)
			{
				PropertyInfo propertyInfo = (PropertyInfo) target;
				propertyInfo.SetValue(instance, GetValue(propertyInfo.PropertyType), null);
			}
			else if (target is FieldInfo)
			{
				FieldInfo fieldInfo = (FieldInfo) target;
				fieldInfo.SetValue(instance, GetValue(fieldInfo.FieldType));
			}
			else
				Logger.Exception("Unsupported member type for inject: " + target.GetType());
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

			Logger.Exception(valueType.Name + " is not supported.");
			return null;
		}
	}
}