using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HG;
using IDRSJsonLoader;
using IDRSJsonLoader.Models;
using ItemDisplayPlacementHelper.Dialogs;
using ItemDisplayPlacementHelper.Editable;
using ItemDisplayPlacementHelper.Native;
using RoR2;
using RoR2.AddressableAssets;
using RoR2.ContentManagement;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.UI;
using static ChildLocator;

namespace ItemDisplayPlacementHelper
{
    public class ItemDisplayRuleSetController : MonoBehaviour
    {
        public GameObject rowPrefab;
        public Transform container;

        [Space]
        public Button enableAllButton;
        public Button disableAllButton;
        public TMP_InputField searchInput;
        public TMP_Dropdown showItemsMode;

        [Space]
        public Button addButton;
        public Button deleteButton;
        public Button importButton;
        public Button exportButton;

        private readonly List<DisplayRuleGroupPreviewController> previewRows = new List<DisplayRuleGroupPreviewController>();
        private readonly HashSet<UnityEngine.Object> notPickedAssets = new HashSet<UnityEngine.Object>();

        private CharacterModel characterModel;
        private EditableItemDisplayRuleSet itemDisplayRuleSet;

        private string filter;

        private Dictionary<string, string> pathToGuid;
        private Dictionary<string, AssetBundle> dirToBundle;
        private Dictionary<AssetBundle, Dictionary<int, string>> bundleToAssets;

        public static ItemDisplayRuleSetController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            showItemsMode.AddOptions(ContentManager.allLoadedContentPacks.Where(el => el.equipmentDefs.Any() || el.itemDefs.Any()).Select(el => el.identifier).ToList());
            ModelPicker.OnModelChanged += OnModelChanged;
            ModelPicker.OnModelWillChange += OnModelWillChange;
        }

        private void OnDestroy()
        {
            Instance = null;
            ModelPicker.OnModelChanged -= OnModelChanged;
            ModelPicker.OnModelWillChange -= OnModelWillChange;
        }

        private void Update()
        {
            enableAllButton.interactable = characterModel;
            disableAllButton.interactable = characterModel;
            searchInput.interactable = characterModel;
            showItemsMode.interactable = characterModel;

            addButton.interactable = characterModel;
            deleteButton.interactable = characterModel && DisplayRuleGroupEditingController.Instance.DisplayRuleGroup is not null;
            importButton.interactable = characterModel;
            exportButton.interactable = characterModel;
        }

        private void OnModelChanged(CharacterModel characterModel)
        {
            if (!characterModel)
            {
                return;
            }

            this.characterModel = characterModel;
            itemDisplayRuleSet = EditableItemDisplayRuleSet.From(characterModel.itemDisplayRuleSet);

            foreach (var itemDef in ItemCatalog.itemDefs)
            {
                notPickedAssets.Add(itemDef);
            }
            foreach (var equipmentDef in EquipmentCatalog.equipmentDefs)
            {
                notPickedAssets.Add(equipmentDef);
            }

            foreach (var displayRuleGroup in itemDisplayRuleSet.DisplayRuleGroups)
            {
                notPickedAssets.Remove(displayRuleGroup.KeyAsset);
                CreateRowInstance(displayRuleGroup);
            }
            ApplyFilter(filter);
        }

        private void OnModelWillChange()
        {
            foreach (var row in previewRows)
            {
                Destroy(row.gameObject);
            }
            previewRows.Clear();

            characterModel = null;
        }

        private void CreateRowInstance(EditableDisplayRuleGroup displayRuleGroup)
        {
            var row = Instantiate(rowPrefab, container);
            var controller = row.GetComponent<DisplayRuleGroupPreviewController>();
    
            controller.displayRuleGroup = displayRuleGroup;
            (controller.icon, controller.nameText) = GetItemInfo(displayRuleGroup.KeyAsset);
    
            row.SetActive(true);
            previewRows.Add(controller);
        }

        private (Sprite, string) GetItemInfo(UnityEngine.Object keyAsset)
        {
            if (keyAsset is ItemDef item)
            {
                return (item.pickupIconSprite, Language.GetString(item.nameToken));
            }

            if (keyAsset is EquipmentDef equipment)
            {
                return (equipment.pickupIconSprite, Language.GetString(equipment.nameToken));
            }

            return default;
        }

        public void ApplyFilter(string newFilter)
        {
            filter = newFilter;
            UpdateRowsVisibility();
        }

        public void ChangeShowMode(int value)
        {
            UpdateRowsVisibility();
        }

        public void EnableAll()
        {
            foreach (var row in previewRows)
            {
                if (row.gameObject.activeSelf)
                {
                    row.ToggleDisplay(true);
                }
            }
            characterModel.UpdateMaterials();
        }

        public void DisableAll()
        {
            foreach (var row in previewRows)
            {
                if (row.gameObject.activeSelf)
                {
                    row.ToggleDisplay(false);
                }
            }
        }

        public void Add()
        {
            DialogController.ShowKeyAssetPicker(notPickedAssets, (asset) =>
            {
                var group = new EditableDisplayRuleGroup();
                group.KeyAsset = asset;
                itemDisplayRuleSet.DisplayRuleGroups.Add(group);
                notPickedAssets.Remove(asset);
                CreateRowInstance(group);
                ApplyFilter(filter);
            });
        }

        public void Delete()
        {
            var group = DisplayRuleGroupEditingController.Instance.DisplayRuleGroup;
            for (var i = 0; i < previewRows.Count; i++)
            {
                var row = previewRows[i];
                if (row.displayRuleGroup == group)
                {
                    notPickedAssets.Add(group.KeyAsset);
                    row.ToggleDisplay(false);
                    itemDisplayRuleSet.DisplayRuleGroups.RemoveAt(i);
                    previewRows.RemoveAt(i);
                    Destroy(row.gameObject);
                    break;
                }
            }
        }

        public void Export()
        {
            DialogController.ShowExport((path, assetsToExport, generateClass, @namespace, withSkin) =>
            {
                try
                {
                    var anyAssetBundle = false;
                    var refreshBundles = true;
                    IEnumerable<DisplayRuleGroupPreviewController> rows;
                    switch (assetsToExport)
                    {
                        case AssetsToExport.All:
                            rows = previewRows;
                            break;
                        case AssetsToExport.Filtered:
                            rows = previewRows.Where(r => r.gameObject.activeSelf);
                            break;
                        case AssetsToExport.Enabled:
                            rows = previewRows.Where(r => r.gameObject.activeSelf && r.displayRuleGroup.Enabled);
                            break;
                        default:
                            throw new NotSupportedException();
                    }

                    var idrs = new ExportItemDisplayRuleSet();
                    idrs.bodyName = ModelPicker.Instance.ModelInfo.bodyName;
                    if (withSkin)
                    {
                        var controller = ModelPicker.Instance.ModelSkinController;
                        var skinDef = HG.ArrayUtils.GetSafe(controller.skins, controller.currentSkinIndex);
                        idrs.skinName = skinDef ? skinDef.name : "";
                    }
                    foreach (var row in rows)
                    {
                        var displayRuleGroup = row.displayRuleGroup;
                        var keyGroup = new ExportKeyAssetRuleGroup();
                        keyGroup.name = displayRuleGroup.KeyAsset.name;

                        foreach (var displayRule in displayRuleGroup.Rules)
                        {
                            var ruleGroup = new ExportItemDisplayRule();
                            if (FillPrefabReference(displayRule, refreshBundles))
                            {
                                refreshBundles = false;
                            }

                            ruleGroup.guid = displayRule.followerPrefabAddress?.AssetGUID;
                            ruleGroup.assetBundle = displayRule.assetBundle;
                            ruleGroup.assetPath = displayRule.assetPath;
                            ruleGroup.localScale = displayRule.localScale;
                            ruleGroup.localPos = displayRule.localPos;
                            ruleGroup.localAngles = displayRule.localAngles;
                            ruleGroup.childName = displayRule.childName;
                            if (!string.IsNullOrEmpty(displayRule.childName) && displayRule.childName.EndsWith(StringHelpers.NamePostfix))
                            {
                                var model = ModelPicker.Instance.CharacterModel;
                                var child = model.childLocator.FindChild(ruleGroup.childName);
                                if (child)
                                {
                                    ruleGroup.childPath = Utils.GetChildPath(model.transform, child);
                                }
                            }
                            ruleGroup.limbMask = displayRule.limbMask;
                            ruleGroup.ruleType = displayRule.ruleType;

                            if (!string.IsNullOrEmpty(ruleGroup.assetBundle))
                            {
                                anyAssetBundle = true;
                            }

                            keyGroup.rules.Add(ruleGroup);
                        }

                        if (displayRuleGroup.KeyAsset is ItemDef)
                        {
                            idrs.itemGroups.Add(keyGroup);
                        }
                        else if (displayRuleGroup.KeyAsset is EquipmentDef)
                        {
                            idrs.equipmentGroups.Add(keyGroup);
                        }
                    }

                    idrs.itemGroups.Sort((a, b) => StringComparer.Ordinal.Compare(a.name, b.name));
                    idrs.equipmentGroups.Sort((a, b) => StringComparer.Ordinal.Compare(a.name, b.name));

                    var json = JsonUtility.ToJson(idrs, true);
                    File.WriteAllText(path, json);

                    if (generateClass)
                    {
                        var @class = GenerateClass(idrs, @namespace, System.IO.Path.GetFileNameWithoutExtension(path), anyAssetBundle);
                        File.WriteAllText(System.IO.Path.ChangeExtension(path, ".cs"), @class);
                    }
                }
                catch (Exception ex)
                {
                    ItemDisplayPlacementHelperPlugin.InstanceLogger.LogError(ex);
                    DialogController.ShowError("Export failed, check log.");
                }
            });
        }

        private string GenerateClass(ExportItemDisplayRuleSet idrs, string @namespace, string @class, bool anyAssetBundle)
        {
            var sb = new StringBuilder();
            var i = 0;

            sb
                .A("using System;")
                .L(i).A("using System.Collections.Generic;")
                .L(i).A("using RoR2;")
                .L(i).A("using UnityEngine;")
                .L(i).A("using UnityEngine.AddressableAssets;")
                .L(0);
            if (!string.IsNullOrEmpty(@namespace))
            {
                sb
                    .L(i).A("namespace ").A(@namespace)
                    .StartBlock(ref i);
            }
            sb
                .L(i).A("public class ").A(@class)
                .StartBlock(ref i);
            if (anyAssetBundle)
            {
                sb
                    .L(i).A("private partial AssetBundle GetAssetBundle(string name);")
                    .L(0);
            }

            sb
                .L(i).A("public ItemDisplayRuleSet Create()")
                .StartBlock(ref i)
                .L(i).A("var idrs = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();")
                .L(i).A("ReplaceRules(idrs);")
                .L(i).A("return idrs;")
                .EndBlock(ref i)
                .L(0)
                .L(i).A("public void ReplaceRules(ItemDisplayRuleSet idrs)")
                .StartBlock(ref i)
                .L(i).A("var existingGroupsLength = idrs.keyAssetRuleGroups.Length;")
                .L(i).A("var newRules = new List<ItemDisplayRuleSet.KeyAssetRuleGroup>();");

            foreach (var group in idrs.itemGroups)
            {
                GenerateRuleText(sb, group, "Item");
            }
            foreach (var group in idrs.equipmentGroups)
            {
                GenerateRuleText(sb, group, "Equipment");
            }

            void GenerateRuleText(StringBuilder sb, ExportKeyAssetRuleGroup group, string type)
            {
                sb
                    .L(i).A(@$"AddOrReplaceGroup(new ItemDisplayRuleSet.KeyAssetRuleGroup")
                    .StartBlock(ref i)
                    .L(i).A("keyAsset = Get").A(type).A("Def(\"").A(group.name).A("\"),")
                    .L(i).A("displayRuleGroup =")
                    .StartBlock(ref i);

                if (group.rules.Count == 0)
                {
                    sb.L(i).A("rules = Array.Empty<ItemDisplayRule>()");
                }
                else
                {
                    sb
                        .L(i).A("rules = new[]")
                        .StartBlock(ref i);

                    foreach (var rule in group.rules)
                    {
                        switch (rule.ruleType)
                        {
                            case ItemDisplayRuleType.ParentedPrefab:
                            {
                                sb.L(i);
                                if (!string.IsNullOrEmpty(rule.guid))
                                {
                                    sb.A("ParentedPrefabAddressables(").A(TemplateHelpers.StringText(rule.guid, ""));
                                }
                                else
                                {
                                    sb.A("ParentedPrefabAssetBundle(GetAssetBundle(").A(TemplateHelpers.StringText(rule.assetBundle, "")).A("), ").A(TemplateHelpers.StringText(rule.assetPath, ""));
                                }

                                sb.A(", ").A(TemplateHelpers.StringText(rule.childName, "")).A(", ").A(TemplateHelpers.Vector3Text(rule.localPos, 10, "", "")).A(", ").A(TemplateHelpers.Vector3Text(rule.localAngles, 10, "", "")).A(", ").A(TemplateHelpers.Vector3Text(rule.localScale, 10, "", "")).A("),");
                                break;
                            }
                            case ItemDisplayRuleType.LimbMask:
                            {
                                sb.L(i).A("LimbMask(").A(TemplateHelpers.EnumFlagsValue(rule.limbMask, "")).A("),");
                                break;
                            }
                        }
                    }
                    sb.EndBlock(ref i);
                }

                sb
                    .EndBlock(ref i)
                    .EndBlock(ref i).A(");");
            }

            sb
                .L(i).A("if (newRules.Count > 0)")
                .StartBlock(ref i)
                .L(i).A("Array.Resize(ref idrs.keyAssetRuleGroups, existingGroupsLength + newRules.Count);")
                .L(i).A("for (var i = 0; i < newRules.Count; i++)")
                .StartBlock(ref i)
                .L(i).A("idrs.keyAssetRuleGroups[existingGroupsLength + i] = newRules[i];")
                .EndBlock(ref i)
                .EndBlock(ref i)
                .L(0)
                .L(0)
                .L(i).A("void AddOrReplaceGroup(ItemDisplayRuleSet.KeyAssetRuleGroup group)")
                .StartBlock(ref i)
                .L(i).A("for (var i = 0; i < existingGroupsLength; i++)")
                .StartBlock(ref i)
                .L(i).A("if (idrs.keyAssetRuleGroups[i].keyAsset == group.keyAsset)")
                .StartBlock(ref i)
                .L(i).A("idrs.keyAssetRuleGroups[i] = group;")
                .L(i).A("return;")
                .EndBlock(ref i)
                .EndBlock(ref i)
                .L(0)
                .L(i).A("newRules.Add(group);")
                .EndBlock(ref i)
                .EndBlock(ref i)
                .L(0)
                .L(i).A("private static ItemDisplayRule ParentedPrefabAddressables(string guid, string childName, Vector3 localPos, Vector3 localAngles, Vector3 localScale)")
                .StartBlock(ref i)
                .L(i).A("return new ItemDisplayRule")
                .StartBlock(ref i)
                .L(i).A("ruleType = ItemDisplayRuleType.ParentedPrefab,")
                .L(i).A("followerPrefabAddress = new AssetReferenceGameObject(guid),")
                .L(i).A("childName = childName,")
                .L(i).A("localPos = localPos,")
                .L(i).A("localAngles = localAngles,")
                .L(i).A("localScale = localScale,")
                .EndBlock(ref i).A(");")
                .EndBlock(ref i)
                .L(0)
                .L(i).A("private static ItemDisplayRule ParentedPrefabAssetBundle(AssetBundle assetBundle, string assetPath, string childName, Vector3 localPos, Vector3 localAngles, Vector3 localScale)")
                .StartBlock(ref i)
                .L(i).A("return new ItemDisplayRule")
                .StartBlock(ref i)
                .L(i).A("ruleType = ItemDisplayRuleType.ParentedPrefab,")
                .L(i).A("followerPrefab = assetBundle.LoadAsset<GameObject>(assetPath),")
                .L(i).A("followerPrefabAddress = new AssetReferenceGameObject(\"\"),")
                .L(i).A("childName = childName,")
                .L(i).A("localPos = localPos,")
                .L(i).A("localAngles = localAngles,")
                .L(i).A("localScale = localScale,")
                .EndBlock(ref i)
                .EndBlock(ref i)
                .L(0)
                .L(i).A("private static ItemDisplayRule LimbMask(LimbFlags limbMask)")
                .StartBlock(ref i)
                .L(i).A("return new ItemDisplayRule")
                .StartBlock(ref i)
                .L(i).A("ruleType = ItemDisplayRuleType.LimbMask,")
                .L(i).A("limbMask = limbMask,")
                .EndBlock(ref i).A(");")
                .EndBlock(ref i)
                .L(0)
                .L(i).A("private static ItemDef GetItemDef(string itemDefName) => ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(itemDefName));")
                .L(i).A("private static EquipmentDef GetEquipmentDef(string equipmentDefName) => EquipmentCatalog.GetEquipmentDef(EquipmentCatalog.FindEquipmentIndex(equipmentDefName));")
                .EndBlock(ref i);

            if (!string.IsNullOrEmpty(@namespace))
            {
                sb.EndBlock(ref i);
            }

            return sb.ToString();
        }

        public void Import()
        {
            DialogController.ShowImport((path, importType) =>
            {
                try
                {
                    var json = File.ReadAllText(path);
                    var idrs = JsonUtility.FromJson<ExportItemDisplayRuleSet>(json);

                    DisableAll();

                    if (importType == ImportType.ReplaceSet)
                    {
                        for (var i = 0; i < previewRows.Count; i++)
                        {
                            var row = previewRows[i];
                            notPickedAssets.Add(row.displayRuleGroup.KeyAsset);
                            Destroy(row.gameObject);
                        }
                        itemDisplayRuleSet.DisplayRuleGroups.Clear();
                        previewRows.Clear();
                    }

                    var allBundles = AssetBundle.GetAllLoadedAssetBundles().ToDictionary(b => b.name);
                    var childNames = characterModel.childLocator.transformPairs.Select(p => p.name).ToHashSet();

                    var newRuleGroups = new List<ItemDisplayRuleSet.KeyAssetRuleGroup>();
                    foreach (var ruleGroup in idrs.itemGroups)
                    {
                        var index = ItemCatalog.FindItemIndex(ruleGroup.name);
                        var def = ItemCatalog.GetItemDef(index);

                        if (!def)
                        {
                            ItemDisplayPlacementHelperPlugin.InstanceLogger.LogError($"Couldn't get ItemDef name=\"{ruleGroup.name}\"");
                            continue;
                        }

                        var group = MapGroup(allBundles, childNames, ruleGroup);
                        group.KeyAsset = def;
                        AddKeyAsset(group);
                    }
                    foreach (var ruleGroup in idrs.equipmentGroups)
                    {
                        var index = EquipmentCatalog.FindEquipmentIndex(ruleGroup.name);
                        var def = EquipmentCatalog.GetEquipmentDef(index);

                        if (!def)
                        {
                            ItemDisplayPlacementHelperPlugin.InstanceLogger.LogError($"Couldn't get EquipmentDef name=\"{ruleGroup.name}\"");
                            continue;
                        }

                        var group = MapGroup(allBundles, childNames, ruleGroup);
                        group.KeyAsset = def;
                        group.KeyAssetAddress = new IDRSKeyAssetReference("");
                        AddKeyAsset(group);
                    }

                    ApplyFilter(filter);
                }
                catch (Exception ex)
                {
                    ItemDisplayPlacementHelperPlugin.InstanceLogger.LogError(ex);
                    DialogController.ShowError("Import failed, check log.");
                }

                static EditableDisplayRuleGroup MapGroup(Dictionary<string, AssetBundle> allBundles, HashSet<string> childNames, ExportKeyAssetRuleGroup ruleGroup)
                {
                    var group = new EditableDisplayRuleGroup();
                    for (int i = 0; i < ruleGroup.rules.Count; i++)
                    {
                        var displayGroup = ruleGroup.rules[i];
                        var childName = displayGroup.childName;
                        if (!string.IsNullOrEmpty(displayGroup.childPath))
                        {
                            var child = ModelPicker.Instance.CharacterModel.transform.Find(displayGroup.childPath);
                            if (child)
                            {
                                childName = StringHelpers.ChildNameFromPath(displayGroup.childPath);
                                var locator = ModelPicker.Instance.CharacterModel.childLocator;
                                if (!locator.FindChild(childName))
                                {
                                    ArrayUtils.ArrayAppend(ref locator.transformPairs, new NameTransformPair
                                    {
                                        name = childName,
                                        transform = child
                                    });
                                    ParentedPrefabDisplayController.Instance.childNameDropdown.options.Add(new TMP_Dropdown.OptionData(childName));
                                }
                            }
                        }
                        var rule = new EditableItemDisplayRule
                        {
                            childName = childName,
                            ruleType = displayGroup.ruleType,
                            limbMask = displayGroup.limbMask,
                            localAngles = displayGroup.localAngles,
                            localPos = displayGroup.localPos,
                            localScale = displayGroup.localScale,
                            followerPrefabAddress = new AssetReferenceGameObject(displayGroup.guid ?? "")
                        };

                        if (!string.IsNullOrEmpty(displayGroup.assetBundle))
                        {
                            var bundle = allBundles[displayGroup.assetBundle];
                            rule.followerPrefab = bundle.LoadAsset<GameObject>(displayGroup.assetPath);
                            if (rule.followerPrefab)
                            {
                                rule.assetBundle = displayGroup.assetBundle;
                                rule.assetPath = displayGroup.assetPath;
                                rule.referenceDiscovered = true;
                            }
                        }
                        else if (rule.followerPrefabAddress?.RuntimeKeyIsValid() ?? false)
                        {
                            rule.referenceDiscovered = true;
                        }
                        group.Rules.Add(rule);
                    }

                    return group;
                }

                void AddKeyAsset(EditableDisplayRuleGroup displayGroup)
                {
                    for (var i = 0; i < itemDisplayRuleSet.DisplayRuleGroups.Count; i++)
                    {
                        var group = itemDisplayRuleSet.DisplayRuleGroups[i];
                        if (group.KeyAsset == displayGroup.KeyAsset)
                        {
                            itemDisplayRuleSet.DisplayRuleGroups[i] = displayGroup;
                            previewRows[i].displayRuleGroup = displayGroup;
                            return;
                        }
                    }

                    itemDisplayRuleSet.DisplayRuleGroups.Add(displayGroup);
                    notPickedAssets.Remove(displayGroup.KeyAsset);
                    CreateRowInstance(displayGroup);
                }
            });
        }

        public bool FillPrefabReference(EditableItemDisplayRule rule, bool refreshBundles)
        {
            if (rule.referenceDiscovered)
            {
                return false;
            }

            rule.referenceDiscovered = true;
            if (!rule.followerPrefab)
            {
                return false;
            }

            var (guid, assetBundle, path) = GetPrefabReference(rule.followerPrefab, refreshBundles);
            rule.followerPrefabAddress = new AssetReferenceGameObject(guid);
            rule.assetBundle = assetBundle;
            rule.assetPath = path;

            return true;
        }

        public (string guid, string assetBundle, string path) GetPrefabReference(GameObject go, bool refreshBundles)
        {
            if (pathToGuid is null)
            {
                var guidRegex = new Regex("^[0-9a-f]{32}$", RegexOptions.Compiled);
                var locator = Addressables.ResourceLocators.FirstOrDefault(l => l.LocatorId == "AddressablesMainContentCatalog") as ResourceLocationMap;
                pathToGuid = locator.Locations
                    .Where(l => l.Key is string guid && guid.Length == 32 && guidRegex.IsMatch(guid))
                    .Where(l =>
                    {
                        var type = l.Value[0].ResourceType;
                        return type.IsAssignableFrom(typeof(GameObject));
                    })
                    .ToDictionary(l => l.Value[0].InternalId, l => l.Key as string, StringComparer.OrdinalIgnoreCase);
            }

            if (dirToBundle is null || refreshBundles)
            {
                dirToBundle ??= new Dictionary<string, AssetBundle>();
                var allBundles = AssetBundle.GetAllLoadedAssetBundles();
                var bundleToAssets = new Dictionary<AssetBundle, Dictionary<int, string>>();
                foreach (var b in allBundles)
                {
                    var dirs = NativeHelpers.GetBundleDirectories(b);
                    foreach (var dir in dirs)
                    {
                        dirToBundle[$"archive:/{dir}/{dir}"] = b;
                    }
                }
            }

            var id = go.GetInstanceID();
            var pathName = NativeHelpers.GetPathName(id);
            if (dirToBundle.TryGetValue(pathName, out var bundle))
            {
                bundleToAssets ??= new Dictionary<AssetBundle, Dictionary<int, string>>();
                if (!bundleToAssets.TryGetValue(bundle, out var assets))
                {
                    bundleToAssets[bundle] = assets = NativeHelpers.GetBundleContainerPaths(bundle);
                }

                if (assets.TryGetValue(id, out var assetPath))
                {
                    var bundleName = bundle.name;
                    if (bundleName.EndsWith(".bundle") && pathToGuid.TryGetValue(assetPath, out var guid))
                    {
                        return (guid, default, default);
                    }

                    return (default, bundleName, assetPath);
                }
            }

            return default;
        }

        private void UpdateRowsVisibility()
        {
            var filterIsEmpty = string.IsNullOrEmpty(filter);

            var identifier = showItemsMode.options[showItemsMode.value].text;
            var packs = showItemsMode.value > 0 ?
                ContentManager.allLoadedContentPacks.Where(p => p.identifier == identifier).ToArray() :
                null;

            foreach (var row in previewRows)
            {
                var active = filterIsEmpty || row.nameText.ContainsInSequence(filter);
                if (active && packs != null)
                {
                    if (row.displayRuleGroup.KeyAsset is ItemDef itemDef)
                    {
                        active = packs.Any(p => p.itemDefs.Contains(itemDef));
                    }
                    else if (row.displayRuleGroup.KeyAsset is EquipmentDef equipmentDef)
                    {
                        active = packs.Any(p => p.equipmentDefs.Contains(equipmentDef));
                    }
                }
                row.gameObject.SetActive(active);
            }
        }
    }
}
