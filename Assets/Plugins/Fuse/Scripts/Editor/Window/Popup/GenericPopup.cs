namespace Fuse.Editor
{
    /// <summary>
    /// Generic pop-up to fully control content.
    /// </summary>
    public class GenericPopup : Popup
    {
        private const char Separator = ';';

        protected override string Title => Payload.Split(Separator)[0];
        protected override string Subtitle => Payload.Split(Separator)[1];

        protected override string Body => Payload.Split(Separator)[2];
        protected override ActionLayout Action => ActionLayout.None;

        public GenericPopup() : base(350)
        {
        }

        public static string Build(string title, string subtitle, string body)
        {
            return title + Separator + subtitle + Separator + body;
        }
    }
}