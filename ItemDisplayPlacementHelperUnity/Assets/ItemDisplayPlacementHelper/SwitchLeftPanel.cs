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
    public class SwitchLeftPanel : MonoBehaviour
    {
        public Button buttonComponent;
        public TextMeshProUGUI buttonText;

        public GameObject mainLeftPanel;
        public string mainLeftPanelButtonText;

        public GameObject animatorLeftPanel;
        public string animatorLeftPanelButtonText;

        public void TogglePanel()
        {
            if (mainLeftPanel.activeSelf)
            {
                mainLeftPanel.SetActive(false);
                animatorLeftPanel.SetActive(true);
                buttonText.text = mainLeftPanelButtonText;
            }
            else
            {
                animatorLeftPanel.SetActive(false);
                mainLeftPanel.SetActive(true);
                buttonText.text = animatorLeftPanelButtonText;
            }
        }
    }
}
