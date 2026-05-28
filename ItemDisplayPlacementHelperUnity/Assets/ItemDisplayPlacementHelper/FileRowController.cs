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
    public class FileRowController : MonoBehaviour
    {
        public TMP_Text text;
        public Image icon;
        public Button button;
        [NonSerialized]
        public bool isFile;
        [NonSerialized]
        public string part;
    }
}
