using System;

namespace Fuse
{
	public class ContentEvent : EventArgs
	{
		public enum Event
		{
			Load,
			Unload
		}

		public const string Id = "fuse.event.content";

		public readonly string bundleName;
		public readonly Event evt;

		public ContentEvent(string bundleName, Event evt)
		{
			this.bundleName = bundleName;
			this.evt = evt;
		}
	}
}