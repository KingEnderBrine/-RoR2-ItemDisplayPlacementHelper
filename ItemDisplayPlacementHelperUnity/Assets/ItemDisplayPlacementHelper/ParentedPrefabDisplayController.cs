using ItemDisplayPlacementHelper.AxisEditing;
using RoR2;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper
{
    public class ParentedPrefabDisplayController : MonoBehaviour
    {
        public TMP_Dropdown childNameDropdown;

        [Space]
        public Vector3InputField localPosInput;
        public Vector3InputField localAnglesInput;
        public Vector3InputField localScaleInput;

        [Space]
        public Button copyToClipboardButton;
        public TMP_InputField precisionInput;

        private CharacterModel characterModel;
        public CharacterModel.ParentedPrefabDisplay ParentedPrefabDisplay { get; private set; }
        public ItemDisplayRule ItemDisplayRule { get; private set; }

        public static ParentedPrefabDisplayController Instance { get; private set; }

        public int CurrentPrecision { get; private set; } = 4;

        private void Awake()
        {
            Instance = this;

            localPosInput.onValueChanged += OnLocalPosChanged;
            localAnglesInput.onValueChanged += OnLocalAnglesChanged;
            localScaleInput.onValueChanged += OnLocalScaleChanged;

            ModelPicker.OnModelChanged += OnModelChanged;
            DisplayRuleGroupEditingController.OnDisplayRuleGroupChanged += OnDisplayRuleGroupChanged;
        }

        private void OnDestroy()
        {            
            ModelPicker.OnModelChanged -= OnModelChanged;
            DisplayRuleGroupEditingController.OnDisplayRuleGroupChanged -= OnDisplayRuleGroupChanged;

            Instance = null;
        }

        private void OnModelChanged(CharacterModel characterModel)
        {
            this.characterModel = characterModel;

            ClearValues();

            if (characterModel)
            {
                childNameDropdown.options.AddRange(characterModel.childLocator.transformPairs.Select(el => new TMP_Dropdown.OptionData(el.name)).ToList());
            }
        }

        private void OnDisplayRuleGroupChanged(DisplayRuleGroup displayRuleGroup)
        {
            SetItemDisplayRule(default, default);
        }

        private void Update()
        {
            bool instanceExists = ParentedPrefabDisplay.instance;
            childNameDropdown.interactable = instanceExists;

            localPosInput.interactable = instanceExists;
            localAnglesInput.interactable = instanceExists;
            localScaleInput.interactable = instanceExists;
            
            copyToClipboardButton.interactable = instanceExists;
            precisionInput.interactable = instanceExists;

            if (!ParentedPrefabDisplay.instance)
            {
                
                return;
            }

            localPosInput.SetValueWithoutNotify(ParentedPrefabDisplay.instance.transform.localPosition);
            localAnglesInput.SetValueWithoutNotify(ParentedPrefabDisplay.instance.transform.localEulerAngles);
            localScaleInput.SetValueWithoutNotify(ParentedPrefabDisplay.instance.transform.localScale);
        }

        private void ClearValues()
        {
            childNameDropdown.captionText.text = "";

            localPosInput.ClearValues();
            localAnglesInput.ClearValues();
            localScaleInput.ClearValues();
        }

        public void SetItemDisplayRule(ItemDisplayRule itemDisplayRule, CharacterModel.ParentedPrefabDisplay parentedPrefabDisplay)
        {
            if (ItemDisplayRule.Equals(itemDisplayRule) && ParentedPrefabDisplay.Equals(parentedPrefabDisplay))
            {
                return;
            }

            ClearValues();

            ParentedPrefabDisplay = parentedPrefabDisplay;
            ItemDisplayRule = itemDisplayRule;

            EditorAxisController.Instance.SetSelectedObject(ParentedPrefabDisplay.instance ? ParentedPrefabDisplay.instance.transform : null);

            if (!ParentedPrefabDisplay.instance)
            {
                return;
            }

            var displayTransform = parentedPrefabDisplay.instance.transform;

            childNameDropdown.captionText.text = characterModel.childLocator.transformPairs.FirstOrDefault(el => el.transform == displayTransform.parent).name;
            childNameDropdown.SetValueWithoutNotify(childNameDropdown.options.FindIndex(el => el.text == childNameDropdown.captionText.text));

            localPosInput.SetValueWithoutNotify(displayTransform.localPosition, true);
            localAnglesInput.SetValueWithoutNotify(displayTransform.localEulerAngles, true);
            localScaleInput.SetValueWithoutNotify(displayTransform.localScale, true);
        }

        public void OnLocalPosChanged(Vector3 value)
        {
            if (!ParentedPrefabDisplay.instance)
            {
                return;
            }
            ParentedPrefabDisplay.instance.transform.localPosition = value;
        }

        public void OnLocalAnglesChanged(Vector3 value)
        {
            if (!ParentedPrefabDisplay.instance)
            {
                return;
            }
            ParentedPrefabDisplay.instance.transform.localEulerAngles = value;
        }

        public void OnLocalScaleChanged(Vector3 value)
        {
            if (!ParentedPrefabDisplay.instance)
            {
                return;
            }
            ParentedPrefabDisplay.instance.transform.localScale = value;
        }

        public void SelectChild(int index)
        {
            if (!ParentedPrefabDisplay.instance)
            {
                return;
            }

            var child = characterModel.childLocator.transformPairs[index].transform;
            ParentedPrefabDisplay.instance.transform.SetParent(child, false);
        }

        public void PrecisionValueChanged(string value)
        {
            if (!int.TryParse(value, out var num))
            {
                precisionInput.SetTextWithoutNotify("");
            }
            else if (num < 0)
            {
                CurrentPrecision = Mathf.Abs(num);
                precisionInput.SetTextWithoutNotify(CurrentPrecision.ToString());
            }
            else
            {
                CurrentPrecision = num;
            }
        }

        public void PrecisionEndEdit(string value)
        {
            if (!int.TryParse(value, out var num))
            {
                precisionInput.SetTextWithoutNotify("0");
                CurrentPrecision = 0;
            }
        }

        public void CopyValuesToClipboard()
        {
            var childName = childNameDropdown.captionText.text;
            var localPos = localPosInput.CurrentValue;
            var localAngles = localAnglesInput.CurrentValue;
            var localScale = localScaleInput.CurrentValue;

            var text = $@"childName = ""{childName}"",
localPos = {NewVector3Text(localPos)},
localAngles = {NewVector3Text(localAngles)},
localScale = {NewVector3Text(localScale)}";

            GUIUtility.systemCopyBuffer = text;
        }

        private string NewVector3Text(Vector3 vector)
        {
            return $"new Vector3({FloatInvariant(vector.x)}F, {FloatInvariant(vector.y)}F, {FloatInvariant(vector.z)}F)";
        }

        private string FloatInvariant(float num)
        {
            return num.ToString($"0.{"".PadLeft(CurrentPrecision, '#')}", CultureInfo.InvariantCulture);
        }
    }
}
