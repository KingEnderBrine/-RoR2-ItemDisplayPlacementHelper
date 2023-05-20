using UnityEngine;

namespace ItemDisplayPlacementHelper
{
    public class MatchLocalScale : MonoBehaviour
    {
        public Transform target;

        private void Update()
        {
            if (target)
            {
                transform.localScale = target.localScale;
            }
        }
    }
}
