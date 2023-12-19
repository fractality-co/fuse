/* Copyright (2021) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using UnityEngine;

namespace Fuse
{
	public class SceneReference : PropertyAttribute
	{
		public bool InBundles;

		public SceneReference(bool inBundles = true) { InBundles = inBundles; }
	}
}