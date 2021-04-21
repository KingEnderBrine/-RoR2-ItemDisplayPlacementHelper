using ItemDisplayPlacementHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static Rewired.Controller;

namespace ItemDisplayPlacementHelper
{
    public class ControlSchemeController : MonoBehaviour
    {
        public TextMeshProUGUI controlSchemeText;

        private void Update()
        {
            switch (EditorSceneCameraController.Instance.controlScheme)
            {
                case EditorSceneCameraController.ControlScheme.Unity:
                    controlSchemeText.text = "Unity";
                    break;
                case EditorSceneCameraController.ControlScheme.Blender:
                    controlSchemeText.text = "Blender";
                    break;
            }
        }

        public void ToggleControlScheme()
        {
            switch (EditorSceneCameraController.Instance.controlScheme)
            {
                case EditorSceneCameraController.ControlScheme.Unity:
                    EditorSceneCameraController.Instance.controlScheme = EditorSceneCameraController.ControlScheme.Blender;
                    break;
                case EditorSceneCameraController.ControlScheme.Blender:
                    EditorSceneCameraController.Instance.controlScheme = EditorSceneCameraController.ControlScheme.Unity;
                    break;
            }
        }
    }
}
