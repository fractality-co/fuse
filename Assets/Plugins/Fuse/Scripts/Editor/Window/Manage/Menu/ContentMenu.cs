using System;

namespace Fuse.Editor
{
    public class ContentMenu : AssetCatalogMenu<Content>
    {
        protected override Type RequiredAttribute => null;
        protected override bool NestByType => true;
        protected override string AssetExtension => "asset";
        protected override string BasePath => Constants.ContentAssetPath;
        protected override string Subtitle => "manage asset bundles";
        protected override string Description => "Manage the dynamic content within your application. " +
                                                 "These can be either baked or hosted online. " +
                                                 "Select to show its folder within Project for storage.";
    }
}