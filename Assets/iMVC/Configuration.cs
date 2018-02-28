using System;
using System.Collections.Generic;
using UnityEngine;

namespace iMVC
{
    public class Configuration : ScriptableObject
    {
        [Header("Asset Management")] 
        [SerializeField] private AssetLoadMethod _coreLoadMethod = AssetLoadMethod.Resources;
        
        [Header("State Machine")]
        [SerializeField] private string _start;
        [SerializeField] private List<State> _states;
        
        [Serializable]
        private class State
        {
            public string Name;
        }

        private enum AssetLoadMethod
        {
            Resources
        }
    }
}