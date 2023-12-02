using System;
using Fuse;

public class InstantForward : Module
{
    [Invoke]
    private void Start()
    {
        Relay.Publish("event.id");
    }
}
