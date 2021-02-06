using UnityEngine;

namespace ItemDisplayPlacementHelper
{
    public class SensitivityController : MonoBehaviour
    {
        public SliderWithInput fastCoefficientInput;
        public SliderWithInput slowCoefficientInput;

        [Space]
        public GameObject container;

        public static SensitivityController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            fastCoefficientInput.OnValueChanged += FastCoefficientChanged;
            slowCoefficientInput.OnValueChanged += SlowCoefficientChanged;
        }

        private void OnDestroy()
        {
            Instance = null;

            fastCoefficientInput.OnValueChanged -= FastCoefficientChanged;
            slowCoefficientInput.OnValueChanged -= SlowCoefficientChanged;
        }

        private void Start()
        {
            EditorSceneCameraController.Instance.fastCoefficient = fastCoefficientInput.Value;
            EditorSceneCameraController.Instance.slowCoefficient = slowCoefficientInput.Value;
        }

        public void ToggleContainerVisibility()
        {
            container.SetActive(!container.activeSelf);
        }

        private void FastCoefficientChanged(float newValue)
        {
            EditorSceneCameraController.Instance.fastCoefficient = newValue;
        }

        private void SlowCoefficientChanged(float newValue)
        {
            EditorSceneCameraController.Instance.slowCoefficient = newValue;
        }
    }
}
