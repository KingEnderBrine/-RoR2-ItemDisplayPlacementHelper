using System;
using System.Collections.Generic;
using System.Linq;
using RoR2;
using RoR2.ContentManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper
{
    public class ItemDisplayRuleSetController : MonoBehaviour
    {
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

        public IReadOnlyDictionary<int, DisplayRuleGroupPreviewController> ItemRows => itemRows;
        public IReadOnlyDictionary<int, DisplayRuleGroupPreviewController> EquipmentRows => equipmentRows;

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
            noIDRSTextObject.SetActive(ModelPicker.Instance.ModelInstance && (!characterModel || !characterModel.itemDisplayRuleSet));

            enableAllButton.interactable = characterModel;
            disableAllButton.interactable = characterModel;
            searchInput.interactable = characterModel;
            showItemsMode.interactable = characterModel;
        }

        private void OnModelChanged(CharacterModel characterModel)
        {
            if (!characterModel || !characterModel.itemDisplayRuleSet)
            {
                return;
            }

            this.characterModel = characterModel;

            GatherDisplayRules(this.characterModel.itemDisplayRuleSet.runtimeItemRuleGroups, itemRows, Catalog.Item);
            GatherDisplayRules(this.characterModel.itemDisplayRuleSet.runtimeEquipmentRuleGroups, equipmentRows, Catalog.Equipment);
            ApplyFilter(filter);
        }

        private void OnModelWillChange(CharacterModel model)
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
                if (row.gameObject.activeSelf)
                {
                    row.ToggleDisplay(true);
                }
            }
            characterModel.UpdateMaterials();
        }

        public void DisableAll()
        {
            foreach (var row in itemRows.Values)
            {
                if (row.gameObject.activeSelf)
                {
                    row.ToggleDisplay(false);
                }
            }
            foreach (var row in equipmentRows.Values)
            {
                if (row.gameObject.activeSelf)
                {
                    row.ToggleDisplay(false);
                }
            }

        }

        private void UpdateRowsVisibility()
        {
            var filterIsEmpty = string.IsNullOrEmpty(filter);

            var identifier = showItemsMode.options[showItemsMode.value].text;
            var packs = showItemsMode.value > 0 ?
                ContentManager.allLoadedContentPacks.Where(p => p.identifier == identifier).ToArray() :
                null;

            foreach (var row in itemRows.Values)
            {
                var active = filterIsEmpty || row.nameText.ContainsInSequence(filter);
                if (active && packs != null)
                {
                    var itemDef = ItemCatalog.GetItemDef((ItemIndex)row.index);
                    active = packs.Any(p => p.itemDefs.Contains(itemDef));
                }
                row.gameObject.SetActive(active);
            }
            foreach (var row in equipmentRows.Values)
            {
                var active = filterIsEmpty || row.nameText.ContainsInSequence(filter);
                if (active && packs != null)
                {
                    var equipmentDef = EquipmentCatalog.GetEquipmentDef((EquipmentIndex)row.index);
                    active = packs.Any(p => p.equipmentDefs.Contains(equipmentDef));
                }
                row.gameObject.SetActive(active);
            }
        }
    }
}
