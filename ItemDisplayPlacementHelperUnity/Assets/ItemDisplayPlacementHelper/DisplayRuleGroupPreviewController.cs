using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper
{
    public class DisplayRuleGroupPreviewController : MonoBehaviour
    {
        [HideInInspector]
        public Sprite icon;
        [HideInInspector]
        public string nameText;
        [HideInInspector]
        public DisplayRuleGroup displayRuleGroup;
        [HideInInspector]
        public ItemDisplayRuleSetController.Catalog catalog;
        [HideInInspector]
        public int index;

        private CharacterModel characterModel;

        [SerializeField]
        private Image imageComponent;
        [SerializeField]
        private TextMeshProUGUI textComponent;
        [SerializeField]
        private Toggle toggleComponent;
        [SerializeField]
        private Button buttonComponent;
        [SerializeField]
        private ToggleGroup toggleGroupComponent;
        [SerializeField]
        private Image background;

        private void Awake()
        {
            characterModel = ModelPicker.Instance.CharacterModel;
            ModelPicker.OnModelChanged += OnModelChanged;
        }

        private void OnDestroy()
        {
            ModelPicker.OnModelChanged -= OnModelChanged;
        }

        private void OnModelChanged(CharacterModel characterModel)
        {
            this.characterModel = characterModel;
        }

        private void Start()
        {
            textComponent.text = nameText;
            imageComponent.sprite = icon;
            
            switch (catalog)
            {
                case ItemDisplayRuleSetController.Catalog.Item:
                    toggleComponent.isOn = characterModel.enabledItemDisplays.Contains((ItemIndex)index);
                    break;
                case ItemDisplayRuleSetController.Catalog.Equipment:
                    toggleComponent.group = toggleGroupComponent;
                    toggleComponent.isOn = characterModel.currentEquipmentDisplayIndex == (EquipmentIndex)index;
                    break;
            }
        }

        public void EditDisplayRuleGroup()
        {
            if (DisplayRuleGroupEditingController.Instance.Catalog == catalog && DisplayRuleGroupEditingController.Instance.Index == index)
            {
                DisplayRuleGroupEditingController.Instance.SetDisplayRuleGroup(default, default, -1);
            }
            else
            {
                toggleComponent.isOn = true;
                DisplayRuleGroupEditingController.Instance.SetDisplayRuleGroup(displayRuleGroup, catalog, index);
            }
        }

        public void ToggleDisplay(bool display)
        {
            if (toggleComponent.isOn != display)
            {
                toggleComponent.isOn = display;
                return;
            }
            if (display)
            {
                switch (catalog)
                {
                    case ItemDisplayRuleSetController.Catalog.Item:
                        characterModel.EnableItemDisplay((ItemIndex)index);
                        break;
                    case ItemDisplayRuleSetController.Catalog.Equipment:
                        characterModel.SetEquipmentDisplay((EquipmentIndex)index);
                        break;
                }
            }
            else
            {
                switch (catalog)
                {
                    case ItemDisplayRuleSetController.Catalog.Item:
                        characterModel.DisableItemDisplay((ItemIndex)index);
                        break;
                    case ItemDisplayRuleSetController.Catalog.Equipment:
                        characterModel.SetEquipmentDisplay(EquipmentIndex.None);
                        break;
                }
                if (DisplayRuleGroupEditingController.Instance.Catalog == catalog && DisplayRuleGroupEditingController.Instance.Index == index)
                {
                    DisplayRuleGroupEditingController.Instance.SetDisplayRuleGroup(default, default, -1);
                }
            }
        }

        private void Update()
        {
            background.enabled = DisplayRuleGroupEditingController.Instance.Catalog == catalog && DisplayRuleGroupEditingController.Instance.Index == index;
        }
    }
}
