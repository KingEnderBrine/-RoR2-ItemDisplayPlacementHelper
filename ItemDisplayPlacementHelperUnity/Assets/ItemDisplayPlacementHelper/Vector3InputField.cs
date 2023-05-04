using RoR2;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;

namespace ItemDisplayPlacementHelper
{
    public class Vector3InputField : MonoBehaviour
    {
        public TMP_InputField xInput;
        public TMP_InputField yInput;
        public TMP_InputField zInput;

        public bool interactable;

        public delegate void OnChangeEvent(Vector3 value);
        public OnChangeEvent onValueChanged;

        private Vector3 _currentValue;
        public Vector3 CurrentValue
        {
            get => _currentValue;
            set
            {
                if (_currentValue == value)
                {
                    return;
                }
                if (SetValueWithoutNotifyInternal(value, false))
                {
                    onValueChanged?.Invoke(value);
                }
            }
        }

        private void Update()
        {
            xInput.interactable = interactable;
            yInput.interactable = interactable;
            zInput.interactable = interactable;
        }

        public void ClearValues()
        {
            _currentValue = Vector3.zero;

            xInput.SetTextWithoutNotify("");
            yInput.SetTextWithoutNotify("");
            zInput.SetTextWithoutNotify("");
        }

        public void OnValueChangedX(string value) => OnValueChanged(value, Axis.X);
        public void OnValueChangedY(string value) => OnValueChanged(value, Axis.Y);
        public void OnValueChangedZ(string value) => OnValueChanged(value, Axis.Z);

        private void OnValueChanged(string value, Axis axis)
        {
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                return;
            }
            switch (axis)
            {
                case Axis.X:
                    _currentValue.x = number;
                    break;
                case Axis.Y:
                    _currentValue.y = number;
                    break;
                case Axis.Z:
                    _currentValue.z = number;
                    break;
                default:
                    return;
            }
            onValueChanged?.Invoke(CurrentValue);
        }

        public void OnEndEditX(string value) => OnEditEnd(value, Axis.X);
        public void OnEndEditY(string value) => OnEditEnd(value, Axis.Y);
        public void OnEndEditZ(string value) => OnEditEnd(value, Axis.Z);
        
        private void OnEditEnd(string value, Axis axis)
        {
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                return;
            }
            switch (axis)
            {
                case Axis.X:
                    xInput.text = "0";
                    break;
                case Axis.Y:
                    yInput.text = "0";
                    break;
                case Axis.Z:
                    zInput.text = "0";
                    break;
            }
        }

        public void SetValueWithoutNotify(Vector3 value, bool forceUpdate = false) => SetValueWithoutNotifyInternal(value, forceUpdate);
        private bool SetValueWithoutNotifyInternal(Vector3 value, bool forceUpdate)
        {
            var newValue = new Vector3(_currentValue.x, _currentValue.y, _currentValue.z);
            if (forceUpdate || (_currentValue.x != value.x && !xInput.isFocused))
            {
                xInput.SetTextWithoutNotify(value.x.ToString(CultureInfo.InvariantCulture));
                newValue.x = value.x;
            }
            if (forceUpdate || (_currentValue.y != value.y && !yInput.isFocused))
            {
                yInput.SetTextWithoutNotify(value.y.ToString(CultureInfo.InvariantCulture));
                newValue.y = value.y;
            }
            if (forceUpdate || (_currentValue.z != value.z && !zInput.isFocused))
            {
                zInput.SetTextWithoutNotify(value.z.ToString(CultureInfo.InvariantCulture));
                newValue.z = value.z;
            }

            if (_currentValue == newValue)
            {
                return false;
            }

            _currentValue = newValue;

            return true;
        }
    }
}
