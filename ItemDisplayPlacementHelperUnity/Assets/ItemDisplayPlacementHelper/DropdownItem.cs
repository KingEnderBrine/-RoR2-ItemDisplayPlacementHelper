using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper
{
    public class DropdownItem : MonoBehaviour, IEventSystemHandler, ICancelHandler
    {
        public TMP_Text text;
        public Image image;
        public RectTransform rectTransform;
        public Button button;
        [HideInInspector]
        public object Value { get; set; }

        public virtual void OnCancel(BaseEventData eventData)
        {
            var componentInParent = GetComponentInParent<SearchableDropdown>();
            if (!componentInParent)
            {
                return;
            }
            componentInParent.Hide();
        }
    }
}
