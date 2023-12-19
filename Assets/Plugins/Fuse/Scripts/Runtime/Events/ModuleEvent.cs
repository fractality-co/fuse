using System;

namespace Fuse
{
	public class ModuleEvent : EventArgs
	{
		public const string Id = "fuse.event.module";

		public readonly Module instance;
		public readonly Lifecycle lifecycle;

		public ModuleEvent(Module instance, Lifecycle lifecycle)
		{
			this.instance = instance;
			this.lifecycle = lifecycle;
		}
	}
}