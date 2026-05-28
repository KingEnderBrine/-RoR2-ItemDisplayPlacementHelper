using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;

namespace ItemDisplayPlacementHelper.Editable
{
    public class EditableItemDisplayRuleSet
    {
        public List<EditableDisplayRuleGroup> DisplayRuleGroups { get; } = new List<EditableDisplayRuleGroup>();

        public static EditableItemDisplayRuleSet From(ItemDisplayRuleSet ruleSet)
        {
            var editable = new EditableItemDisplayRuleSet();
            if (ruleSet)
            {
                foreach (var ruleGroup in ruleSet.keyAssetRuleGroups)
                {
                    if (ruleGroup.keyAsset is not ItemDef && ruleGroup.keyAsset is not EquipmentDef)
                    {
                        ItemDisplayPlacementHelperPlugin.InstanceLogger.LogWarning("Rule group keyAsset is invalid, skipping");
                        continue;
                    }

                    editable.DisplayRuleGroups.Add(EditableDisplayRuleGroup.From(ruleGroup));
                }
            }
            return editable;
        }
    }
}
