using MVVM;
using UnityEngine;

namespace AugNav.Events
{
    public class ResizeElementCommand : Command
    {
        public Vector2 Scale;

        public ResizeElementCommand(Vector2 scale)
        {
            Scale = scale;
        }
    }
}