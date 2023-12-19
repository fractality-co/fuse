/*
 * Copyright (2020) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System.Collections;
using UnityEngine;

namespace Fuse
{
	/// <summary>
	/// Orthographic system logic that will be started when assigned to a <see cref="State"/> or globally in <see cref="Configuration"/>.
	/// The framework will handle all <see cref="Lifecycle"/> automatically for you, read documentation to fully leverage lifecycle.
	/// You may extend the IOC logic layer via creating attributes leveraging <see cref="IFusible"/> (and it's sub-interfaces).
	/// </summary>
	[Document("Module",
		"Orthographic system logic that will be started when assigned to a state or globally in the configuration. " +
		"Fuse will handle all Lifecycles for you automatically, but you may plug in your own custom logic as needed." +
		"\n\nYou may extend the IOC (inversion of control) logic layer via creating attributes leveraging the IFusible* interfaces.")]
	public class Module : ScriptableObject
	{
		/// <summary>
		/// Check if the module is still actively available to execute.
		/// </summary>
		public bool IsActive => this != null;

		private static Scheduler Scheduler => Scheduler.Get();

		protected IEnumerator Run(IEnumerator routine) { yield return Scheduler.StartCoroutine(routine); }
		protected Coroutine StartRun(IEnumerator routine) { return Scheduler.StartCoroutine(routine); }
		protected void StopRun(Coroutine routine) { Scheduler.StopCoroutine(routine); }
		protected void StopRun(IEnumerator routine) { Scheduler.StopCoroutine(routine); }
	}
}