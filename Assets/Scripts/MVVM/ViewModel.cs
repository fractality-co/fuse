using System.Collections;
using Fuse;
using UnityEngine;

namespace MVVM
{
    public abstract class ViewModel<T, TJ> : Module where T : ViewController where TJ : Model
    {
        [Inject] protected T View;
        [Inject] protected TJ Model;

        [Coroutine]
        protected IEnumerator Initialize()
        {
            yield return new WaitUntil(() => Model.IsReady);
            Setup();
            View.Setup();
        }

        [Invoke(Lifecycle = Lifecycle.Cleanup)]
        protected void Deinitialize()
        {
            Cleanup();
            View.Cleanup();
        }

        protected abstract void Setup();
        protected abstract void Cleanup();
    }
}