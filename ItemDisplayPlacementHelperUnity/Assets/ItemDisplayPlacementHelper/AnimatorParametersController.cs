using ItemDisplayPlacementHelper.AnimatorEditing;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper
{
    public class AnimatorParametersController : MonoBehaviour
    {
        private Animator currentAnimator;

        public Transform container;
        private readonly List<AnimatorParameterField> rows = new List<AnimatorParameterField>();

        [Space]
        public Button toggleButton;
        public GameObject itemPanel;
        public GameObject parametersPanel;
       
        [Space]
        public GameObject boolRowPrefab;
        public GameObject intRowPrefab;
        public GameObject floatRowPrefab;

        private void Awake()
        {
            AnimatorPanelController.OnAnimatorChanged += OnAnimatorChanged;
        }

        private void OnDestroy()
        {
            AnimatorPanelController.OnAnimatorChanged -= OnAnimatorChanged;
        }

        private void OnAnimatorChanged(Animator animator)
        {
            foreach (var row in rows)
            {
                Destroy(row.gameObject);
            }
            rows.Clear();
            currentAnimator = animator;

            if (!animator)
            {
                return;
            }

            foreach (var parameter in currentAnimator.parameters)
            {
                GameObject row = null;
                switch (parameter.type)
                {
                    case AnimatorControllerParameterType.Float:
                        row = Instantiate(floatRowPrefab, container);
                        break;
                    case AnimatorControllerParameterType.Int:
                        row = Instantiate(intRowPrefab, container);
                        break;
                    case AnimatorControllerParameterType.Bool:
                        row = Instantiate(boolRowPrefab, container);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        continue;
                }

                var parameterField = row.GetComponent<AnimatorParameterField>();
                parameterField.animator = currentAnimator;
                parameterField.parameter = parameter;
                
                rows.Add(parameterField);

                row.SetActive(true);
            }
        }

        public void Toggle()
        {
            if (parametersPanel.activeSelf)
            {
                parametersPanel.SetActive(false);
                itemPanel.SetActive(true);
            }
            else
            {
                parametersPanel.SetActive(true);
                itemPanel.SetActive(false);
            }

            var scale = toggleButton.transform.localScale;
            scale.y *= -1;
            toggleButton.transform.localScale = scale;
        }
    }
}
