/*
 * Copyright (2020) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System;
using Object = UnityEngine.Object;

namespace Fuse
{
    /// <summary>
    /// Simple serializable string key:value object.
    /// </summary>
    [Serializable]
    public class StringProperty
    {
        public string key;
        public string value;
    }

    [Serializable]
    public class ObjectProperty
    {
        public string key;
        public Object value;
    }
}