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
    public class KeyAssetRowController : MonoBehaviour
    {
        public TMP_Text text;
        public TMP_Text subText;
        public Image icon;
        public Button button;
        [NonSerialized]
        public UnityEngine.Object keyAsset;
    }
}
