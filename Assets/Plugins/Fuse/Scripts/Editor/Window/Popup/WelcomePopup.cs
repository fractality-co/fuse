using UnityEngine;

namespace Fuse.Editor
{
    /// <summary>
    /// Initial welcome window that shows upon installation for Fuse.
    /// </summary>
    public class WelcomePopup : Popup
    {
        protected override string Title => "WELCOME";
        protected override string Subtitle => string.Empty;

        protected override string Body =>
            "The framework has been installed into the project. To begin, simply open up and run the scene @ Assets/Scenes/Fuse. " +
            "In addition, we have added this scene as the first entry into build settings so you can make functional builds.\n\n" +
            "All framework management is within one window, located at FUSE/Manage. " +
            "This window is central to managing your application with resources for support.\n\n" +
            "Please reference the PDF documentation for more details.";

        protected override ActionLayout Action => ActionLayout.None;

        public WelcomePopup() : base(new Vector2(500, 400), 20)
        {
        }
    }
}