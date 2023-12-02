using System;
using System.Linq;
using System.Reflection;

namespace Fuse
{
    /// <summary>
    /// Extension for helping make interaction with reflection easier.
    /// </summary>
    public static class MemberInfoExtensions
    {
        public static object GetValue(this MemberInfo target, object instance)
        {
            if (target is PropertyInfo)
            {
                var propertyInfo = (PropertyInfo) target;
                return propertyInfo.GetValue(instance, null);
            }

            if (target is FieldInfo)
            {
                var fieldInfo = (FieldInfo) target;
                return fieldInfo.GetValue(instance);
            }

            throw new NotSupportedException();
        }

        public static void SetValue(this MemberInfo target, object instance, object value)
        {
            if (target is MethodInfo)
            {
                var methodInfo = (MethodInfo) target;
                var parameters = methodInfo.GetParameters();
                methodInfo.Invoke(instance, parameters.Select(parameter => value).ToArray());
            }
            else if (target is PropertyInfo)
            {
                var propertyInfo = (PropertyInfo) target;
                propertyInfo.SetValue(instance, value, null);
            }
            else if (target is FieldInfo)
            {
                var fieldInfo = (FieldInfo) target;
                fieldInfo.SetValue(instance, value);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}