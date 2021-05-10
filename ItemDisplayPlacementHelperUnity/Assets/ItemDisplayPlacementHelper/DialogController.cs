using TMPro;
using UnityEngine;

namespace ItemDisplayPlacementHelper
{
    public class DialogController : MonoBehaviour
    {
        private static DialogController Instance { get; set; }

        public TMP_Text textComponent;
        public GameObject container;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public static void ShowError(string text)
        {
            if (!Instance)
            {
                return;
            }

            Instance.textComponent.text = text;
            Instance.container.SetActive(true);
        }

        public void Ok()
        {
            container.SetActive(false);
        }
    }
}
