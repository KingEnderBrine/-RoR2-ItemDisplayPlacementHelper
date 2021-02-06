using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ItemDisplayPlacementHelper.AxisEditing
{
    public class ScaleObjectAlongAxis : TransformObjectAlongAxis
    {
        protected override void OnMouseDragActive()
        {
            if (editorAxisController.SelectedObject)
            {
                var additionalScale = Vector3.zero;
                switch (axis)
                {
                    case Axis.X:
                        additionalScale = new Vector3(GetMovementMagnitude(), 0, 0);
                        break;
                    case Axis.Y:
                        additionalScale = new Vector3(0, GetMovementMagnitude(), 0);
                        break;
                    case Axis.Z:
                        additionalScale = new Vector3(0, 0, GetMovementMagnitude());
                        break;
                }

                if (additionalScale == default)
                {
                    return;
                }

                editorAxisController.SelectedObject.localScale += Vector3.Scale(additionalScale, startObjectScale);
            }
        }
    }
}
