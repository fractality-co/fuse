/*
 * Copyright (2020) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System.Collections.Generic;
using UnityEngine;

namespace Fuse
{
    /// <summary>
    /// State machine implementation that scopes what logic and content should become active or inactive.
    /// </summary>
    [Document("Core",
        "Represents a state within the application state machine, which controls the references to content and modules. " +
        "Within a state defines what transitions there are, which listens to events to invoke and change state." +
        "\n\nThis is fully managed by Fuse and should require no direct management or loading.")]
    public class State : ScriptableObject
    {
        /// <summary>
        /// Defines the content to load before the state can become active.
        /// </summary>
        [AssetReference(typeof(Content), Constants.ContentAssetPath)]
        public string[] Content;

        /// <summary>
        /// Defines the modules to load and start up before the state can become active.
        /// </summary>
        [ModuleReference] public string[] Modules;

        /// <summary>
        /// Defines what scenes to load for this state.
        /// This only supports scenes within the <see cref="Fuse"/> system.
        /// </summary>
        [SceneReference] public string[] Scenes;

        /// <summary>
        /// Manages what the transitions are from this state and where they should go to.
        /// </summary>
        public List<Transition> Transitions;


        /// <summary>
        /// Persisted location of state in visual editor.
        /// </summary>
        public Rect Rect = new Rect(113, 250, 175, 50);

        /// <summary>
        /// Title of the state in visual editor.
        /// </summary>
        public string NodeTitle => this != null ? name : string.Empty;

        public Rect NodeRect
        {
            get => Rect;
            set => Rect = value;
        }

        public List<Transition> NodeTransitions
        {
            get => Transitions;
            set => Transitions = value;
        }
    }
}