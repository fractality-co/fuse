using System;
using UnityEngine;

namespace Fuse
{
	public abstract class FusableBehavior : MonoBehaviour
	{
		private void Awake()
		{
			Events.Publish(new LifecycleArgs(this, LifecycleArgs.Step.OnSetup));
			Setup();
		}

		private void OnDestroy()
		{
			Events.Publish(new LifecycleArgs(this, LifecycleArgs.Step.OnCleanup));
			Cleanup();
		}

		protected abstract void Setup();
		protected abstract void Cleanup();

		public class LifecycleArgs : EventArgs
		{
			public enum Step
			{
				OnSetup,
				OnCleanup
			}

			public readonly FusableBehavior behaviour;
			public readonly Step lifecycle;

			public LifecycleArgs(FusableBehavior behaviour, Step lifecycle)
			{
				this.behaviour = behaviour;
				this.lifecycle = lifecycle;
			}
		}
	}
}