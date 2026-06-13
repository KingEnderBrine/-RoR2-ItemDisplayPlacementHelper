using System;
using System.Collections;
using RoR2;
using UnityEngine;

namespace IDRSJsonLoader.Models
{
    [Serializable]
    public class ExportItemDisplayRule
    {
        public string guid;
        public string assetBundle;
        public string assetPath;
        public Vector3 localScale;
        public Vector3 localPos;
        public Vector3 localAngles;
        public string childName;
        public string childPath;
        public LimbFlags limbMask;
        public ItemDisplayRuleType ruleType;
    }
}