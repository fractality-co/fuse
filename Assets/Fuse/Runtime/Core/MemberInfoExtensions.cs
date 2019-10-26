using System;
using System.Linq;
using System.Reflection;

namespace Fuse.Core
{
	public static class MemberInfoExtensions
	{
		public static object GetValue(this MemberInfo target, object instance)
		{
			if (target is PropertyInfo)
			{
				PropertyInfo propertyInfo = (PropertyInfo) target;
				propertyInfo.GetValue(instance, null);
			}
			else if (target is FieldInfo)
			{
				FieldInfo fieldInfo = (FieldInfo) target;
				fieldInfo.GetValue(instance);
			}

			throw new NotSupportedException();
		}

		public static void SetValue(this MemberInfo target, object instance, object value)
		{
			if (target is MethodInfo)
			{
				MethodInfo methodInfo = (MethodInfo) target;
				ParameterInfo[] parameters = methodInfo.GetParameters();
				methodInfo.Invoke(instance, parameters.Select(parameter => value).ToArray());
			}
			else if (target is PropertyInfo)
			{
				PropertyInfo propertyInfo = (PropertyInfo) target;
				propertyInfo.SetValue(instance, value, null);
			}
			else if (target is FieldInfo)
			{
				FieldInfo fieldInfo = (FieldInfo) target;
				fieldInfo.SetValue(instance, value);
			}

			throw new NotSupportedException();
		}
	}
}