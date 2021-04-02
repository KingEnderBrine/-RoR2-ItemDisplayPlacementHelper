using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace ItemDisplayPlacementHelper
{
    public class PostProcessingValuesInit : MonoBehaviour
    {
        public static PostProcessResources PostProcessResources { get; set; }
        public static PostProcessProfile PostProcessProfile { get; set; }

        public PostProcessLayer postProcessLayer;
        public PostProcessVolume postProcessVolume;

        private void Awake()
        {
            typeof(PostProcessLayer).GetField("m_Resources", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(postProcessLayer, PostProcessResources);
            postProcessVolume.sharedProfile = PostProcessProfile;
        }
    }
}
