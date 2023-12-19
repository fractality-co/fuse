using MVVM;

namespace AugNav.Events
{
    public class DeleteElementCommand : Command
    {
        public readonly string Uid;

        public DeleteElementCommand(string uid)
        {
            Uid = uid;
        }
    }
}