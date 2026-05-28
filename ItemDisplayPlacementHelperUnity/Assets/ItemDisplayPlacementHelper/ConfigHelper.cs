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
        public static ConfigFile ConfigFile { get; private set; }
        public static ConfigEntry<float> FastCoefficient { get; private set; }
        public static ConfigEntry<float> SlowCoefficient { get; private set; }
        public static ConfigEntry<CopyFormat> CopyFormat { get; private set; }
        public static ConfigEntry<string> CustomFormat { get; private set; }
        public static ConfigEntry<string> FilePickerLastPath { get; private set; }
        public static ConfigEntry<string> ExportNamespace { get; private set; }
        public static ConfigEntry<bool> ExportGenerateClass { get; private set; }
        public static ConfigEntry<AssetsToExport> ExportAssetsToExport { get; private set; }
        public static ConfigEntry<ImportType> ImportImportType { get; private set; }

        internal static void InitConfigs(ConfigFile config)
        {
            ConfigFile = config;
            config.SaveOnConfigSet = false;
            FastCoefficient = config.Bind("EditorInputs", nameof(FastCoefficient), 2.5F);
            SlowCoefficient = config.Bind("EditorInputs", nameof(SlowCoefficient), 0.1F);
            CopyFormat = config.Bind("EditorInputs", nameof(CopyFormat), ItemDisplayPlacementHelper.CopyFormat.Block);
            CustomFormat = config.Bind("EditorInputs", nameof(CustomFormat), "");
            FilePickerLastPath = config.Bind("Cache", nameof(FilePickerLastPath), "");
            ExportGenerateClass = config.Bind("Cache", nameof(ExportGenerateClass), false);
            ExportNamespace = config.Bind("Cache", nameof(ExportNamespace), "");
            ExportAssetsToExport = config.Bind("Cache", nameof(ExportAssetsToExport), AssetsToExport.All);
            ImportImportType = config.Bind("Cache", nameof(ImportImportType), ImportType.ReplaceSet);
            config.SaveOnConfigSet = true;
            config.Save();
        }
    }
}
