namespace Fuse.Editor
{
    public class ErrorPopup : Popup
    {
        protected override string Title => "ERROR";
        protected override string Subtitle => "Unexpected Issue";
        protected override string Body => Payload;
        protected override ActionLayout Action => ActionLayout.None;
    }
}