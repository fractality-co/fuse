using System.IO;
using UnityEditor;

namespace Fuse.Editor
{
    /// <summary>
    /// Helper class for consolidating common Editor actions.
    /// </summary>
    public static class PathUtility
    {
        public const string KeyButton = "{key}";
        public static readonly string IconPath = BasePath + "/2D/Icons/{0}.png";
        public static readonly string ButtonPath = string.Format(IconPath, KeyButton + "Button");
        public static readonly string BarIconPath = string.Format(IconPath, "Bar");
        public static readonly string GrayscaleBarIconPath = string.Format(IconPath, "GrayscaleBar");
        public static readonly string MenuHeaderIconPath = string.Format(IconPath, "MenuHeader");

        public static readonly string RuntimeScriptsPath =
            BasePath + Constants.DefaultSeparator + "Scripts" + Constants.DefaultSeparator + "Runtime";

        public static readonly string EditorScriptsPath =
            BasePath + Constants.DefaultSeparator + "Scripts" + Constants.DefaultSeparator + "Editor";

        private static string _basePath;

        public static string BasePath
        {
            get
            {
                if (string.IsNullOrEmpty(_basePath))
                {
                    var guids = AssetDatabase.FindAssets(nameof(Fuse) + " t:Folder");
                    if (guids.Length > 0)
                        _basePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                }

                return _basePath;
            }
        }

        public static string SystemPath(string path)
        {
            return path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        }

        public static void PreparePath(string path)
        {
            string[] directories = path.Split('/');
            string parent = string.Empty;
            for (int i = 0; i <= directories.Length - 1; i++)
            {
                string next = parent == string.Empty
                    ? directories[i]
                    : parent + Path.DirectorySeparatorChar + directories[i];
                if (!Directory.Exists(next))
                    Directory.CreateDirectory(next);

                parent = next;
            }
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
                file.CopyTo(Path.Combine(destDirName, file.Name), true);

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subDirs in dirs)
                    DirectoryCopy(subDirs.FullName, Path.Combine(destDirName, subDirs.Name), true);
            }
        }

        public static string GetDefaultVisualizerPath()
        {
            return $"{BasePath}/Prefabs/Loader";
        }
    }
}