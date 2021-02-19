using ItemDisplayPlacementHelper.AnimatorEditing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper.AnimatorEditing
{
    public class AnimatorParameterField : MonoBehaviour
    {
        [HideInInspector]
        public Animator animator;
        [HideInInspector]
        public AnimatorControllerParameter parameter;

        public Toggle toggle;
        public TMP_InputField inputField;
        public TMP_Text label;

        private bool currentBoolValue;
        private float currentFloatValue;
        private int currentIntValue;

        private void Start()
        {
            label.text = parameter.name;
        }

        private void LateUpdate()
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Bool:
                    var boolValue = animator.GetBool(parameter.name);
                    if (currentBoolValue != boolValue)
                    {
                        currentBoolValue = boolValue;
                        toggle.isOn = currentBoolValue;
                    }
                    break;
                case AnimatorControllerParameterType.Int:
                    var intValue = animator.GetInteger(parameter.name);
                    if (currentIntValue != intValue)
                    {
                        currentIntValue = intValue;
                        inputField.text = currentIntValue.ToString();
                    }
                    break;
                case AnimatorControllerParameterType.Float:
                    var floatValue = animator.GetFloat(parameter.name);
                    if (currentFloatValue != floatValue)
                    {
                        currentFloatValue = floatValue;
                        inputField.text = currentFloatValue.ToString(CultureInfo.InvariantCulture);
                    }
                    break;
            }
        }

        public void OnInputValueChanged(string value)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Int:
                    if (int.TryParse(value, out var intValue))
                    {
                        currentIntValue = intValue;
                        animator.SetInteger(parameter.name, intValue);
                    }
                    break;
                case AnimatorControllerParameterType.Float:
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatValue))
                    {
                        currentFloatValue = floatValue;
                        animator.SetFloat(parameter.name, floatValue);
                    }
                    break;
            }
        }

        public void OnInputEndEdit(string value)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Int:
                    if (!int.TryParse(value, out _))
                    {
                        inputField.text = parameter.defaultInt.ToString();
                    }
                    break;
                case AnimatorControllerParameterType.Float:
                    if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                    {
                        inputField.text = parameter.defaultFloat.ToString(CultureInfo.InvariantCulture);
                    }
                    break;
            }
        }

        public void OnToggle(bool value)
        {
            currentBoolValue = value;
            animator.SetBool(parameter.name, currentBoolValue);
        }
    }
}
