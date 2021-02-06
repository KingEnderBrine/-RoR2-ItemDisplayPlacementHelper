using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ItemDisplayPlacementHelper.AxisEditing
{
    public abstract class TransformObjectAlongAxis : MonoBehaviour
    {
        public EditorAxisController editorAxisController;
        public Axis axis;
        public Color hoverColor;
        public Color initialColor;
        public Color inactiveColor;

        protected Vector3 lastMousePosition;
        protected CircleAxisDrawer circleAxisDrawer;
        protected LineAxisDrawer lineAxisDrawer;
        protected new Renderer renderer; 
        protected bool mouseHover;

        protected bool canDrag;
        protected EditSpace startEditSpace;
        protected bool editSpaceChanged;
        protected bool startedDragOutOfScene;
        protected float startDistanceToObject;
        protected Vector2 normalizedDirection;
        protected Vector3 startObjectScale;

        protected const float magnitudeMultiplier = 0.002F;

        protected virtual void Awake()
        {
            renderer = GetComponentInChildren<Renderer>();
            if (renderer)
            {
                initialColor = renderer.material.color;
            }

            lineAxisDrawer = GetComponent<LineAxisDrawer>();
            if (lineAxisDrawer)
            {
                lineAxisDrawer.color = initialColor;
                lineAxisDrawer.axis = axis;
            }

            circleAxisDrawer = GetComponent<CircleAxisDrawer>();
            if (circleAxisDrawer)
            {
                circleAxisDrawer.color = initialColor;
            }
        }

        protected virtual void OnDisable()
        {
            canDrag = false;
            startedDragOutOfScene = false;
            mouseHover = false;
            editorAxisController.SelectedAxis = Axis.None;
            editSpaceChanged = false;
        }

        protected virtual void Update()
        {
            var color = GetUpdateColor();
            if (renderer)
            {
                renderer.material.color = color;
            }
            if (lineAxisDrawer)
            {
                lineAxisDrawer.color = color;
            }
            if (circleAxisDrawer)
            {
                circleAxisDrawer.color = color;
            }
        }

        private Color GetUpdateColor()
        {
            if (editorAxisController.SelectedAxis == Axis.None)
            {
                if (mouseHover && EditorSceneCameraController.Instance.PointerInside)
                {
                    return hoverColor;
                }
                return initialColor;
            }
            if (editorAxisController.SelectedAxis == axis)
            {
                return hoverColor;
            }
            return inactiveColor;
        }

        protected virtual void OnMouseDown()
        {
            startedDragOutOfScene = !EditorSceneCameraController.Instance.PointerInside;
            if (startedDragOutOfScene)
            {
                return;
            }

            Vector3 objectDirection = Vector3.zero;

            switch (axis)
            {
                case Axis.X:
                    objectDirection = transform.right;
                    break;
                case Axis.Y:
                    objectDirection = transform.up;
                    break;
                case Axis.Z:
                    objectDirection = transform.forward;
                    break;
            }

            var objectOnScreenDirection = Camera.main.WorldToScreenPoint(transform.position + Vector3.ClampMagnitude(objectDirection, 0.001F));
            var objectOnScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
            var directon = objectOnScreenDirection - objectOnScreenPosition;

            normalizedDirection = directon.normalized;
            startDistanceToObject = objectOnScreenPosition.z;

            lastMousePosition = Input.mousePosition;
            editorAxisController.SelectedAxis = axis;
            startEditSpace = editorAxisController.EditSpace;
            canDrag = true;
            startObjectScale = editorAxisController.SelectedObject.localScale;
        }

        protected virtual void OnMouseUp()
        {
            editorAxisController.SelectedAxis = Axis.None;
            editSpaceChanged = false;
            startedDragOutOfScene = false;
            canDrag = false;
        }

        protected virtual void OnMouseEnter()
        {
            mouseHover = true;
        }

        protected virtual void OnMouseExit()
        {
            mouseHover = false;
        }

        protected virtual void OnMouseDrag()
        {
            if (!canDrag || startedDragOutOfScene || editSpaceChanged)
            {
                return;
            }

            if (startEditSpace != editorAxisController.EditSpace)
            {
                editorAxisController.SelectedAxis = Axis.None;
                editSpaceChanged = true;
                if (circleAxisDrawer)
                {
                    circleAxisDrawer.movementStart = default;
                    circleAxisDrawer.movementAngle = 0;
                }
                return;
            }

            OnMouseDragActive();

            lastMousePosition = Input.mousePosition;
        }

        protected virtual void OnMouseDragActive()
        {

        }

        protected virtual float GetMovementMagnitude()
        {
            var mouseDelta = Input.mousePosition - lastMousePosition;
            if (mouseDelta.magnitude == 0)
            {
                return 0;
            }

            return Vector2.Dot(mouseDelta, normalizedDirection) * startDistanceToObject * magnitudeMultiplier;
        }
    }
}
