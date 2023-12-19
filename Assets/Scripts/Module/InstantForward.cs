using System;
using Fuse;

public class InstantForward : Module
{
    [Invoke]
    private void Start()
    {
        Events.Publish("event.id");
    }
}
