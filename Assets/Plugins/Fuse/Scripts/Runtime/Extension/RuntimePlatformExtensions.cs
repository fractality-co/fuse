/* Copyright (2021) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fuse
{
	/// <summary>
	/// Extensions for <see cref="RuntimePlatform"/>.
	/// </summary>
	public static class RuntimePlatformExtensions
	{
		public static string GetBuildPlatformName(this RuntimePlatform platform)
		{
			return Strip(platform.ToString(), new[] { "Player", "Editor", "X86", "X64", "ARM" }).ToLower();
		}
	
		public static string GetPlatformName(this RuntimePlatform platform)
		{
			return Application.isEditor ? "editor" : Strip(platform.ToString(), new[] { "Player", "Editor", "X86", "X64", "ARM" }).ToLower();
		}

		private static string Strip(string value, IEnumerable<string> toStrip) { return toStrip.Aggregate(value, (current, strip) => current.Replace(strip, string.Empty)); }
	}
}