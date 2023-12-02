using System;

namespace Fuse.Editor
{
    /// <summary>
    /// Sub-menu for managing <see cref="Environment"/>s.
    /// </summary>
    public class EnvironmentMenu : AssetCatalogMenu<Environment>
    {
        protected override Type RequiredAttribute => null;
        protected override string BasePath => Constants.EnvironmentsAssetPath;
        protected override string Subtitle => "manage application settings";
        protected override string Description =>
            "<i>Environments manage configurations for the application. This includes how to load assets, run on device and options for builds.</i>";
        protected override string AssetExtension => "asset";
        protected override bool NestByType => false;
    }
}