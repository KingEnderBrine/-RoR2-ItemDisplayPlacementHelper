using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ItemDisplayPlacementHelper
{
    public class RotateWithObject : MonoBehaviour
    {
        public Transform objectTransform;

        private void LateUpdate()
        {
            if (!objectTransform)
            {
                return;
            }

            transform.rotation = objectTransform.rotation;
        }
    }
}
