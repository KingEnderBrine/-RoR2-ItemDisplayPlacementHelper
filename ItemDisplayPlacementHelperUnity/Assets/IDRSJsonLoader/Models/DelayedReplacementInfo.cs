using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;

namespace IDRSJsonLoader.Models
{
    internal class DelayedReplacementInfo
    {
        public ExportItemDisplayRuleSet exportIdrs;
        public ParseCallback beforeGenerateRuntimeValues;
        internal ItemDisplayRuleSet idrs;
    }
}
