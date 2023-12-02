using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    public class CaptureManager
    {
        [MenuItem("FUSE/Screenshot")]
        public static void CapturePicture()
        {
            var now = DateTime.Now;
            var title = now.ToShortDateString() + " - " + now.ToLongTimeString();
            title = title.Replace(":", "-");
            title = title.Replace("/", "-");
            var filename = title + ".jpeg";
            var path = EditorUtility.OpenFolderPanel("Save Captured Screenshot", "", "");
            ScreenCapture.CaptureScreenshot(path + Path.DirectorySeparatorChar + filename);
            Logger.Info("Saved screenshot (" + filename + ") at path: " + path);
        }
    }
}