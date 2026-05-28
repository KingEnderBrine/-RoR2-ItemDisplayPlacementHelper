using ItemDisplayPlacementHelper.Editable;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper
{
    public class ItemDisplayRulePreviewController : MonoBehaviour
    {
        [HideInInspector, NonSerialized]
        public EditableItemDisplayRule itemDisplayRule;

        [SerializeField]
        private TMP_Text textComponent;
        [SerializeField]
        private Button buttonComponent;

        private void Update()
        {
            switch (itemDisplayRule.ruleType)
            {
                case ItemDisplayRuleType.ParentedPrefab:
                {
                    textComponent.text = itemDisplayRule.followerPrefab ? itemDisplayRule.followerPrefab.name : "No prefab";
                    break;
                }
                case ItemDisplayRuleType.LimbMask:
                {
                    textComponent.text = "LimbMask";
                    break;
                }
            }

            buttonComponent.interactable = ParentedPrefabDisplayController.Instance.ItemDisplayRule != itemDisplayRule;
        }

        public void EditItemDisplayRule()
        {
            ParentedPrefabDisplayController.Instance.SetItemDisplayRule(itemDisplayRule);
        }
    }
}
