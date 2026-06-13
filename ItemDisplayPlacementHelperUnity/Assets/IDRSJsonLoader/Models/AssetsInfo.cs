using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;

namespace IDRSJsonLoader.Models
{
    internal class AssetsInfo
    {
        public Dictionary<UnityEngine.Object, ExportKeyAssetRuleGroup> groups = new Dictionary<UnityEngine.Object, ExportKeyAssetRuleGroup>();
        public CharacterModel characterModel;
    }
}
