using AugNav.Data;
using MVVM;
using UnityEngine;

namespace AugNav.Events
{
    public class CreateElementCommand : Command
    {
        public readonly MissionElement.Type Type;
        public readonly GeoReference Location;
        public readonly Vector2 Scale;

        public CreateElementCommand(MissionElement.Type type, GeoReference location, Vector2 scale)
        {
            Type = type;
            Location = location;
            Scale = scale;
        }
    }
}