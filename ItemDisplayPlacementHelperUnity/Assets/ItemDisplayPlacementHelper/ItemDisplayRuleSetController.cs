using RoR2;
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
        public enum Catalog { Item, Equipment }

        public GameObject rowPrefab;
        public Transform container;

        [Space]
        public Button enableAllButton;
        public Button disableAllButton;
        public TMP_InputField searchInput;
        public GameObject noIDRSTextObject;

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
            var filterIsEmpty = string.IsNullOrEmpty(filter);

            foreach (var row in itemRows.Values)
            {
                row.gameObject.SetActive(filterIsEmpty || row.nameText.ContainsInSequence(filter));
            }
            foreach (var row in equipmentRows.Values)
            {
                row.gameObject.SetActive(filterIsEmpty || row.nameText.ContainsInSequence(filter));
            }
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
    }
}
