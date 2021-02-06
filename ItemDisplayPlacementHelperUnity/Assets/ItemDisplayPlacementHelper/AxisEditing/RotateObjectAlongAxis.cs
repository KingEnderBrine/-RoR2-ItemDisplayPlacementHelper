using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ItemDisplayPlacementHelper.AxisEditing
{
    public class RotateObjectAlongAxis : TransformObjectAlongAxis
    {
        protected override void OnDisable()
        {
            base.OnDisable();

            if (circleAxisDrawer)
            {
                circleAxisDrawer.movementStart = default;
                circleAxisDrawer.movementAngle = 0;
            }
        }

        protected override void OnMouseDragActive()
        {
            if (editorAxisController.SelectedObject)
            {
                var magnitude = GetMovementMagnitude();
                var rotationVector = Vector3.zero;
                var space = (Space)editorAxisController.EditSpace;
                
                switch (axis)
                {
                    case Axis.X:
                        rotationVector = new Vector3(magnitude, 0, 0);
                        break;
                    case Axis.Y:
                        rotationVector = new Vector3(0, magnitude, 0);
                        break;
                    case Axis.Z:
                        rotationVector = new Vector3(0, 0, magnitude);
                        break;
                    case Axis.CameraPerpendicular:
                        rotationVector = Camera.main.transform.rotation * new Vector3(0, 0, magnitude);
                        space = Space.World;
                        break;
                }


                if (rotationVector == default)
                {
                    return;
                }

                var oldRotation = editorAxisController.SelectedObject.rotation;

                editorAxisController.SelectedObject.Rotate(rotationVector, space);

                var angleDelta = Quaternion.Angle(oldRotation, editorAxisController.SelectedObject.rotation) * Mathf.Sign(magnitude) * Mathf.Deg2Rad;

                if (circleAxisDrawer)
                {
                    switch (editorAxisController.EditSpace)
                    {
                        case EditSpace.Global:
                            circleAxisDrawer.movementAngle += angleDelta;
                            break;
                        case EditSpace.Local:
                            if (axis == Axis.CameraPerpendicular)
                            {
                                circleAxisDrawer.movementAngle += angleDelta;
                            }
                            else
                            {
                                circleAxisDrawer.movementAngle -= angleDelta;
                            }
                            break;
                    }
                }
            }
        }

        protected override void OnMouseDown()
        {
            startedDragOutOfScene = !EditorSceneCameraController.Instance.PointerInside;
            if (startedDragOutOfScene)
            {
                return;
            }

            var objectOnScreenPosition = Camera.main.WorldToScreenPoint(transform.position);

            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 10000, LayerIndex.uiWorldSpace.mask);

            var hitPointOnAxis = Vector3.ProjectOnPlane(transform.InverseTransformPoint(hit.point), Vector3.forward).normalized;

            normalizedDirection = Camera.main.transform.InverseTransformDirection(transform.TransformDirection(Vector2.Perpendicular(hitPointOnAxis))).normalized;
            startDistanceToObject = objectOnScreenPosition.z;

            lastMousePosition = Input.mousePosition;
            editorAxisController.SelectedAxis = axis;
            startEditSpace = editorAxisController.EditSpace;
            canDrag = true;
            startObjectScale = editorAxisController.SelectedObject.localScale;

            if (circleAxisDrawer)
            {
                circleAxisDrawer.movementStart = hitPointOnAxis;
            }
        }

        protected override void OnMouseUp()
        {
            base.OnMouseUp();

            if (circleAxisDrawer)
            {
                circleAxisDrawer.movementStart = Vector3.zero;
                circleAxisDrawer.movementAngle = 0;
            }
        }

        protected override float GetMovementMagnitude()
        {
            var mouseDelta = Input.mousePosition - lastMousePosition;
            if (mouseDelta.magnitude == 0)
            {
                return 0;
            }

            return Vector2.Dot(mouseDelta, normalizedDirection) * Mathf.Rad2Deg * 0.02F;
        }
    }
}
