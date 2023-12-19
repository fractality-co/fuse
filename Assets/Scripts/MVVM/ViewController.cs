using Fuse;
using UnityEngine;

namespace MVVM
{
    public abstract class ViewController : MonoBehaviour
    {
        public abstract void Setup();
        public abstract void Cleanup();

        public void Notify(Command command)
        {
            Events.Publish(command);
        }
    }
}