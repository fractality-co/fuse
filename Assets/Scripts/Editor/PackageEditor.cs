using System.IO;
using Fuse.Editor;
using UnityEditor;

namespace Fuse.Package
{
	public static class PackageEditor
	{
		private const string PackageFilename = "FUSE-{0}.unitypackage";

		[MenuItem("FUSE/Staging/Increase Version", priority = 1000)]
		public static void IncreaseVersion() { License.IncreaseVersion(); }

		[MenuItem("FUSE/Staging/Export Package", priority = -1000)]
		private static void ExportPackage()
		{
			var licenseName = string.Format(PackageFilename, License.FetchVersion());
			var outputPath = "Builds" + Path.DirectorySeparatorChar + licenseName;

			AssetDatabase.ExportPackage(PathUtility.BasePath, licenseName, ExportPackageOptions.Recurse);

			if (!Directory.Exists("Builds"))
				Directory.CreateDirectory("Builds");

			if (File.Exists(outputPath))
				File.Delete(outputPath);

			File.Move(licenseName, outputPath);
			Logger.Info($"Exported package [{licenseName}]");
			EditorUtility.DisplayDialog("Export Complete", "FUSE has been exported to Builds: '" + licenseName + "'.", "Ok");
		}
	}
}