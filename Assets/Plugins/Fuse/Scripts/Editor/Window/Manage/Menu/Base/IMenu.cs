using System;
using UnityEngine;

namespace Fuse.Editor
{
    /// <summary>
    /// Abstraction to create a sub-menu within the <see cref="FuseEditorWindow"/>.
    /// </summary>
    public interface IMenu
    {
        Action Refresh { get; set; }

        void Setup();
        void Draw(Rect window);
        void Cleanup();
    }
}