/*
 * Copyright (2020) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System;
using UnityEngine;

namespace Fuse
{
    /// <summary>
    /// Allows a <see cref="string"/> field can become a weakly referenced path to an asset.
    /// </summary>
    public sealed class AssetReference : PropertyAttribute
    {
        public readonly Type RequiredAttribute;
        public readonly string RequiredSubpath;
        public readonly Type Type;

        public AssetReference(Type type)
        {
            Type = type;
        }

        public AssetReference(Type type, Type requiredAttribute)
        {
            Type = type;
            RequiredAttribute = requiredAttribute;
        }

        public AssetReference(Type type, string requiredSubpath)
        {
            Type = type;
            RequiredSubpath = requiredSubpath;
        }
    }
}