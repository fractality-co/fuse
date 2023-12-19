using System;
using System.Collections;
using Fuse;
using UnityEngine;
using Environment = Fuse.Environment;

/// <summary>
/// This class is a very basic example of a <see cref="Module"/>.
/// It will load the default <see cref="Content"/> model, which stores properties.
/// It will then extract the delay time and event name via a dynamic bundle, then publishes the event.
/// </summary>
public class Example : Module
{
    // this will be resolved when this module becomes active by the system
    [Inject] private Content _content; // from default content bundle
    [Inject] private Environment _env; // active assigned in configuration

    private int _counter;

    [Coroutine(Lifecycle = Lifecycle.Setup)]
    private IEnumerator Setup()
    {
        Debug.Log("Example coroutine blocking [Setup] ...");
        yield return new WaitForSeconds(5f);
        Debug.Log("Example coroutine complete [Setup]!");
    }

    [Invoke]
    private void Active()
    {
        Events.Publish("event.id", EventArgs.Empty);

        var dynamicMessage = _content.GetValue("example.message"); 
        Debug.Log(dynamicMessage);
    }

    [Subscribe("event.id")]
    private void OnEventId(EventArgs args)
    {
        _counter++;
        Debug.Log($"Example received 'event.id' event with args [{args}] #{_counter}");
    }
}