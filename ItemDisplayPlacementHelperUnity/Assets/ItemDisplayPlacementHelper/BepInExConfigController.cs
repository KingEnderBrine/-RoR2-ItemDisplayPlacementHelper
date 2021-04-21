using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ItemDisplayPlacementHelper
{
    public class BepInExConfigController : MonoBehaviour
    {
        public SensitivityController sensitivityController;
        public ParentedPrefabDisplayController parentedPrefabDisplayController;
        public EditorSceneCameraController editorSceneCameraController;

        private void Start()
        {
            sensitivityController.fastCoefficientInput.Value = ConfigHelper.FastCoefficient.Value;
            sensitivityController.slowCoefficientInput.Value = ConfigHelper.SlowCoefficient.Value;
            parentedPrefabDisplayController.precisionInput.text = ConfigHelper.CopyPrecision.Value.ToString();
            editorSceneCameraController.controlScheme = ConfigHelper.ControlScheme.Value;

            StartCoroutine(SaveCurrentValues());
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private IEnumerator SaveCurrentValues()
        {
            while (true)
            {
                yield return new WaitForSeconds(10);

                ConfigHelper.FastCoefficient.Value = sensitivityController.fastCoefficientInput.Value;
                ConfigHelper.SlowCoefficient.Value = sensitivityController.slowCoefficientInput.Value;
                ConfigHelper.CopyPrecision.Value = parentedPrefabDisplayController.CurrentPrecision;
                ConfigHelper.ControlScheme.Value = editorSceneCameraController.controlScheme;
            }
        }
    }
}
