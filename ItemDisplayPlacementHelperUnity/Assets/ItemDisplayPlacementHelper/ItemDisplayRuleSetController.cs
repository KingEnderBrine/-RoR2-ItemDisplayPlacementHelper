using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper
{
    public class ItemDisplayRuleSetController : MonoBehaviour
    {
        private static ReadOnlyNamedAssetCollection<EquipmentDef> _ror2EquipmentDefs;
        private static ReadOnlyNamedAssetCollection<EquipmentDef> RoR2EquipmentDefs { get => _ror2EquipmentDefs.src != null ? _ror2EquipmentDefs : (_ror2EquipmentDefs = ContentManager.FindContentPack("RoR2.BaseContent").GetValueOrDefault().equipmentDefs); }

        private static ReadOnlyNamedAssetCollection<ItemDef> _ror2ItemDefs;
        private static ReadOnlyNamedAssetCollection<ItemDef> RoR2ItemDefs { get => _ror2ItemDefs.src != null ? _ror2ItemDefs : (_ror2ItemDefs = ContentManager.FindContentPack("RoR2.BaseContent").GetValueOrDefault().itemDefs); }

        public enum Catalog { Item, Equipment }

        public GameObject rowPrefab;
        public Transform container;

        [Space]
        public Button enableAllButton;
        public Button disableAllButton;
        public TMP_InputField searchInput;
        public GameObject noIDRSTextObject;
        public TMP_Dropdown showItemsMode;

        private readonly Dictionary<int, DisplayRuleGroupPreviewController> itemRows = new Dictionary<int, DisplayRuleGroupPreviewController>();
        private readonly Dictionary<int, DisplayRuleGroupPreviewController> equipmentRows = new Dictionary<int, DisplayRuleGroupPreviewController>();

        private CharacterModel characterModel;

        private string filter;

        private void Awake()
        {
            ModelPicker.OnModelChanged += OnModelChanged;
        }

        private void OnDestroy()
        {
            ModelPicker.OnModelChanged -= OnModelChanged;
        }

        private void Update()
        {
            noIDRSTextObject.SetActive(characterModel && !characterModel.itemDisplayRuleSet);

            enableAllButton.interactable = characterModel;
            disableAllButton.interactable = characterModel;
            searchInput.interactable = characterModel;
            showItemsMode.interactable = characterModel;
        }

        private void OnModelChanged(CharacterModel characterModel)
        {
            ClearCurrentModel();
            
            if (!characterModel || !characterModel.itemDisplayRuleSet)
            {
                return;
            }

            this.characterModel = characterModel;

            GatherDisplayRules(this.characterModel.itemDisplayRuleSet.runtimeItemRuleGroups, itemRows, Catalog.Item);
            GatherDisplayRules(this.characterModel.itemDisplayRuleSet.runtimeEquipmentRuleGroups, equipmentRows, Catalog.Equipment);
            ApplyFilter(filter);
        }

        private void ClearCurrentModel()
        {
            foreach (var row in itemRows.Values)
            {
                Destroy(row.gameObject);
            }
            itemRows.Clear();

            foreach (var row in equipmentRows.Values)
            {
                Destroy(row.gameObject);
            }
            equipmentRows.Clear();

            characterModel = null;
        }

        private void GatherDisplayRules(DisplayRuleGroup[] displayRuleGroups, Dictionary<int, DisplayRuleGroupPreviewController> rows, Catalog catalog)
        {
            for (var index = 0; index < displayRuleGroups.Length; index++)
            {
                var displayGroup = displayRuleGroups[index];
                if (displayGroup.isEmpty)
                {
                    continue;
                }

                var row = Instantiate(rowPrefab, container);
                var controller = row.GetComponent<DisplayRuleGroupPreviewController>();
        
                controller.displayRuleGroup = displayGroup;
                (controller.icon, controller.nameText) = GetItemInfo(catalog, index);
                controller.catalog = catalog;
                controller.index = index;
        
                row.SetActive(true);
                rows[index] = controller;
            }
        }

        private (Sprite, string) GetItemInfo(Catalog catalog, int index)
        {
            switch (catalog)
            {
                case Catalog.Item:
                    var item = ItemCatalog.GetItemDef((ItemIndex)index);
                    return (item.pickupIconSprite, Language.GetString(item.nameToken));
                case Catalog.Equipment:
                    var equipment = EquipmentCatalog.GetEquipmentDef((EquipmentIndex)index);
                    return (equipment.pickupIconSprite, Language.GetString(equipment.nameToken));
            }
            throw new ArgumentException();
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
            foreach (var row in itemRows.Values)
            {
                row.ToggleDisplay(true);
            }
        }

        public void DisableAll()
        {
            foreach (var row in itemRows.Values)
            {
                row.ToggleDisplay(false);
            }
            foreach (var row in equipmentRows.Values)
            {
                row.ToggleDisplay(false);
            }
        }

        private void UpdateRowsVisibility()
        {
            var filterIsEmpty = string.IsNullOrEmpty(filter);

            foreach (var row in itemRows.Values)
            {
                var active = filterIsEmpty || row.nameText.ContainsInSequence(filter);
                if (active)
                {
                    var itemDef = ItemCatalog.GetItemDef((ItemIndex)row.index);
                    switch (showItemsMode.value)
                    {
                        case 1:
                            active = RoR2ItemDefs.Contains(itemDef);
                            break;
                        case 2:
                            active = !RoR2ItemDefs.Contains(itemDef);
                            break;
                    }
                }
                row.gameObject.SetActive(active);
            }
            foreach (var row in equipmentRows.Values)
            {
                var active = filterIsEmpty || row.nameText.ContainsInSequence(filter);
                if (active)
                {
                    var equipmentDef = EquipmentCatalog.GetEquipmentDef((EquipmentIndex)row.index);
                    switch (showItemsMode.value)
                    {
                        case 1:
                            active = RoR2EquipmentDefs.Contains(equipmentDef);
                            break;
                        case 2:
                            active = !RoR2EquipmentDefs.Contains(equipmentDef);
                            break;
                    }
                }
                row.gameObject.SetActive(active);
            }
        }
    }
}
