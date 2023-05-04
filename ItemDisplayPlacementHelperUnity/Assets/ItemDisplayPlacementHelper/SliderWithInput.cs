using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper
{
    public class SliderWithInput : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField input;
        [SerializeField]
        private Slider slider;

        [SerializeField]
        private float min;
        public float Min 
        {
            get => min;
            set
            {
                if (min == value)
                {
                    return;
                }
                min = value;

                skipAllNotifications = true;
                slider.minValue = min;
                skipAllNotifications = false;

                Value = Value;
            }
        }
        [SerializeField]
        private float max;
        public float Max
        {
            get => max;
            set
            {
                if (max == value)
                {
                    return;
                }
                max = value;
                
                skipAllNotifications = true;
                slider.maxValue = max;
                skipAllNotifications = false;

                Value = Value;
            }
        }
        [SerializeField]
        private float value;
        public float Value
        {
            get => value;
            set
            {
                var clampedValue = Mathf.Clamp(value, Min, Max);
                if (this.value == clampedValue)
                {
                    return;
                }
                this.value = clampedValue;

                if (skipAllNotifications)
                {
                    return;
                }
                if (!skipComponentsNotifications)
                {
                    skipAllNotifications = true;
                    slider.value = this.value;
                    input.SetTextWithoutNotify(this.value.ToString(CultureInfo.InvariantCulture));
                    skipAllNotifications = false;
                }

                OnValueChanged?.Invoke(clampedValue);
            }
        }

        public delegate void OnValueChangedHandler(float newValue);
        public event OnValueChangedHandler OnValueChanged;

        private bool skipAllNotifications;
        private bool skipComponentsNotifications;

        private void Start()
        {
            skipAllNotifications = true;

            value = Mathf.Clamp(Value, Min, Max);

            slider.minValue = Min;
            slider.maxValue = Max;
            slider.value = Value;

            input.SetTextWithoutNotify(value.ToString(CultureInfo.InvariantCulture));

            skipAllNotifications = false;
        }

        public void SliderValueChange(float value)
        {
            if (skipAllNotifications || skipComponentsNotifications)
            {
                return;
            }
            skipComponentsNotifications = true;
            Value = value;
            input.SetTextWithoutNotify(value.ToString(CultureInfo.InvariantCulture));
            skipComponentsNotifications = false;

        }

        public void InputValueChange(string value)
        {
            if (skipAllNotifications || skipComponentsNotifications)
            {
                return;
            }
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var num))
            {
                return;
            }

            skipComponentsNotifications = true;
            Value = num;
            if (Value != num)
            {
                input.SetTextWithoutNotify(num.ToString(CultureInfo.InvariantCulture));
            }
            slider.value = num;
            skipComponentsNotifications = false;
        }

        public void InputEndEdit(string newValue)
        {
            if (!float.TryParse(newValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var num))
            {
                Value = Min;
            }
        }
    }
}
