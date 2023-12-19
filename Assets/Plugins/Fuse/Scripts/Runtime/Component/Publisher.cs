/* Copyright (2021) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Fuse
{
	/// <summary>
	/// Simple publisher component that publishes events over FUSE's <see cref="Events"/>.
	/// </summary>
	[Document("Events",
		"Simple implementation of a component that publishes out events over the Relay." +
		"\n\nAttach this to a GameObject in your scene or prefab, put in your event then simply call publish!")]
	public class Publisher : MonoBehaviour
	{
		[FormerlySerializedAs("event")] public string eventId = "event.id";
		[Range(0, 10)] public float delay;

		public void DelayedPublish(string evt)
		{
			eventId = evt;
			DelayedPublish();
		}

		public void DelayedPublish() { StartCoroutine(DelayingPublish()); }

		private IEnumerator DelayingPublish()
		{
			yield return new WaitForSeconds(delay);
			Publish();
		}

		/// <summary>
		/// Publish the serialized event.
		/// </summary>
		public void Publish()
		{
			if (!string.IsNullOrEmpty(eventId))
				Events.Publish(eventId, EventArgs.Empty);
		}

		/// <summary>
		/// Publish an event.
		/// </summary>
		public void Publish(string evt)
		{
			if (!string.IsNullOrEmpty(evt))
				Events.Publish(evt, EventArgs.Empty);
		}

		/// <summary>
		/// Publish an event with custom arguments.
		/// </summary>
		public void Publish(string evt, EventArgs evtArgs)
		{
			if (!string.IsNullOrEmpty(evt))
				Events.Publish(evt, evtArgs);
		}
	}
}