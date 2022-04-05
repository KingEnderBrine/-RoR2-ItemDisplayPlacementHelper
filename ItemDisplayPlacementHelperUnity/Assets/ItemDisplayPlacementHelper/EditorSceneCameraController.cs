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
        private const float minFocusLength = 0.1F;
        private float focusLength = 4F;

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

        private Vector3? lerpPosition;
        private float lerpCameraTime;

        private void Awake()
        {
            Instance = this;

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
                        if (Input.GetKey(KeyCode.LeftAlt))
                        {
                            foreach (var row in ModelPicker.Instance.CachedSkinnedMeshRenderers)
                            {
                                var localScale = row.Key.transform.localScale;
                                var lossyScale = row.Key.transform.lossyScale;
                                var scaleProportion = new Vector3(localScale.x / lossyScale.x, localScale.y / lossyScale.y, localScale.z / lossyScale.z);
                                if (localScale != Vector3.one || scaleProportion != Vector3.one)
                                {
                                    row.Key.transform.localScale = scaleProportion;
                                }

                                row.Key.BakeMesh(row.Value.sharedMesh);
                                row.Value.sharedMesh = row.Value.sharedMesh;

                                if (localScale != Vector3.one || scaleProportion != Vector3.one)
                                {
                                    row.Key.transform.localScale = localScale;
                                }
                            }
                            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo, 5000F, LayerMask.GetMask("World")))
                            {
                                lerpPosition = hitInfo.point + CameraRigController.transform.forward * -1 * focusLength;
                                lerpCameraTime = 0;
                            }
                        }
                        else
                        {
                            currentActionType = ActionType.Movement;
                            lerpPosition = null;
                        }
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        currentActionType = ActionType.Rotation;
                        lerpPosition = null;
                    }
                }

                var scrollMovement = CameraRigController.transform.forward * Input.mouseScrollDelta.y * forwardMovementSensitivity * forwardMovementMultiplier * coefficient;
                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    var sign = Mathf.Sign(Input.mouseScrollDelta.y);
                    if (sign == 1 && focusLength <= minFocusLength)
                    {
                        scrollMovement = Vector3.zero;
                        focusLength = minFocusLength;
                    }
                    else
                    {
                        var focusPointScale = Mathf.Max((float)Math.Log(focusLength), 0.5F);
                        scrollMovement *= focusPointScale;
                        if (focusLength - sign * scrollMovement.magnitude <= minFocusLength)
                        {
                            scrollMovement = Vector3.zero;
                        }
                        focusLength -= sign * scrollMovement.magnitude;
                    }
                }

                if (scrollMovement != Vector3.zero)
                {
                    lerpPosition = null;
                }

                CameraRigController.transform.position += scrollMovement;
            }

            if (lerpPosition.HasValue)
            {
                lerpCameraTime += Time.unscaledDeltaTime;
                CameraRigController.transform.position = Vector3.Lerp(CameraRigController.transform.position, lerpPosition.Value, lerpCameraTime);
                if (lerpCameraTime >= 1)
                {
                    lerpPosition = null;
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
                        break;
                    case ActionType.Rotation:
                        if (Input.GetKey(KeyCode.LeftAlt))
                        {
                            var focusPoint = CameraRigController.transform.position + CameraRigController.transform.forward * focusLength;
                            CameraRigController.transform.RotateAround(focusPoint, CameraRigController.transform.right, deltaMousePosition.y * rotationSensitivity * rotationMultiplier * -2);
                            CameraRigController.transform.RotateAround(focusPoint, Vector3.up, deltaMousePosition.x * rotationSensitivity * rotationMultiplier * 2);
                        }
                        else
                        {
                            CameraRigController.transform.Rotate(Vector3.right, deltaMousePosition.y * rotationSensitivity * rotationMultiplier * -1, Space.Self);
                            CameraRigController.transform.Rotate(Vector3.up, deltaMousePosition.x * rotationSensitivity * rotationMultiplier, Space.World);
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

        public void FocusOnPoint(Vector3 point)
        {
            lerpPosition = CameraRigController.transform.forward * -1 * focusLength + point;
            lerpCameraTime = 0;
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
