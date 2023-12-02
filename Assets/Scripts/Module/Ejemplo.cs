using System.Collections;
using Fuse;
using UnityEngine;
using Logger = Fuse.Logger;

public class Ejemplo : Module
{
    [Inject] private Example _example;
    
    [Coroutine]
    private IEnumerator Run()
    {
        while (IsActive)
        {
            Logger.Info("Para ejemplo ... " + _example);
            yield return new WaitForSeconds(30f);
        }
    }
}