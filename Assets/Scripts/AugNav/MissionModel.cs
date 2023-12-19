using System;
using AugNav.Data;
using MVVM;
using UnityEngine;

public class MissionModel : Model
{
    private const string InstanceKey = "mission.instance";

    public MissionInstance Instance { get; private set; }

    public override bool IsReady => Instance != null;

    protected override void Setup()
    {
        Instance = PlayerPrefs.HasKey(InstanceKey) ? LoadPersisted(InstanceKey) : new MissionInstance();
    }

    protected override void Cleanup()
    {
        SavePersisted(InstanceKey, Instance);
    }
    
    public void CreateElement(MissionElement.Type type, GeoReference location, Vector2 scale)
    {
        var missionElement = new MissionElement(type, location, scale);
        Instance.elements.Add(missionElement);
        Notify(new UpdateEvent(Instance));
    }

    public void RemoveElement(string uid)
    {
        var missionElement = Instance.elements.Find((element) => element.uid == uid);
        if(missionElement != null && Instance.elements.Remove(missionElement))
            Notify(new UpdateEvent(Instance));
    }

    private static MissionInstance LoadPersisted(string key)
    {
        return JsonUtility.FromJson<MissionInstance>(PlayerPrefs.GetString(key));
    }

    private static void SavePersisted(string key, MissionInstance instance)
    {
        PlayerPrefs.SetString(key, JsonUtility.ToJson(instance));
    }

    public class UpdateEvent : ModelEvent
    {
        public readonly MissionInstance Instance;

        public UpdateEvent(MissionInstance instance)
        {
            Instance = instance;
        }
    }
}