using System;
using UnityEditor;

namespace Fuse.Editor
{
	public static class BuildTargetGroupExtensions
	{
		private const string Delimiter = ";";

		public static bool HasScriptingDefine(this BuildTargetGroup buildTargetGroup, string define)
		{
			return PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Contains(define);
		}

		public static bool AddScriptingDefine(this BuildTargetGroup buildTargetGroup, string define)
		{
			try
			{
				// if we already have this define, don't add it again
				string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
				if (defines.Contains(define))
					return true;

				string delimit = string.IsNullOrEmpty(defines) ? string.Empty : Delimiter;
				PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines + delimit + define);

				return true;
			}
			catch (Exception)
			{
				// Unity throws a message that we are fine with, we still try to alter ALL platforms
				return false;
			}
		}

		public static bool RemoveScriptingDefine(this BuildTargetGroup buildTargetGroup, string define)
		{
			try
			{
				// find index of define within defines, if there is none then stop
				string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
				int indexOf = defines.IndexOf(define, StringComparison.Ordinal);
				if (indexOf == -1)
					return true;

				// if it is in the beginning, remove it alone, else with delimiter
				string toRemove = indexOf == 0 ? define : Delimiter + define;
				defines = defines.Replace(toRemove, string.Empty);
				PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);

				return true;
			}
			catch (Exception)
			{
				// Unity throws a message that we are fine with, we still try to alter ALL platforms
				return false;
			}
		}
	}
}