using System;
using ItemDisplayPlacementHelper.Editable;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper
{
    public class DisplayRuleGroupPreviewController : MonoBehaviour
    {
        [HideInInspector, NonSerialized]
        public Sprite icon;
        [HideInInspector, NonSerialized]
        public string nameText;
        [HideInInspector, NonSerialized]
        public EditableDisplayRuleGroup displayRuleGroup;

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

            toggleComponent.isOn = displayRuleGroup.Enabled;
        }

        public void EditDisplayRuleGroup()
        {
            if (DisplayRuleGroupEditingController.Instance.DisplayRuleGroup == displayRuleGroup)
            {
                DisplayRuleGroupEditingController.Instance.SetDisplayRuleGroup(null);
            }
            else
            {
                toggleComponent.isOn = true;
                DisplayRuleGroupEditingController.Instance.SetDisplayRuleGroup(displayRuleGroup);
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
                displayRuleGroup.Enable(characterModel);
            }
            else
            {
                displayRuleGroup.Disable(characterModel);
                if (DisplayRuleGroupEditingController.Instance.DisplayRuleGroup == displayRuleGroup)
                {
                    DisplayRuleGroupEditingController.Instance.SetDisplayRuleGroup(null);
                }
            }
        }

        private void Update()
        {
            background.enabled = DisplayRuleGroupEditingController.Instance.DisplayRuleGroup == displayRuleGroup;
        }
    }
}
