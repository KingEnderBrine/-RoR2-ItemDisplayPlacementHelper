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
        [HideInInspector]
        public ItemDisplayRule itemDisplayRule;
        [HideInInspector]
        public CharacterModel.ParentedPrefabDisplay parentedPrefabDisplay;

        [SerializeField]
        private TMP_Text textComponent;
        [SerializeField]
        private Button buttonComponent;

        private void Start()
        {
            textComponent.text = parentedPrefabDisplay.instance.name;
        }

        private void Update()
        {
            buttonComponent.interactable = !ParentedPrefabDisplayController.Instance.ParentedPrefabDisplay.Equals(parentedPrefabDisplay);
        }

        public void EditItemDisplayRule()
        {
            ParentedPrefabDisplayController.Instance.SetItemDisplayRule(itemDisplayRule, parentedPrefabDisplay);
        }
    }
}
