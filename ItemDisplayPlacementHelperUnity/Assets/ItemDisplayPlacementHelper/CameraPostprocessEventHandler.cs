using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ItemDisplayPlacementHelper
{
    public class CameraPostprocessEventHandler : MonoBehaviour
    {
        public static EventHandler onPostRender;

        private void OnPostRender()
        {
            onPostRender?.Invoke(this, null);
        }
    }
}
