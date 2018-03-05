using System;
using JetBrains.Annotations;

namespace iMVC
{
	/// <summary>
	/// Invokes a method based off a timed interval passed (millseconds).
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class TickAttribute : Attribute
	{
		public readonly TimeSpan Interval;

		public TickAttribute(double milliseconds)
		{
			Interval = TimeSpan.FromMilliseconds(milliseconds);
		}
	}

	/// <summary>
	/// When input button matches (exact match) the assigned method will be invoked.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class InputAttribute : Attribute
	{
		public readonly string Button;

		/// <summary>
		/// When input button matches (exact match) the assigned method will be invoked.
		/// </summary>
		/// <param name="button"></param>
		public InputAttribute(string button)
		{
			Button = button;
		}
	}

	/// <summary>
	/// Adds hook to an <code>event</code> defined, that will call <see cref="SubscribeAttribute"/> with corresponding type."/>
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Event)]
	public sealed class PublishAttribute : Attribute
	{
		public readonly string Type;

		public PublishAttribute(string type)
		{
			Type = type;
		}
	}

	/// <summary>
	/// When events attached to <see cref="PublishAttribute"/> are invoked, and the type specified here matches it will be invoked.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class SubscribeAttribute : Attribute
	{
		public readonly string Type;

		public SubscribeAttribute(string type)
		{
			Type = type;
		}
	}
}