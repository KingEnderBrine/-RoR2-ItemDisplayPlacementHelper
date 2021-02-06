using System.IO;
using UnityEngine;

namespace ItemDisplayPlacementHelper
{
    internal static class AssetsHelper
    {
        public static AssetBundle SceneBundle { get; private set; }

        public static void LoadAssetBundle()
        {
            SceneBundle = AssetBundle.LoadFromFile(GetBundlePath("kingenderbrine_idrs_editor"));
        }

        private static string GetBundlePath(string bundleName)
        {
            return Path.Combine(Path.GetDirectoryName(ItemDisplayPlacementHelperPlugin.Instance.Info.Location), bundleName);
        }
    }
}
