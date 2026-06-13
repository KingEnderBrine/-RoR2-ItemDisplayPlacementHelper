using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper
{
    [RequireComponent(typeof(LayoutElement))]
    public class TargetMinSize : MonoBehaviour
    {
        public bool useHeight;
        public float heightOffset;
        public bool useWidth;
        public float widthOffset;
        public RectTransform target;
        private LayoutElement layoutElement;

        public void Awake()
        {
            layoutElement = GetComponent<LayoutElement>();
        }

        public void Update()
        {
            if (!layoutElement || !target)
            {
                return;
            }

            if (useHeight)
            {
                layoutElement.minHeight = LayoutUtility.GetPreferredHeight(target) - heightOffset;
            }

            if (useWidth)
            {
                layoutElement.minWidth = LayoutUtility.GetPreferredWidth(target) - widthOffset;
            }
        }
    }
}
