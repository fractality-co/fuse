using UnityEditor;
using UnityEngine;

namespace Fuse.Editor
{
    /// <summary>
    /// Popup to confirm deletion of an object.
    /// </summary>
    public class DeletePopup : Popup
    {
        private const string ErrorMessage = "Fuse was unable to delete this asset! " +
                                            "Please ensure no other programs or the OS is currently accessing the file.";

        protected override string Title => "DELETE";
        protected override string Subtitle => Category.ToUpper();
        protected override ActionLayout Action => ActionLayout.AcceptDecline;
        protected override string Toggle => "";

        protected override string Body =>
            $"Are you sure you want to delete the <b>{Category}</b>: \"{(Target == null ? Type.Name : Target.name)}\"?";

        private string Category => Target == null
            ? (Type.IsSubclassOf(typeof(Module)) ? "Module" : Type.Name)
            : Target.GetType().Name.Replace("Asset", string.Empty);

        protected override void OnAccept()
        {
            if (Selection.activeObject == Target)
                Selection.activeObject = null;

            if (Type != null && Type.IsSubclassOf(typeof(Module)))
            {
                var guids = AssetDatabase.FindAssets(Type.Name + " t:Script");
                if (guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    if (!AssetDatabase.DeleteAsset(path))
                        Display<ErrorPopup>(ErrorMessage);
                    else
                    {
                        AssetDatabase.Refresh();
                        GUI.changed = true;
                    }
                }
            }
            else if (!AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(Target)))
                Display<ErrorPopup>(ErrorMessage);
            else
            {
                AssetDatabase.Refresh();
                GUI.changed = true;
            }
        }
    }
}