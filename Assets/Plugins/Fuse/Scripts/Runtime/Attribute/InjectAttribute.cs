using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fuse
{
    /// <summary>
    /// Assigns a dependency based on its type.
    /// By default, this executes on the Setup <see cref="Lifecycle" />.
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    [DefaultLifecycle(Lifecycle.Setup)]
    [Document("Module",
        "Resolves nearly any object, asset or module within Unity, as long as it is already loaded within a scene or Fuse. " +
        "Fuse will resolve it during the Setup lifecycle, so ensure that you are accessing it during Active lifecycle." +
        "\n\n[Inject] private ExampleModule _example" + 
        "\n[Inject] private Button _buttonInScene" +
        "\n[Inject(\"name\")] private Content _content")]
    public sealed class InjectAttribute : Attribute, IFusibleInvoke
    {
        private readonly Regex _regex;
        private readonly bool _root;

        /// <summary>
        /// Assigns a dependency based on its type.
        /// By default, this executes on the Setup <see cref="Lifecycle" />.
        /// </summary>
        public InjectAttribute(string filter = ".")
        {
            if (filter == ".")
                _root = true;
            _regex = new Regex(filter);
        }

        public uint Order { get; [UsedImplicitly] set; }

        public Lifecycle Lifecycle { get; [UsedImplicitly] set; }

        public void Invoke(MemberInfo target, object instance)
        {
            switch (target)
            {
                case MethodInfo methodInfo:
                {
                    var parameters = methodInfo.GetParameters();
                    methodInfo.Invoke(instance,
                        parameters.Select(parameter => GetValue(parameter.ParameterType)).ToArray());
                    break;
                }
                case PropertyInfo propertyInfo:
                {
                    propertyInfo.SetValue(instance, GetValue(propertyInfo.PropertyType), null);
                    break;
                }
                case FieldInfo fieldInfo:
                {
                    fieldInfo.SetValue(instance, GetValue(fieldInfo.FieldType));
                    break;
                }
                default:
                    Logger.Exception("Unsupported member type for inject: " + target.GetType());
                    break;
            }
        }

        private object GetValue(Type valueType)
        {
            if (typeof(IList).IsAssignableFrom(valueType))
                try
                {
                    var value = (IList) Convert.ChangeType(Activator.CreateInstance(valueType), valueType);
                    if (value == null)
                        return null;

                    foreach (var obj in FindValue(HeuristicallyDetermineType(value)))
                        if (_regex.IsMatch(obj.name))
                            value.Add(obj);

                    return value;
                }
                catch (Exception)
                {
                    // ignored
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

        private Object[] FindValue(Type type)
        {
            if (type == typeof(Environment))
                return FetchEnvironment();

            if (type == typeof(Content) && _root)
            {
                var found = Resources.FindObjectsOfTypeAll(type);
                foreach (var foundContent in found)
                {
                    if (foundContent.name == Content.DefaultName)
                        return new[] { foundContent };
                }
            }

            if (type.IsSubclassOf(typeof(ScriptableObject)))
                return Resources.FindObjectsOfTypeAll(type);

            return Object.FindObjectsOfType(type);
        }

        /// <summary>
        /// At run-time, we want to filter access to <see cref="Environment"/>s by what is assigned in configuration.
        /// This makes `[Inject] private Environment _env;` always return the one chosen in Configuration via pipeline.
        /// </summary>
        private static Object[] FetchEnvironment()
        {
            var configuration = Resources.FindObjectsOfTypeAll<Configuration>();
            if (configuration.Length > 0)
            {
                var environmentName = Path.GetFileNameWithoutExtension(configuration[0].Environment);
                foreach (var environment in Resources.FindObjectsOfTypeAll<Environment>())
                {
                    if (environment.name == environmentName)
                        return new Object[] {environment};
                }
            }

            return new Object[] { };
        }
    }
}