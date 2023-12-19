/*
 * Copyright (2020) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fuse
{
	/// <summary>
	/// Internal reference to module and manages it's <see cref="Lifecycle"/>.
	/// </summary>
	[UsedImplicitly]
	public class ModuleInstance : Instance
	{
		private readonly Type _type;
		private readonly string _id;
		private Lifecycle _lifecycle;
		private Module _instance;

		public ModuleInstance(string id, Environment environment) : base(id, environment)
		{
			var idList = id.Split(Constants.DefaultSeparator);
			_type = FindType(idList[idList.Length - 1]);
			_id = id;
		}

		protected override IEnumerator Load()
		{
			if (_type == null)
			{
				Logger.Error("Unable to instantiate module because of unknown type with id: " + _id);
				yield break;
			}

			_instance = ScriptableObject.CreateInstance(_type) as Module;
			if (_instance == null)
			{
				Logger.Error("Unable to instantiate module because of unknown module with type: " + _type.Name);
				yield break;
			}

			_instance.name = _type.Name;
			yield return SetLifecycle(Lifecycle.Setup);
			Events.Publish(ModuleEvent.Id, new ModuleEvent(_instance, Lifecycle.Setup));

			yield return SetLifecycle(Lifecycle.Active);
			Events.Publish(ModuleEvent.Id, new ModuleEvent(_instance, Lifecycle.Active));

			Logger.Info($"Module started ({_type.Name})");
		}

		protected override IEnumerator Unload()
		{
			Events.Publish(ModuleEvent.Id, new ModuleEvent(_instance, Lifecycle.Cleanup));
			yield return SetLifecycle(Lifecycle.Cleanup);
			Object.Destroy(_instance);
			_instance = null;
			Logger.Info($"Module stopped ({_type.Name})");
		}

		private IEnumerator SetLifecycle(Lifecycle lifecycle)
		{
			yield return ProcessInjections(lifecycle, _lifecycle);
			_lifecycle = lifecycle;
		}

		private IEnumerator ProcessInjections(Lifecycle toEnter, Lifecycle toExit)
		{
			if (_instance == null)
				yield break;

			var attributes = new List<Tuple<IFusible, MemberInfo>>();
			foreach (var member in GetMembers())
			{
				foreach (var custom in member.GetCustomAttributes(true))
				{
					if (custom is IFusible attribute)
						attributes.Add(new Tuple<IFusible, MemberInfo>(attribute, member));
				}
			}

			attributes.Sort((a, b) => a.Item1.Order < b.Item1.Order ? -1 : 1);

			foreach (var attribute in attributes)
			{
				var active = attribute.Item1.Lifecycle;
				if (active == Lifecycle.None)
				{
					var possibleDefaults = attribute.Item1.GetType()
						.GetCustomAttributes(typeof(DefaultLifecycleAttribute), true);
					if (possibleDefaults.Length > 0)
					{
						var defaultLifecycle =
							(DefaultLifecycleAttribute)possibleDefaults[0];
						active = defaultLifecycle.Lifecycle;
					}
					else
					{
						active = (Lifecycle)((DefaultValueAttribute)active.GetType().GetCustomAttributes(true)[0])
							.Value;
					}
				}

				if (toEnter == active)
				{
					if (attribute.Item1 is IFusibleInvoke executor)
						executor.Invoke(attribute.Item2, _instance);

					if (attribute.Item1 is IFusibleCoroutine executorCoroutine)
						yield return executorCoroutine.Invoke(attribute.Item2, _instance);
				}

				if (toEnter == active || toExit == active)
				{
					if (attribute.Item1 is IFusibleLifecycle lifecycle)
					{
						if (toEnter == active)
							lifecycle.OnEnter(attribute.Item2, _instance);
						else if (toExit == active)
							lifecycle.OnExit(attribute.Item2, _instance);
					}
				}
			}
		}

		private IEnumerable<MemberInfo> GetMembers()
		{
			return _type.FindMembers(MemberTypes.All,
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, null);
		}

		private static Type FindType(string qualifiedTypeName)
		{
			var t = Type.GetType(qualifiedTypeName, false, true);
			if (t != null)
				return t;

			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				t = asm.GetType(qualifiedTypeName);
				if (t != null)
					return t;
			}

			return null;
		}
	}
}