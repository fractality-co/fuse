using System;

namespace Fuse.Editor
{
    [Serializable]
    public class Package
    {
        public string name;
        public string version;
        public string license;
        public string displayName;
        public string description;
        public string unity;
        public string[] keywords;
        public Author author;

        public Package IncreaseVersion()
        {
            var versions = version.Split('.');
            var originalVersion = versions[versions.Length - 1];
            var newVersion = int.Parse(originalVersion) + 1;
            version = version.Replace(originalVersion, newVersion.ToString());
            return this;
        }
    }
}