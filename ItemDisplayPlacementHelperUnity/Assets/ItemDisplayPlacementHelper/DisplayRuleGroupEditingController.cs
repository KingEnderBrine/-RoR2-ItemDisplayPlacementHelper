using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ItemDisplayPlacementHelper.Editable;
using RoR2;
using UnityEngine;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper
{
    public class DisplayRuleGroupEditingController : MonoBehaviour
    {
        public GameObject rowPrefab;
        public Transform container;
        public Button reapplyButton;
        public Button addButton;
        public Button deleteButton;

        private CharacterModel characterModel;

        public delegate void OnDisplayRuleGroupChangedHandler(EditableDisplayRuleGroup keyAssetRuleGroup);
        public static OnDisplayRuleGroupChangedHandler OnDisplayRuleGroupChanged;

        public static DisplayRuleGroupEditingController Instance { get; private set; }
        public EditableDisplayRuleGroup DisplayRuleGroup { get; private set; }

        private readonly List<ItemDisplayRulePreviewController> rows = new List<ItemDisplayRulePreviewController>();

        private void Awake()
        {
            Instance = this;
            ModelPicker.OnModelWillChange += OnModelWillChange;
            ModelPicker.OnModelChanged += OnModelChanged;
        }

        private void OnDestroy()
        {
            ModelPicker.OnModelWillChange -= OnModelWillChange;
            ModelPicker.OnModelChanged -= OnModelChanged;
            Instance = null;
        }

        private void Update()
        {
            var hasGroup = DisplayRuleGroup is not null;
            reapplyButton.interactable = hasGroup;
            addButton.interactable = hasGroup;
            deleteButton.interactable = hasGroup && ParentedPrefabDisplayController.Instance.ItemDisplayRule is not null;
        }

        private void OnModelWillChange()
        {
            SetDisplayRuleGroup(null);
        }

        private void OnModelChanged(CharacterModel characterModel)
        {
            this.characterModel = characterModel;
        }

        public void SetDisplayRuleGroup(EditableDisplayRuleGroup displayRuleGroup)
        {
            if (DisplayRuleGroup == displayRuleGroup)
            {
                return;
            }

            DisplayRuleGroup = displayRuleGroup;

            foreach (var row in rows)
            {
                Destroy(row.gameObject);
            }
            rows.Clear();

            if (displayRuleGroup is null || !characterModel)
            {
                OnDisplayRuleGroupChanged(displayRuleGroup);
                return;
            }

            var rules = displayRuleGroup.Rules;
            for (var i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                CreateRow(rule);
            }

            OnDisplayRuleGroupChanged(displayRuleGroup);
        }

        private void CreateRow(EditableItemDisplayRule rule)
        {
            var row = Instantiate(rowPrefab, container);
            var itemDisplayRuleController = row.GetComponent<ItemDisplayRulePreviewController>();
            itemDisplayRuleController.itemDisplayRule = rule;

            row.SetActive(true);
            rows.Add(itemDisplayRuleController);
        }

        public void ReapplyCurrentDisplayGroup()
        {
            DisplayRuleGroup.Disable(characterModel);
            DisplayRuleGroup.Enable(characterModel);
        }

        public void Add()
        {
            var rule = new EditableItemDisplayRule();
            rule.localScale = Vector3.one;
            DisplayRuleGroup.Rules.Add(rule);
            CreateRow(rule);
            rule.Enable(characterModel);
        }

        public void Delete()
        {
            var rule = ParentedPrefabDisplayController.Instance.ItemDisplayRule;
            ParentedPrefabDisplayController.Instance.SetItemDisplayRule(null);
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row.itemDisplayRule == rule)
                {
                    rule.Disable(characterModel);
                    rows.RemoveAt(i);
                    DisplayRuleGroup.Rules.RemoveAt(i);
                    Destroy(row.gameObject);
                    break;
                }
            }
        }
    }
}
