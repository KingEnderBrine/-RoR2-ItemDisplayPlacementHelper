using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IDRSJsonLoader.Models
{
    [Serializable]
    public class ExportItemDisplayRuleSet
    {
        public string bodyName;
        public string skinName;
        public List<ExportKeyAssetRuleGroup> itemGroups = new List<ExportKeyAssetRuleGroup>();
        public List<ExportKeyAssetRuleGroup> equipmentGroups = new List<ExportKeyAssetRuleGroup>();
    }
}
