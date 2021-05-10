using ItemDisplayPlacementHelper.AxisEditing;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper
{
    public class EditorKeysHandler : MonoBehaviour
    {
        public CameraRigController cameraRigController;
        
        private MPEventSystemLocator eventSystemLocator;

        private GameObject lastCheckedObject;
        private bool disallowKeyPress;

        private void Awake()
        {
            eventSystemLocator = GetComponent<MPEventSystemLocator>();
        }

        private void Update()
        {
            if (!eventSystemLocator.eventSystem)
            {
                return;
            }
            var selectedObject = eventSystemLocator.eventSystem.currentSelectedGameObject;
            if (!selectedObject)
            {
                disallowKeyPress = false;
                lastCheckedObject = null;
            }
            else if (selectedObject != lastCheckedObject)
            {
                if (selectedObject.GetComponent<TMP_InputField>() ||
                    selectedObject.GetComponent<InputField>())
                {
                    disallowKeyPress = true;
                }
                else
                {
                    disallowKeyPress = false;
                }
                lastCheckedObject = selectedObject;
            }

            if (disallowKeyPress)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                EditorAxisController.Instance.EditMode = EditMode.Move;
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                EditorAxisController.Instance.EditMode = EditMode.Rotate;
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                EditorAxisController.Instance.EditMode = EditMode.Scale;
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                switch (EditorAxisController.Instance.ActualEditSpace)
                {
                    case EditSpace.Global:
                        EditorAxisController.Instance.ActualEditSpace = EditSpace.Local;
                        break;
                    case EditSpace.Local:
                        EditorAxisController.Instance.ActualEditSpace = EditSpace.Global;
                        break;
                }
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                Time.timeScale = Time.timeScale == 0 ? 1 : 0;
            }
            if (Input.GetKeyDown(KeyCode.F) && ParentedPrefabDisplayController.Instance && ParentedPrefabDisplayController.Instance.ParentedPrefabDisplay.instance)
            {
                EditorSceneCameraController.Instance.FocusOnPoint(ParentedPrefabDisplayController.Instance.ParentedPrefabDisplay.instance.transform.position);
            }
            if (Input.GetKeyDown(KeyCode.G) && cameraRigController)
            {
                cameraRigController.enableFading = !cameraRigController.enableFading;
            }
        }
    }
}
