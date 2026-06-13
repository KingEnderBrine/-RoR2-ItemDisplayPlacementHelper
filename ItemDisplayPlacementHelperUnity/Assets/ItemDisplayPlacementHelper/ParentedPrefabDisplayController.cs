using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HG;
using IDRSJsonLoader;
using ItemDisplayPlacementHelper.Assets.ItemDisplayPlacementHelper;
using ItemDisplayPlacementHelper.AxisEditing;
using ItemDisplayPlacementHelper.Dialogs;
using ItemDisplayPlacementHelper.Editable;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ChildLocator;

namespace ItemDisplayPlacementHelper
{
    public class ParentedPrefabDisplayController : MonoBehaviour
    {
        private static readonly Regex placeholderRegex = new Regex(@"(?<!\{)\{((?<modificator>.??):)?(?<field>[^\{\}:]*?)(\.(?<subfield>.*?))?(:(?<precision>.*?))?\}(?!\})", RegexOptions.Compiled);

        public TMP_Dropdown ruleTypeDropdown;
        public MultipleOptionsDropdown limbMaskDropdown;
        public Button selectPrefabButton;
        public TMP_Text prefabText;
        public TMP_Dropdown childNameDropdown;
        public SearchableDropdown childPathDropdown;

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
        public EditableItemDisplayRule ItemDisplayRule { get; private set; }

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

            ruleTypeDropdown.AddOptions(Enum.GetNames(typeof(ItemDisplayRuleType)).Select(name => new TMP_Dropdown.OptionData(name)).ToList());
            formatDropdown.AddOptions(Enum.GetNames(typeof(CopyFormat)).Select(name => new TMP_Dropdown.OptionData(name)).ToList());

            var limbMaskNames = Enum.GetNames(typeof(LimbFlags));
            var limbMaskValues = Enum.GetValues(typeof(LimbFlags)) as IList;
            var limbMaskOptions = new List<MultipleOptionsDropdown.OptionData>();
            for (var i = 0; i < limbMaskNames.Length; i++)
            {
                var value = (int)limbMaskValues[i];
                if (!Utils.IsPowerOfTwo(value))
                {
                    continue;
                }
                limbMaskOptions.Add(new MultipleOptionsDropdown.OptionData(limbMaskNames[i], null, value));
            }
            limbMaskDropdown.AddOptions(limbMaskOptions);
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
            childPathDropdown.ClearOptions();
        }

        private void OnModelChanged(CharacterModel characterModel)
        {
            this.characterModel = characterModel;

            if (characterModel && characterModel.childLocator)
            {
                childNameDropdown.options.AddRange(characterModel.childLocator.transformPairs.Select(el => new TMP_Dropdown.OptionData(el.name)).ToList());
                BuildChildPathDropdownOptions(ModelPicker.Instance.ModelInfo.modelPrefab.transform);
            }
        }

        private void BuildChildPathDropdownOptions(Transform parent, string parentPath = null)
        {
            var n = parent.childCount;
            for (var i = 0; i < n; i++)
            {
                var child = parent.GetChild(i);
                var childPath = parentPath is null ? child.name : $"{parentPath}/{child.name}";
                childPathDropdown.Options.Add(new SearchableDropdown.OptionData(childPath, childPath));
                BuildChildPathDropdownOptions(child, childPath);
            }
        }

        private void OnDisplayRuleGroupChanged(EditableDisplayRuleGroup keyAssetRuleGroup)
        {
            SetItemDisplayRule(null);
        }

        private void Update()
        {
            var ruleExists = ItemDisplayRule is not null;
            bool instanceExists = ItemDisplayRule?.instance;

            ruleTypeDropdown.interactable = ruleExists;

            limbMaskDropdown.interactable = ItemDisplayRule?.ruleType == ItemDisplayRuleType.LimbMask;

            var isParentedPrefab = ItemDisplayRule?.ruleType == ItemDisplayRuleType.ParentedPrefab;

            selectPrefabButton.interactable = isParentedPrefab;
            childNameDropdown.interactable = isParentedPrefab;
            childPathDropdown.interactable = isParentedPrefab;

            localPosInput.interactable = instanceExists;
            localAnglesInput.interactable = instanceExists;
            localScaleInput.interactable = instanceExists;
            
            copyToClipboardButton.interactable = ruleExists;
            editCopyFormatButton.interactable = ruleExists;
            if (!ruleExists)
            {
                editFormatContainer.SetActive(false);
            }

            if (!instanceExists)
            {
                return;
            }

            localPosInput.SetValueWithoutNotify(ItemDisplayRule.instance.transform.localPosition);
            localAnglesInput.SetValueWithoutNotify(ItemDisplayRule.instance.transform.localEulerAngles);
            localScaleInput.SetValueWithoutNotify(ItemDisplayRule.instance.transform.localScale);

            ItemDisplayRule.localPos = localPosInput.CurrentValue;
            ItemDisplayRule.localAngles = localAnglesInput.CurrentValue;
            ItemDisplayRule.localScale = localScaleInput.CurrentValue;
        }

        private void ClearValues()
        {
            prefabText.text = "No prefab";

            ruleTypeDropdown.SetValueWithoutNotify(-1);
            ruleTypeDropdown.RefreshShownValue();

            limbMaskDropdown.SetValueWithoutNotify(0);
            limbMaskDropdown.RefreshShownValue();

            childNameDropdown.SetValueWithoutNotify(-1);
            childNameDropdown.RefreshShownValue();

            localPosInput.ClearValues();
            localAnglesInput.ClearValues();
            localScaleInput.ClearValues();
        }

        public void SetItemDisplayRule(EditableItemDisplayRule itemDisplayRule)
        {
            if (ItemDisplayRule == itemDisplayRule)
            {
                return;
            }

            ClearValues();

            ItemDisplayRule = itemDisplayRule;
            EditorAxisController.Instance.SetSelectedObject(ItemDisplayRule?.instance ? ItemDisplayRule.instance.transform : null);

            if (ItemDisplayRule is null)
            {
                return;
            }

            ruleTypeDropdown.SetValueWithoutNotify((int)itemDisplayRule.ruleType);
            ruleTypeDropdown.RefreshShownValue();

            limbMaskDropdown.SetValueWithoutNotify((int)itemDisplayRule.limbMask);
            limbMaskDropdown.RefreshShownValue();

            prefabText.text = itemDisplayRule.followerPrefab ? itemDisplayRule.followerPrefab.name : "No prefab";
            var displayTransform = itemDisplayRule.instance?.transform;
            if (displayTransform)
            {
                var childName = characterModel.childLocator.transformPairs.FirstOrDefault(el => el.transform == displayTransform.parent).name;
                childNameDropdown.SetValueWithoutNotify(childNameDropdown.options.FindIndex(el => el.text == childName));
                childNameDropdown.RefreshShownValue();

                localPosInput.SetValueWithoutNotify(displayTransform.localPosition, true);
                localAnglesInput.SetValueWithoutNotify(displayTransform.localEulerAngles, true);
                localScaleInput.SetValueWithoutNotify(displayTransform.localScale, true);
            }
            else
            {
                childNameDropdown.SetValueWithoutNotify(childNameDropdown.options.FindIndex(el => el.text == ItemDisplayRule.childName));
                childNameDropdown.RefreshShownValue();

                localPosInput.SetValueWithoutNotify(ItemDisplayRule.localPos, true);
                localAnglesInput.SetValueWithoutNotify(ItemDisplayRule.localAngles, true);
                localScaleInput.SetValueWithoutNotify(ItemDisplayRule.localScale, true);
            }
        }

        public void SelectRuleType(int index)
        {
            ItemDisplayRule.ruleType = (ItemDisplayRuleType)index;
            switch ((ItemDisplayRuleType)index)
            {
                case ItemDisplayRuleType.ParentedPrefab:
                    characterModel.limbFlagSet.RemoveFlags(ItemDisplayRule.limbMask);
                    ItemDisplayRule.TrySpawnInstance(characterModel);
                    break;
                case ItemDisplayRuleType.LimbMask:
                    Destroy(ItemDisplayRule.instance);
                    ItemDisplayRule.instance = null;
                    characterModel.limbFlagSet.AddFlags(ItemDisplayRule.limbMask);
                    break;
            }
            characterModel.materialsDirty = true;
            EditorAxisController.Instance.SetSelectedObject(ItemDisplayRule?.instance ? ItemDisplayRule.instance.transform : null);
        }

        public void SelectLimbMask(int value)
        {
            characterModel.limbFlagSet.RemoveFlags(ItemDisplayRule.limbMask);
            ItemDisplayRule.limbMask = (LimbFlags)value;
            characterModel.limbFlagSet.AddFlags(ItemDisplayRule.limbMask);
            characterModel.materialsDirty = true;
        }

        public void SelectPrefab()
        {
            ItemDisplayRuleSetController.Instance.FillPrefabReference(ItemDisplayRule, true);
            DialogController.ShowPrefabPicker(ItemDisplayRule.followerPrefabAddress?.AssetGUID, ItemDisplayRule.assetBundle, ItemDisplayRule.assetPath, (prefab, reference, assetBundle, path) =>
            {
                if (ItemDisplayRule.instance)
                {
                    Destroy(ItemDisplayRule.instance);
                    ItemDisplayRule.instance = null;
                }

                ItemDisplayRule.followerPrefab = prefab;
                ItemDisplayRule.followerPrefabAddress = reference;
                ItemDisplayRule.assetBundle = assetBundle;
                ItemDisplayRule.assetPath = path;
                ItemDisplayRule.referenceDiscovered = true;
                ItemDisplayRule.TrySpawnInstance(characterModel);
                prefabText.text = ItemDisplayRule.followerPrefab.name;
                EditorAxisController.Instance.SetSelectedObject(ItemDisplayRule?.instance ? ItemDisplayRule.instance.transform : null);
            });
        }

        public void OnLocalPosChanged(Vector3 value)
        {
            if (!ItemDisplayRule.instance)
            {
                return;
            }
            ItemDisplayRule.instance.transform.localPosition = value;
        }

        public void OnLocalAnglesChanged(Vector3 value)
        {
            if (!ItemDisplayRule.instance)
            {
                return;
            }
            ItemDisplayRule.instance.transform.localEulerAngles = value;
        }

        public void OnLocalScaleChanged(Vector3 value)
        {
            if (!ItemDisplayRule.instance)
            {
                return;
            }
            ItemDisplayRule.instance.transform.localScale = value;
        }

        public void SelectChild(int index)
        {
            var pair = characterModel.childLocator.transformPairs[index];
            var child = pair.transform;
            ItemDisplayRule.childName = pair.name;
            ItemDisplayRule.TrySpawnInstance(characterModel);
            if (ItemDisplayRule.followerPrefab)
            {
                prefabText.text = ItemDisplayRule.followerPrefab.name;
            }
            if (ItemDisplayRule.instance)
            {
                ItemDisplayRule.instance.transform.SetParent(child, false);
                EditorAxisController.Instance.SetSelectedObject(ItemDisplayRule.instance.transform);
            }
        }

        public void AddNewChild(object childPathObj)
        {
            var childPath = childPathObj as string;
            var model = ModelPicker.Instance.CharacterModel;
            var childName = StringHelpers.ChildNameFromPath(childPath);
            var locator = model.childLocator;
            var childIndex = locator.FindChildIndex(childName);
            if (childIndex < 0)
            {
                childIndex = locator.transformPairs.Length;
                var child = model.transform.Find(childPath);
                ArrayUtils.ArrayAppend(ref locator.transformPairs, new NameTransformPair
                {
                    name = childName,
                    transform = child
                });
                childNameDropdown.options.Add(new TMP_Dropdown.OptionData(childName));
            }

            childNameDropdown.value = childIndex;
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
                ItemDisplayRuleSetController.Instance.FillPrefabReference(ItemDisplayRule, true);

                var text = GetText();

                var result = ReplacePlaceHolders(text);

                GUIUtility.systemCopyBuffer = result;
            }
            catch (Exception e)
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
            var matches = placeholderRegex.Matches(text);

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
                    return TemplateHelpers.StringText(ItemDisplayRule.childName, modificator);
                case "localPos":
                    return TemplateHelpers.Vector3Text(ItemDisplayRule.localPos, precision, modificator, subField);
                case "localAngles":
                    return TemplateHelpers.Vector3Text(ItemDisplayRule.localAngles, precision, modificator, subField);
                case "localScale":
                    return TemplateHelpers.Vector3Text(ItemDisplayRule.localScale, precision, modificator, subField);
                case "modelName":
                    return TemplateHelpers.StringText(ModelPicker.Instance.ModelInfo.modelName, modificator);
                case "bodyName":
                    return TemplateHelpers.StringText(ModelPicker.Instance.ModelInfo.bodyName, modificator);
                case "itemName":
                    return TemplateHelpers.StringText(DisplayRuleGroupEditingController.Instance.DisplayRuleGroup.KeyAsset.name, modificator);
                case "objectName":
                    return TemplateHelpers.StringText(ItemDisplayRule.followerPrefab?.name, modificator);
                case "ruleType":
                    return TemplateHelpers.EnumValue(ItemDisplayRule.ruleType, modificator);
                case "limbMask":
                    return TemplateHelpers.EnumFlagsValue(ItemDisplayRule.limbMask, modificator);
                case "guid":
                    return TemplateHelpers.StringText(ItemDisplayRule.followerPrefabAddress?.AssetGUID, modificator);
                case "assetBundle":
                    return TemplateHelpers.StringText(ItemDisplayRule.assetBundle, modificator);
                case "assetPath":
                    return TemplateHelpers.StringText(ItemDisplayRule.assetPath, modificator);
                case "skinName":
                {
                    var controller = ModelPicker.Instance.ModelSkinController;
                    var skinDef = HG.ArrayUtils.GetSafe(controller.skins, controller.currentSkinIndex);
                    var skinName = skinDef ? skinDef.name : "";
                    return TemplateHelpers.StringText(skinName, modificator);
                }
                default:
                    throw new ArgumentException($"Failed to parse placeholder {match.Value}");
            }
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
