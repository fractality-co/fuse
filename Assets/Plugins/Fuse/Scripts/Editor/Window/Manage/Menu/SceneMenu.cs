using System;
using UnityEditor;

namespace Fuse.Editor
{
    /// <summary>
    /// Sub-menu for managing Scene(s).
    /// </summary>
    public class SceneMenu : AssetCatalogMenu<SceneAsset>
    {
        protected override Type RequiredAttribute => null;
        protected override bool NestByType => false;
        protected override string AssetExtension => "unity";
        protected override string BasePath => Constants.ScenesAssetPath;
        protected override string Subtitle => "manage accessible scenes";
        protected override string Description => "<i>Scenes manage your components, level and lighting data. Scene's available here can be assigned to a State which will the framework will then load.</i>";
    }
}