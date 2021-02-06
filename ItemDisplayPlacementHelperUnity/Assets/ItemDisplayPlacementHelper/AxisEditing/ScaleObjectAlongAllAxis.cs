using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ItemDisplayPlacementHelper.AxisEditing
{
    public class ScaleObjectAlongAllAxis : TransformObjectAlongAxis
    {
        protected override void OnMouseDragActive()
        {
            if (editorAxisController.SelectedObject)
            {
                var magnitude = GetMovementMagnitude();
                
                if (magnitude == 0)
                {
                    return;
                }

                editorAxisController.SelectedObject.localScale += Vector3.Scale(new Vector3(magnitude, magnitude, magnitude), startObjectScale);
            }
        }

        protected override float GetMovementMagnitude()
        {
            var mouseDelta = Input.mousePosition - lastMousePosition;
            if (mouseDelta.magnitude == 0)
            {
                return 0;
            }

            return Vector2.Dot(mouseDelta, Vector2.one) * startDistanceToObject * magnitudeMultiplier;
        }
    }
}
