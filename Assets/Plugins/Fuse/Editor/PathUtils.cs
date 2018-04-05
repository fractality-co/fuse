using System.IO;

namespace Fuse.Editor
{
	/// <summary>
	/// Helper class for condolidating common Editor actions.
	/// </summary>
	public static class PathUtils
	{
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
				string next = parent == string.Empty ? directories[i] : parent + Path.DirectorySeparatorChar + directories[i];
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
			{
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			DirectoryInfo[] dirs = dir.GetDirectories();
			// If the destination directory doesn't exist, create it.
			if (!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}

			// Get the files in the directory and copy them to the new location.
			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files)
			{
				string temppath = Path.Combine(destDirName, file.Name);
				file.CopyTo(temppath, true);
			}

			// If copying subdirectories, copy them and their contents to new location.
			if (copySubDirs)
			{
				foreach (DirectoryInfo subdir in dirs)
				{
					string temppath = Path.Combine(destDirName, subdir.Name);
					DirectoryCopy(subdir.FullName, temppath, true);
				}
			}
		}
	}
}