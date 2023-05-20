using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        private CharacterModel characterModel;

        public delegate void OnDisplayRuleGroupChangedHadler(DisplayRuleGroup displayRuleGroup);
        public static OnDisplayRuleGroupChangedHadler OnDisplayRuleGroupChanged;

        public static DisplayRuleGroupEditingController Instance { get; private set; }
        public DisplayRuleGroup DisplayRuleGroup { get; private set; }
        public ItemDisplayRuleSetController.Catalog Catalog { get; private set; }
        public int Index { get; private set; } = -1;

        private readonly List<CharacterModel.ParentedPrefabDisplay> parentedPrefabDisplays = new List<CharacterModel.ParentedPrefabDisplay>();

        private readonly List<GameObject> rows = new List<GameObject>();

        private ItemDisplayRuleSet TempRuleSet;

        private void Awake()
        {
            Instance = this;
            ModelPicker.OnModelChanged += OnModelChanged;
            TempRuleSet = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
            TempRuleSet.GenerateRuntimeValues();
        }

        private void OnDestroy()
        {
            ModelPicker.OnModelChanged -= OnModelChanged;
            Instance = null;
            Destroy(TempRuleSet);
        }

        private void Update()
        {
            reapplyButton.interactable = Index != -1;
        }

        private void OnModelChanged(CharacterModel characterModel)
        {
            this.characterModel = characterModel;
            SetDisplayRuleGroup(default, default, -1);
        }

        public void SetDisplayRuleGroup(DisplayRuleGroup displayRuleGroup, ItemDisplayRuleSetController.Catalog catalog, int index)
        {
            if (DisplayRuleGroup.Equals(displayRuleGroup) && Catalog == catalog && Index == index)
            {
                return;
            }

            DisplayRuleGroup = displayRuleGroup;
            Catalog = catalog;
            Index = index;


            parentedPrefabDisplays.Clear();

            rows.ForEach(Destroy);
            rows.Clear();

            if (index == -1 || !characterModel)
            {
                OnDisplayRuleGroupChanged(displayRuleGroup);
                return;
            }

            switch (catalog)
            {
                case ItemDisplayRuleSetController.Catalog.Item:
                    var itemIndex = (ItemIndex)index;
                    parentedPrefabDisplays.AddRange(characterModel.parentedPrefabDisplays.Where(el => el.itemIndex == itemIndex));
                    break;
                case ItemDisplayRuleSetController.Catalog.Equipment:
                    var equipmentIndex = (EquipmentIndex)index;
                    parentedPrefabDisplays.AddRange(characterModel.parentedPrefabDisplays.Where(el => el.equipmentIndex == equipmentIndex));
                    break;
            }
            
            for (var i = 0; i < parentedPrefabDisplays.Count; i++)
            {
                var row = Instantiate(rowPrefab, container);
            
                var itemDisplayRuleController = row.GetComponent<ItemDisplayRulePreviewController>();
            
                itemDisplayRuleController.itemDisplayRule = displayRuleGroup.rules[i];
                itemDisplayRuleController.parentedPrefabDisplay = parentedPrefabDisplays[i];

                var itemFollower = parentedPrefabDisplays[i].instance.GetComponent<ItemFollower>();
                if (itemFollower && itemFollower.followerPrefab)
                {
                    itemFollower.StartCoroutine(AddComponentToFollowerInstanceCoroutine(itemFollower));
                }

                row.SetActive(true);
                rows.Add(row);
            }

            OnDisplayRuleGroupChanged(displayRuleGroup);
        }

        public void ReapplyCurrentDisplayGroup()
        {
            var displayRuleGroup = DisplayRuleGroup;
            var displayRuleIndex = Array.IndexOf(displayRuleGroup.rules, ParentedPrefabDisplayController.Instance.ItemDisplayRule);
            var rules = displayRuleGroup.rules.ToArray();

            for (var i = 0; i < parentedPrefabDisplays.Count; i++)
            {
                var display = parentedPrefabDisplays[i];
                var instance = display.instance;
                var parent = instance.transform.parent;

                var rule = rules[i];
                rule.localPos = instance.transform.localPosition;
                rule.localScale = instance.transform.localScale;
                rule.localAngles = instance.transform.localEulerAngles;
                rule.childName = characterModel.childLocator.transformPairs.FirstOrDefault(p => p.transform == parent).name;
                rules[i] = rule;
            }
            displayRuleGroup.rules = rules;

            DisplayRuleGroupPreviewController displayRuleGroupPreviewController;
            if (Catalog == ItemDisplayRuleSetController.Catalog.Item)
            {
                displayRuleGroupPreviewController = ItemDisplayRuleSetController.Instance.ItemRows[Index];
                TempRuleSet.runtimeItemRuleGroups[Index] = displayRuleGroup;
            }
            else
            {
                displayRuleGroupPreviewController = ItemDisplayRuleSetController.Instance.EquipmentRows[Index];
                TempRuleSet.runtimeEquipmentRuleGroups[Index] = displayRuleGroup;
            }

            displayRuleGroupPreviewController.ToggleDisplay(false);
            var oldIDRS = characterModel.itemDisplayRuleSet;
            characterModel.itemDisplayRuleSet = TempRuleSet;
            displayRuleGroupPreviewController.EditDisplayRuleGroup();
            characterModel.itemDisplayRuleSet = oldIDRS;

            ParentedPrefabDisplayController.Instance.SetItemDisplayRule(DisplayRuleGroup.rules[displayRuleIndex], parentedPrefabDisplays[displayRuleIndex]);
        }

        private IEnumerator AddComponentToFollowerInstanceCoroutine(ItemFollower itemFollower)
        {
            yield return new WaitUntil(() => itemFollower.followerInstance);

            var matchLocalScale = itemFollower.followerInstance.AddComponent<MatchLocalScale>();
            matchLocalScale.target = itemFollower.transform;
        }
    }
}
