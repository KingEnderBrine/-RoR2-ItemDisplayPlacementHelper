using System.Reflection;
using ItemDisplayPlacementHelper.AxisEditing;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper
{
    public class EditorConfigPanel : MonoBehaviour
    {
        private static readonly MethodInfo dynamicBonesLateUpdateMethod = typeof(DynamicBone).GetMethod(nameof(DynamicBone.LateUpdate), BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo timeGetUnscaledDeltaTimeMethod = typeof(Time).GetProperty(nameof(Time.unscaledDeltaTime)).GetGetMethod();

        public CameraRigController cameraRigController;

        [Space]
        public Toggle fadeToggle;
        public Toggle timeToggle;
        public Toggle dynamicBonesTimeToggle;

        [Space]
        public Button editorSpaceButton;
        public TextMeshProUGUI editorSpaceText;

        [Space]
        public Toggle editorModeMove;
        public Toggle editorModeRotate;
        public Toggle editorModeScale;

        private bool skipNotification;
        private ILHook dynamicBonesHook;

        private void OnDestroy()
        {
            if (dynamicBonesHook != null)
            {
                dynamicBonesHook.Undo();
            }
        }

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

        public void ToggleDynamicBonesTimeScale(bool enabled)
        {
            if (skipNotification) return;

            if (enabled)
            {
                if (dynamicBonesHook == null)
                {
                    dynamicBonesHook = new ILHook(dynamicBonesLateUpdateMethod, DynamicBonesLateUpdateIL);
                }
            }
            else
            {
                if (dynamicBonesHook != null)
                {
                    dynamicBonesHook.Undo();
                    dynamicBonesHook = null;
                }
            }
        }

        private void DynamicBonesLateUpdateIL(ILContext il)
        {
            var cursor = new ILCursor(il);
            cursor.GotoNext(
                MoveType.After,
                x => x.MatchCallOrCallvirt<Time>("get_deltaTime"));
            cursor.Emit(OpCodes.Pop);
            cursor.Emit(OpCodes.Call, timeGetUnscaledDeltaTimeMethod);
        }
    }
}
