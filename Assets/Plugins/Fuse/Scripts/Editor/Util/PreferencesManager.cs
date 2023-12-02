using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    public static class PreferencesManager
    {
        [MenuItem("FUSE/Preferences/Clear")]
        public static void ClearPreferences()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}