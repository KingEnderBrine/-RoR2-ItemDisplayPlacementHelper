using R2API.Utils;
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
            postProcessLayer.SetFieldValue("m_Resources", PostProcessResources);
            postProcessVolume.sharedProfile = PostProcessProfile;
        }
    }
}
