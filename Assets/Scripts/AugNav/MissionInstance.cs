using System;
using System.Collections.Generic;
using AugNav.Data;
using UnityEngine;

[Serializable]
public class MissionInstance
{
    public string uid = Guid.NewGuid().ToString();
    public List<MissionElement> elements;
}

[Serializable]
public class MissionElement
{
    public enum Type
    {
        Aoi,
        Racetrack,
        Waypoint
    }

    public string uid;
    public Type type;
    public GeoReference location;
    public Vector2 scale;

    public MissionElement(Type type, GeoReference location, Vector2 scale)
    {
        uid = Guid.NewGuid().ToString();
        this.type = type;
        this.scale = scale;
        this.location = location;
    }
}