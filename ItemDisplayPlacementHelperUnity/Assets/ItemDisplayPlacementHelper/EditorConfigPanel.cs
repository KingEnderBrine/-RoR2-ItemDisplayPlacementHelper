using ItemDisplayPlacementHelper.AxisEditing;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper
{
    public class EditorConfigPanel : MonoBehaviour
    {
        public CameraRigController cameraRigController;

        [Space]
        public Toggle fadeToggle;
        public Toggle timeToggle;

        [Space]
        public Button editorSpaceButton;
        public TextMeshProUGUI editorSpaceText;

        [Space]
        public Toggle editorModeMove;
        public Toggle editorModeRotate;
        public Toggle editorModeScale;

        private bool skipNotification;

        private void Update()
        {
            skipNotification = true;

            if (cameraRigController)
            {
                fadeToggle.isOn = cameraRigController.enableFading;
            }
            timeToggle.isOn = Time.timeScale > 0;

            editorSpaceButton.interactable = !EditorAxisController.Instance.OverrideToLocalSpace;
            switch (EditorAxisController.Instance.EditSpace)
            {
                case EditSpace.Global:
                    editorSpaceText.text = "Global";
                    break;
                case EditSpace.Local:
                    editorSpaceText.text = "Local";
                    break;
            }

            editorModeMove.isOn = EditorAxisController.Instance.EditMode == EditMode.Move;
            editorModeRotate.isOn = EditorAxisController.Instance.EditMode == EditMode.Rotate;
            editorModeScale.isOn = EditorAxisController.Instance.EditMode == EditMode.Scale;

            skipNotification = false;
        }

        public void ToggleTime(bool enabled)
        {
            if (skipNotification) return;

            Time.timeScale = enabled ? 1 : 0;
        }

        public void ToggleFade(bool enabled)
        {
            if (skipNotification) return;
            
            if (!cameraRigController)
            {
                return;
            }
            cameraRigController.enableFading = enabled;
        }

        public void ToggleEditModeMove(bool value)
        {
            if (skipNotification) return;
            
            if (value)
            {
                EditorAxisController.Instance.EditMode = EditMode.Move;
            }
        }

        public void ToggleEditModeScale(bool value)
        {
            if (skipNotification) return;
            
            if (value)
            {
                EditorAxisController.Instance.EditMode = EditMode.Scale;
            }
        }

        public void ToggleEditModeRotate(bool value)
        {
            if (skipNotification) return;
            
            if (value)
            {
                EditorAxisController.Instance.EditMode = EditMode.Rotate;
            }
        }

        public void ToggleEditSpace()
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
    }
}
