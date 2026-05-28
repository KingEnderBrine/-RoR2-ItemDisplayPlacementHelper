using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDRSJsonLoader.Models
{
    [Serializable]
    public class ExportKeyAssetRuleGroup
    {
        public string name;
        public List<ExportItemDisplayRule> rules = new List<ExportItemDisplayRule>();
    }
}
