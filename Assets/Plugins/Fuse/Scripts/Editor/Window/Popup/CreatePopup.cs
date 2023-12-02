using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    /// <summary>
    /// Popup to confirm create of an object.
    /// </summary>
    public class CreatePopup : Popup
    {
        protected override string Title => "CREATE";
        protected override string Subtitle => Category.ToUpper();
        protected override string Body => $"Create the <b>{Category}</b> named: \"{Input}\"?";
        protected override ActionLayout Action => ActionLayout.Input | ActionLayout.Confirmation;
        private string Category => Type.IsSubclassOf(typeof(Module)) ? "Module" : Type.Name.Replace("Asset", string.Empty);

        protected override void OnAccept()
        {
            if (Type == typeof(Module))
                ScriptProcessor.CreateModule(Input, new List<Type> {typeof(InvokeAttribute)});
            else if (Type == typeof(Content))
                AssetProcessor.CreateContent(Input);
            else if (Type.IsSubclassOf(typeof(ScriptableObject)))
            {
                var created = AssetProcessor.CreateAsset(Type, Payload, Input);
                var state = created as State;
                if (state != null)
                {
                    var rect = state.NodeRect;
                    rect.position = StateMenu.LastCursor - new Vector2(rect.width / 2, rect.height / 2);
                    state.NodeRect = rect;
                }
            }
            else if (Type == typeof(SceneAsset))
                AssetProcessor.CreateScene(Input);
        }
    }
}