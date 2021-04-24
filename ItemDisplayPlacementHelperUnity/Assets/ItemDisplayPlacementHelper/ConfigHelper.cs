using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemDisplayPlacementHelper
{
    public static class ConfigHelper
    {
        public static ConfigEntry<float> FastCoefficient { get; private set; }
        public static ConfigEntry<float> SlowCoefficient { get; private set; }
        public static ConfigEntry<int> CopyPrecision { get; private set; }

        internal static void InitConfigs(ConfigFile config)
        {
            FastCoefficient = config.Bind("EditorInputs", nameof(FastCoefficient), 2.5F);
            SlowCoefficient = config.Bind("EditorInputs", nameof(SlowCoefficient), 0.1F);
            CopyPrecision = config.Bind("EditorInputs", nameof(CopyPrecision), 4);
        }
    }
}
