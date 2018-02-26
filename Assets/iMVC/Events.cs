using System;
using JetBrains.Annotations;

namespace iMVC.Events
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
	/// Invokes a method per update from engine.
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class UpdateAttribute : Attribute
	{
		public readonly bool IsFixed;

		public UpdateAttribute()
		{
			IsFixed = false;
		}

		public UpdateAttribute(bool isFixed)
		{
			IsFixed = isFixed;
		}
	}
		
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Event | AttributeTargets.Method)]
	public sealed class PublishAttribute : PubSubAttribute
	{
		public PublishAttribute(string type) : base(type)
		{
			
		}
	}
		
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class SubscribeAttribute : PubSubAttribute
	{
		public SubscribeAttribute(string type) : base(type)
		{
		}
	}

	public abstract class PubSubAttribute : Attribute
	{
		public readonly string Type;

		protected PubSubAttribute(string type)
		{
			Type = type;
		}
	}
}