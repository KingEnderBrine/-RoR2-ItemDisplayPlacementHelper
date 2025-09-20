using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ItemDisplayPlacementHelper.AxisEditing;
using RoR2;
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
        public Button editCopyFormatButton;

        [Space]
        public GameObject editFormatContainer;
        public TMP_InputField customFormatInput;
        public TMP_Text formatPreviewText;
        public TMP_Dropdown formatDropdown;

        private CharacterModel characterModel;
        public CharacterModel.ParentedPrefabDisplay ParentedPrefabDisplay { get; private set; }
        public ItemDisplayRule ItemDisplayRule { get; private set; }

        public static ParentedPrefabDisplayController Instance { get; private set; }

        private CopyFormat _copyFormat;
        public CopyFormat CopyFormat
        {
            get => _copyFormat;
            set
            {
                if (formatDropdown.value == (int)value)
                {
                    SelectFormat((int)value);
                }
                else
                {
                    formatDropdown.value = (int)value;
                }
            }
        }

        private void Awake()
        {
            Instance = this;

            localPosInput.onValueChanged += OnLocalPosChanged;
            localAnglesInput.onValueChanged += OnLocalAnglesChanged;
            localScaleInput.onValueChanged += OnLocalScaleChanged;

            ModelPicker.OnModelChanged += OnModelChanged;
            ModelPicker.OnModelWillChange += OnModelWillChange;
            DisplayRuleGroupEditingController.OnDisplayRuleGroupChanged += OnDisplayRuleGroupChanged;

            formatDropdown.AddOptions(Enum.GetNames(typeof(CopyFormat)).Select(name => new TMP_Dropdown.OptionData(name)).ToList());
        }

        private void OnDestroy()
        {            
            ModelPicker.OnModelChanged -= OnModelChanged;
            ModelPicker.OnModelWillChange -= OnModelWillChange;
            DisplayRuleGroupEditingController.OnDisplayRuleGroupChanged -= OnDisplayRuleGroupChanged;

            Instance = null;
        }

        private void OnModelWillChange()
        {
            ClearValues();
            childNameDropdown.ClearOptions();
        }

        private void OnModelChanged(CharacterModel characterModel)
        {
            this.characterModel = characterModel;

            if (characterModel && characterModel.childLocator)
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
            editCopyFormatButton.interactable = instanceExists;

            if (!instanceExists)
            {
                editFormatContainer.SetActive(false);
            }

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

        public void SelectFormat(int format)
        {
            _copyFormat = (CopyFormat)format;
            
            customFormatInput.gameObject.SetActive(CopyFormat == CopyFormat.Custom);
            formatPreviewText.transform.parent.parent.gameObject.SetActive(CopyFormat != CopyFormat.Custom);

            formatPreviewText.text = GetText();
        }

        public void CopyValuesToClipboard()
        {
            try
            {
                var text = GetText();

                var result = ReplacePlaceHolders(text);

                GUIUtility.systemCopyBuffer = result;
            }
            catch(Exception e)
            {
                DialogController.ShowError(e.Message);
            }
        }

        public void ToogleEditFormatContainerVisibility()
        {
            editFormatContainer.SetActive(!editFormatContainer.activeSelf);
        }

        private string ReplacePlaceHolders(string text)
        {
            var matches = Regex.Matches(text, @"(?<!\{)\{((?<modificator>.??):)?(?<field>[^\{\}:]*?)(\.(?<subfield>.*?))?(:(?<precision>.*?))?\}(?!\})");

            if (matches.Count == 0)
            {
                return text;
            }

            var builder = new StringBuilder();
            var textIndex = 0;
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                builder.Append(text.Substring(textIndex, match.Index - textIndex).Replace("{{", "{").Replace("}}", "}"));
                textIndex = match.Index + match.Length;
                builder.Append(ParsePlaceHolder(match));
            }

            if (matches.Count == 0)
            {
                return text;
            }
            else if (textIndex != text.Length)
            {
                builder.Append(text.Substring(textIndex, text.Length - textIndex).Replace("{{", "{").Replace("}}", "}"));
            }

            return builder.ToString();
        }

        private string ParsePlaceHolder(Match match)
        {
            var modificator = match.Groups["modificator"].Value?.ToLower();
            var field = match.Groups["field"].Value;
            var subField = match.Groups["subfield"].Value;
            var precision = 5;
            if (match.Groups["precision"].Success && !int.TryParse(match.Groups["precision"].Value, out precision))
            {
                throw new ArgumentException($"Failed to parse placeholder {match.Value}");
            }

            switch (field)
            {
                case "childName":
                    return StringText(childNameDropdown.captionText.text, modificator);
                case "localPos":
                    return Vector3Text(localPosInput.CurrentValue, precision, modificator, subField);
                case "localAngles":
                    return Vector3Text(localAnglesInput.CurrentValue, precision, modificator, subField);
                case "localScale":
                    return Vector3Text(localScaleInput.CurrentValue, precision, modificator, subField);
                case "modelName":
                    return StringText(ModelPicker.Instance.ModelInfo.modelName, modificator);
                case "bodyName":
                    return StringText(ModelPicker.Instance.ModelInfo.bodyName, modificator);
                case "itemName":
                    return StringText(GetItemName(), modificator);
                case "objectName":
                    return StringText(ParentedPrefabDisplay.prefabReference.Result?.name ?? "", modificator);
                default:
                    throw new ArgumentException($"Failed to parse placeholder {match.Value}");
            }
        }

        private string GetItemName()
        {
            if (ParentedPrefabDisplay.itemIndex != ItemIndex.None)
            {
                return ItemCatalog.GetItemDef(ParentedPrefabDisplay.itemIndex)?.name;
            }
            if (ParentedPrefabDisplay.equipmentIndex != EquipmentIndex.None)
            {
                return EquipmentCatalog.GetEquipmentDef(ParentedPrefabDisplay.equipmentIndex)?.name;
            }

            return "";
        }

        private string GetText()
        {
            switch (CopyFormat)
            {
                case CopyFormat.Custom:
                    return customFormatInput.text;
                case CopyFormat.Block:
                    return blockFormat;
                case CopyFormat.Inline:
                    return inlineFormat;
                case CopyFormat.ForParsing:
                    return forParsing;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        private string StringText(string str, string modificator)
        {
            switch (modificator)
            {
                case "r":
                    return str;
                default:
                    return $@"""{str}""";
            }
        }

        private string Vector3Text(Vector3 vector, int precision, string modificator, string subField)
        {
            bool raw = modificator == "r";
            switch (subField)
            {
                case "x":
                    return FloatInvariant(vector.x, precision) + (raw ? "" : "F");
                case "y":
                    return FloatInvariant(vector.y, precision) + (raw ? "" : "F");
                case "z":
                    return FloatInvariant(vector.z, precision) + (raw ? "" : "F");
            }
            return $"new Vector3({FloatInvariant(vector.x, precision)}F, {FloatInvariant(vector.y, precision)}F, {FloatInvariant(vector.z, precision)}F)";
        }

        private string FloatInvariant(float num, int precision)
        {
            return num.ToString($"0.{"".PadLeft(precision, '#')}", CultureInfo.InvariantCulture);
        }

        private const string blockFormat =
@"childName = {childName},
localPos = {localPos:5},
localAngles = {localAngles:5},
localScale = {localScale:5}
";
        private const string inlineFormat = @"{childName}, {localPos:5}, {localAngles:5}, {localScale:5}";
        private const string forParsing = @"{r:childName},{r:localPos.x:5},{r:localPos.y:5},{r:localPos.z:5},{r:localAngles.x:5},{r:localAngles.y:5},{r:localAngles.z:5},{r:localScale.x:5},{r:localScale.y:5},{r:localScale.z:5}";
    }
}
