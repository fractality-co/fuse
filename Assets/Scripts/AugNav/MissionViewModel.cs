using AugNav.Events;
using Fuse;
using MVVM;
using UnityEngine;

namespace AugNav
{
    public class MissionViewModel : ViewModel<MissionViewController, MissionModel>
    {
        protected override void Setup()
        {
            Debug.Log($"Setup has view ({View}), model ({Model})");
        }

        protected override void Cleanup()
        {
            Debug.Log($"Cleanup has view ({View}), model ({Model})");
        }

        [Subscribe(typeof(CreateElementCommand))]
        private void OnCreateMission(CreateElementCommand evt)
        {
            if (evt.Process())
                Model.CreateElement(evt.Type, evt.Location, evt.Scale);
        }
    }
}