using UnityEditor;

namespace Fuse.Editor
{
    /// <summary>
    /// Handles the post-processing for assets.
    /// </summary>
    public class BuildAssetProcessor : BuildProcessor
    {
        public new const int CallbackOrder = 50;

        protected override int Callback => CallbackOrder;

        public override void PreProcessPlayer(Environment environment, BuildTarget buildTarget,
            BuildTargetGroup buildTargetGroup)
        {
            AssetProcessor.Build(buildTarget, environment);
            AssetProcessor.Integrate(buildTarget, environment);
        }

        public override void PostProcessPlayer(Environment environment,
            BuildTarget buildTarget,
            BuildTargetGroup buildTargetGroup)
        {
            AssetProcessor.Cleanup();
        }
    }
}