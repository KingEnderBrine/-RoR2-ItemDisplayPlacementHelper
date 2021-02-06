using UnityEngine;

namespace ItemDisplayPlacementHelper.AxisEditing
{
    public class MoveObjectAlongAxis : TransformObjectAlongAxis
    {
        protected override void OnMouseDragActive()
        {
            if (editorAxisController.SelectedObject)
            {
                var movementVector = Vector3.zero;
                switch (axis)
                {
                    case Axis.X:
                        movementVector = new Vector3(GetMovementMagnitude(), 0, 0);
                        break;
                    case Axis.Y:
                        movementVector = new Vector3(0, GetMovementMagnitude(), 0);
                        break;
                    case Axis.Z:
                        movementVector = new Vector3(0, 0, GetMovementMagnitude());
                        break;
                }
                if (movementVector == default)
                {
                    return;
                }
                switch (editorAxisController.EditSpace)
                {
                    case EditSpace.Global:
                        editorAxisController.SelectedObject.position += movementVector;
                        break;
                    case EditSpace.Local:
                        editorAxisController.SelectedObject.position += editorAxisController.SelectedObject.rotation * movementVector;
                        break;
                }
            }
        }
    }
}
