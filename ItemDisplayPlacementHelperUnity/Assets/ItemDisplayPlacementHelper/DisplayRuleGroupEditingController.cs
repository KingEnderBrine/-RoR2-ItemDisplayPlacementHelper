using ItemDisplayPlacementHelper;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ItemDisplayPlacementHelper
{
    public class DisplayRuleGroupEditingController : MonoBehaviour
    {
        public GameObject rowPrefab;
        public Transform container;

        private CharacterModel characterModel;

        public delegate void OnDisplayRuleGroupChangedHadler(DisplayRuleGroup displayRuleGroup);
        public static OnDisplayRuleGroupChangedHadler OnDisplayRuleGroupChanged;

        public static DisplayRuleGroupEditingController Instance { get; private set; }
        public DisplayRuleGroup DisplayRuleGroup { get; private set; }
        public ItemDisplayRuleSetController.Catalog Catalog { get; private set; }
        public int Index { get; private set; } = -1;

        private readonly List<CharacterModel.ParentedPrefabDisplay> parentedPrefabDisplays = new List<CharacterModel.ParentedPrefabDisplay>();

        private readonly List<GameObject> rows = new List<GameObject>();

        private void Awake()
        {
            Instance = this;
            ModelPicker.OnModelChanged += OnModelChanged;
        }

        private void OnDestroy()
        {
            ModelPicker.OnModelChanged -= OnModelChanged;
            Instance = null;
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

            rows.ForEach(el => Destroy(el));
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
            
                row.SetActive(true);
                rows.Add(row);
            }

            OnDisplayRuleGroupChanged(displayRuleGroup);
        }
    }
}
