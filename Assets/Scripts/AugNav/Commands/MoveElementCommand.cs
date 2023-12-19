using AugNav.Data;
using MVVM;

namespace AugNav.Events
{
    public class MoveElementCommand : Command
    {
        public GeoReference Location;

        public MoveElementCommand(GeoReference location)
        {
            Location = location;
        }
    }
}