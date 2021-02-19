using ItemDisplayPlacementHelper.AnimatorEditing;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ItemDisplayPlacementHelper
{
    public class AnimatorParametersController : MonoBehaviour
    {
        private CharacterModel currentModel;
        private Animator currentAnimator;

        public Transform container;
        private readonly List<AnimatorParameterField> rows = new List<AnimatorParameterField>();
       
        [Space]
        public GameObject boolRowPrefab;
        public GameObject intRowPrefab;
        public GameObject floatRowPrefab;

        private void Awake()
        {
            ModelPicker.OnModelChanged += OnModelChanged;
        }

        private void OnDestroy()
        {
            ModelPicker.OnModelChanged -= OnModelChanged;
        }

        private void OnModelChanged(CharacterModel model)
        {
            foreach (var row in rows)
            {
                Destroy(row.gameObject);
            }
            rows.Clear();
            currentAnimator = null;

            currentModel = model;
            if (currentModel)
            {
                currentAnimator = currentModel.GetComponent<Animator>();
            }
            if (currentAnimator)
            {
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
        }
    }
}
