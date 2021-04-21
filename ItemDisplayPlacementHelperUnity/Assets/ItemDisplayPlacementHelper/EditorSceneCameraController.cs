using ItemDisplayPlacementHelper.AxisEditing;
using RoR2;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper
{
    public class EditorSceneCameraController : MonoBehaviour, ICameraStateProvider, IPointerEnterHandler, IPointerExitHandler
    {
        private Vector3 focusPoint = default;
        public enum ControlScheme { Unity, Blender }
        public ControlScheme controlScheme;

        enum ActionType { None, Rotation, Movement }

        private const float rotationMultiplier = 0.15F;
        private const float sidewaysMovementMultiplier = 0.01F;
        private const float forwardMovementMultiplier = 0.3F;

        public float sidewaysMovementSensitivity = 1;
        public float rotationSensitivity = 1;
        public float forwardMovementSensitivity = 1F;
        public float slowCoefficient = 0.1F;
        public float fastCoefficient = 3;

        [Space]
        public Transform cameraDefaultPosition;
        public CameraRigController CameraRigController;

        public static EditorSceneCameraController Instance;

        private Vector3 previousMousePosition;
        public bool PointerInside { get; private set; }
        private ActionType currentActionType;

        public RectTransform rectTransform;

        private void Awake()
        {
            Instance = this;

            rectTransform = GetComponent<RectTransform>();

            CameraRigController.SetOverrideCam(this, 0);
            
            CameraRigController.transform.SetPositionAndRotation(cameraDefaultPosition.position, cameraDefaultPosition.rotation);
        }

        private void OnDestroy()
        {
            Instance = null;
            if (CameraRigController)
            {
                CameraRigController.SetOverrideCam(null, 0);
            }
        }

        private void Update()
        {
            var coefficient = Input.GetKey(KeyCode.LeftControl) ? slowCoefficient : Input.GetKey(KeyCode.LeftShift) ? fastCoefficient : 1;
            
            if (PointerInside)
            {
                if (currentActionType == ActionType.None)
                {
                    if (Input.GetMouseButtonDown(2))
                    {
                        if (Input.GetKeyDown(KeyCode.LeftAlt))
                        {
                            //TODO: change focus point
                        }
                        else
                        {
                            currentActionType = ActionType.Movement;
                        }
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        currentActionType = ActionType.Rotation;
                    }
                }

                switch (controlScheme)
                {
                    case ControlScheme.Unity:
                        CameraRigController.transform.position += CameraRigController.transform.forward * Input.mouseScrollDelta.y * forwardMovementSensitivity * forwardMovementMultiplier * coefficient;
                        break;
                    case ControlScheme.Blender:
                        CameraRigController.transform.position += (float)Math.Log((CameraRigController.transform.position - focusPoint).magnitude) * CameraRigController.transform.forward * Input.mouseScrollDelta.y * forwardMovementSensitivity * forwardMovementMultiplier * coefficient;
                        break;
                }
            }

            if (EditorAxisController.Instance.SelectedAxis == Axis.None)
            {
                var deltaMousePosition = Input.mousePosition - previousMousePosition;
                switch (currentActionType)
                {
                    case ActionType.Movement:
                        CameraRigController.transform.position -= CameraRigController.transform.up * deltaMousePosition.y * sidewaysMovementSensitivity * sidewaysMovementMultiplier * coefficient;
                        CameraRigController.transform.position -= CameraRigController.transform.right * deltaMousePosition.x * sidewaysMovementSensitivity * sidewaysMovementMultiplier * coefficient;
                        if (controlScheme == ControlScheme.Blender)
                        {
                            //TODO: move focusPoint
                        }
                        break;
                    case ActionType.Rotation:
                        switch (controlScheme)
                        {
                            case ControlScheme.Unity:
                                CameraRigController.transform.Rotate(Vector3.right, deltaMousePosition.y * rotationSensitivity * rotationMultiplier * -1, Space.Self);
                                CameraRigController.transform.Rotate(Vector3.up, deltaMousePosition.x * rotationSensitivity * rotationMultiplier, Space.World);
                                break;
                            case ControlScheme.Blender:
                                //TODO: camera orbiting
                                break;
                        }
                        break;
                }
            }

            if (currentActionType != ActionType.None && Input.GetMouseButtonUp((int)currentActionType))
            {
                currentActionType = ActionType.None;
            }

            previousMousePosition = Input.mousePosition;
        }

        public void GetCameraState(CameraRigController cameraRigController, ref CameraState cameraState)
        {
            cameraState.rotation = cameraRigController.transform.rotation;
            cameraState.position = cameraRigController.transform.position;
        }

        public bool IsHudAllowed(CameraRigController cameraRigController) => false;

        public bool IsUserControlAllowed(CameraRigController cameraRigController) => false;

        public bool IsUserLookAllowed(CameraRigController cameraRigController) => false;

        public void OnPointerExit(PointerEventData eventData)
        {
            PointerInside = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerInside = true;
        }

        //Too many edge cases with that and it only will work on Windows.
        //On hold for now, maybe I will come back to this later.
        /*[DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        private void DetectMouseOutOfBounds()
        {

            Debug.LogWarning(Input.mousePosition);
            Debug.LogWarning(transform.InverseTransformPoint(Input.mousePosition));

            var mousePosition = Input.mousePosition;
            var transformMousePosition = transform.InverseTransformPoint(mousePosition);

            var newTransformMousePosition = new Vector3(transformMousePosition.x, transformMousePosition.y);

            if (transformMousePosition.x < rectTransform.rect.x)
            {
                newTransformMousePosition.x += rectTransform.rect.width;
                Debug.LogWarning("Exit left");
            }
            else if (transformMousePosition.x > rectTransform.rect.xMax)
            {
                newTransformMousePosition.x -= rectTransform.rect.width;
                Debug.LogWarning("Exit right");
            }

            if (transformMousePosition.y < rectTransform.rect.y)
            {
                newTransformMousePosition.y += rectTransform.rect.height;
                Debug.LogWarning("Exit bottom");
            }
            else if (transformMousePosition.y > rectTransform.rect.yMax)
            {
                newTransformMousePosition.y -= rectTransform.rect.height;
                Debug.LogWarning("Exit top");
            }

            Debug.Log(newTransformMousePosition);
            if (newTransformMousePosition != transformMousePosition)
            {
                newTransformMousePosition.y *= -1;
                var newMousePosition = transform.TransformPoint(newTransformMousePosition);
                Debug.Log(newMousePosition);
                SetCursorPos((int)newMousePosition.x, (int)newMousePosition.y);
            }
        }*/
    }
}
