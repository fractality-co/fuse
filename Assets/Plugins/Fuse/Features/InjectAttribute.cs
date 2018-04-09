using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEngine;
using Logger = Fuse.Core.Logger;
using Object = UnityEngine.Object;

namespace Fuse.Feature
{
	/// <summary>
	/// Assigns a dependency based on its type.
	/// By default, this executes on the Load <see cref="Lifecycle"/>.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
	[DefaultLifecycle(Lifecycle.Load)]
	public sealed class InjectAttribute : Attribute, IFuseExecutor
	{
		public uint Order
		{
			get;
			[UsedImplicitly]
			set;
		}

		public Lifecycle Lifecycle
		{
			get;
			[UsedImplicitly]
			set;
		}

		private readonly Regex _regex;

		public InjectAttribute(string regex = ".")
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
			if (typeof(IList).IsAssignableFrom(valueType))
			{
				try
				{
					IList value = (IList) Convert.ChangeType(Activator.CreateInstance(valueType), valueType);

					if (value == null)
						return null;

					foreach (Object obj in FindValue(HeuristicallyDetermineType(value)))
					{
						if (_regex.IsMatch(obj.name))
							value.Add(obj);
					}

					return value;
				}
				catch (Exception)
				{
					// ignored
				}
			}

			if (!typeof(IEnumerable).IsAssignableFrom(valueType))
				return FindValue(valueType).FirstOrDefault(obj => _regex.IsMatch(obj.name));

			Logger.Exception(valueType.Name + " is not supported.");
			return null;
		}

		private static Type HeuristicallyDetermineType(IList myList)
		{
			var enumerableType =
				myList.GetType()
					.GetInterfaces()
					.Where(i => i.IsGenericType && i.GetGenericArguments().Length > 0)
					.FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

			if (enumerableType != null)
				return enumerableType.GetGenericArguments()[0];

			return myList.Count == 0 ? null : myList[0].GetType();
		}

		private static Object[] FindValue(Type type)
		{
			if (type == typeof(ScriptableObject))
				return Resources.FindObjectsOfTypeAll(type);

			return Object.FindObjectsOfType(type);
		}
	}
}