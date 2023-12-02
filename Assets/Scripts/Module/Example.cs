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
    [Inject("Default")] private Content _content; // from default content bundle
    [Inject] private Environment _env; // active assigned in configuration
    [Inject("Cube")] private MeshRenderer _cube; // component in the world scene
    [Inject] private Ejemplo _ejemplo; // global module

    private int _counter;

    [Invoke(Lifecycle = Lifecycle.Setup)]
    private void SetupInvoke()
    {
        Debug.Log($"Example invoke [lifecycle={Lifecycle.Setup}] [env={_env}] [content={_content}] [cube={_cube}] [ejemplo={_ejemplo}]");
    }

    [Invoke]
    private void ActiveInvoke()
    {
        Debug.Log($"Example invoke [lifecycle={Lifecycle.Active}] [env={_env}] [content={_content}] [cube={_cube}] [ejemplo={_ejemplo}]");
    }

    [Coroutine(Lifecycle = Lifecycle.Setup)]
    private IEnumerator Setup()
    {
        Debug.Log("Example coroutine blocking [Setup] ...");
        yield return new WaitForSeconds(5f);
        Debug.Log("Example coroutine blocking [Setup]!");
    }

    // when the module becomes active, the method with this attribute is invoked
    [Coroutine]
    private IEnumerator Active()
    {
        Debug.Log("Example coroutine [Active] ...");
        if (_content == null)
            Debug.LogWarning("Content not available ...");
        yield return new WaitForSeconds(5f);

        Relay.Publish("event.id", EventArgs.Empty);
        Debug.Log("Example coroutine [Active]!");
    }

    [Invoke(Lifecycle = Lifecycle.Cleanup)]
    private void CleanupInvoke()
    {
        Debug.Log($"Example invoke [lifecycle={Lifecycle.Cleanup}] [env={_env}] [content={_content}] [cube={_cube}] [ejemplo={_ejemplo}]");
    }

    [Thread]
    private void Threaded()
    {
        Debug.Log("Example thread ...");
    }

    // publish out the custom event, in this case a state transition will be triggered
    // all modules, components or state listening will trigger subscriptions 
    [Subscribe("event.id")]
    private void OnEventId(EventArgs args)
    {
        _counter++;
        Debug.Log($"Example received 'event.id' event with args [{args}] #{_counter}");
    }
}