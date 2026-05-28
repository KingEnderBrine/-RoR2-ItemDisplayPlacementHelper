using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using RoR2.AddressableAssets;

namespace ItemDisplayPlacementHelper.Editable
{
    public class EditableDisplayRuleGroup
    {
        public UnityEngine.Object KeyAsset { get; set; }
        public IDRSKeyAssetReference KeyAssetAddress { get; set; }
        public List<EditableItemDisplayRule> Rules { get; } = new List<EditableItemDisplayRule>();
        public bool Enabled { get; private set; }

        public static EditableDisplayRuleGroup From(ItemDisplayRuleSet.KeyAssetRuleGroup keyAssetRuleGroup)
        {
            var editable = new EditableDisplayRuleGroup
            {
                KeyAsset = keyAssetRuleGroup.keyAsset,
                KeyAssetAddress = new IDRSKeyAssetReference(keyAssetRuleGroup.keyAssetAddress?.AssetGUID),
            };
            if (!keyAssetRuleGroup.displayRuleGroup.isEmpty)
            {
                editable.Rules.AddRange(keyAssetRuleGroup.displayRuleGroup.rules.Select(EditableItemDisplayRule.From));
            }

            return editable;
        }

        public void Enable(CharacterModel characterModel)
        {
            if (Enabled)
            {
                return;
            }

            Enabled = true;

            foreach (var rule in Rules)
            {
                rule.Enable(characterModel);
            }
        }

        public void Disable(CharacterModel characterModel)
        {
            if (!Enabled)
            {
                return;
            }

            Enabled = false;

            foreach (var rule in Rules)
            {
                rule.Disable(characterModel);
            }
        }
    }
}
