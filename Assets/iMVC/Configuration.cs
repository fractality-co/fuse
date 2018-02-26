using System;
using System.Collections.Generic;
using UnityEngine;

namespace iMVC
{
    public class Configuration : ScriptableObject
    {
        [Header("State Machine")]
        public string Start;
        public List<State> States;

        [Serializable]
        public class State
        {
            public string Name;
        }
    }
}