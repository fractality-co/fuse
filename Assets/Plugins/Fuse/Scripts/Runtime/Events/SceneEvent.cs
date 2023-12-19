using System;

namespace Fuse
{
	public class SceneEvent : EventArgs
	{
		public enum Event
		{
			Load,
			Unload
		}

		public const string Id = "fuse.event.scene";

		public readonly string sceneName;
		public readonly Event evt;

		public SceneEvent(string sceneName, Event evt)
		{
			this.sceneName = sceneName;
			this.evt = evt;
		}
	}
}