using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ItemDisplayPlacementHelper
{
    public class BepInExConfigController : MonoBehaviour
    {
        public SensitivityController sensitivityController;
        public ParentedPrefabDisplayController parentedPrefabDisplayController;

        private void Start()
        {
            sensitivityController.fastCoefficientInput.Value = ConfigHelper.FastCoefficient.Value;
            sensitivityController.slowCoefficientInput.Value = ConfigHelper.SlowCoefficient.Value;
            parentedPrefabDisplayController.CopyFormat = ConfigHelper.CopyFormat.Value;
            parentedPrefabDisplayController.customFormatInput.text = FromConfigFriendly(ConfigHelper.CustomFormat.Value);

            StartCoroutine(SaveCurrentValues());
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private IEnumerator SaveCurrentValues()
        {
            while (true)
            {
                yield return new WaitForSeconds(10);

                ConfigHelper.FastCoefficient.Value = sensitivityController.fastCoefficientInput.Value;
                ConfigHelper.SlowCoefficient.Value = sensitivityController.slowCoefficientInput.Value;
                ConfigHelper.CopyFormat.Value = parentedPrefabDisplayController.CopyFormat;
                ConfigHelper.CustomFormat.Value = ToConfigFriendly(parentedPrefabDisplayController.customFormatInput.text);
            }
        }

        private static string ToConfigFriendly(string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str)).Replace('=', '-');
        }

        private static string FromConfigFriendly(string str)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(str.Replace('-', '=')));
        }
    }
}
