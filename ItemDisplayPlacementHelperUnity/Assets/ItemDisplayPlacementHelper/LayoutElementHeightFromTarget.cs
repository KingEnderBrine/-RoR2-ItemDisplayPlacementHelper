using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper
{
    public class LayoutElementHeightFromTarget : LayoutElement
    {
        public float maxHeight = -1;
        public RectTransform target;

        public override float preferredHeight
        {
            get
            {
                var height = target.rect.height;
                if (maxHeight != -1 && target)
                {

                    return height > maxHeight ? maxHeight : height;
                }
                else
                {
                    return height;
                }
            }
            set => base.preferredHeight = value;
        }
    }
}
