using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper.Dialogs
{

    public delegate void ExportHandler(string path, AssetsToExport assetsToExport, bool generateClass, string @namespace);
    public class ExportDialogController : MonoBehaviour
    {
        public TMP_Dropdown exportSourceDropdown;
        public TMP_InputField pathField;
        public Toggle classToggle;
        public TMP_InputField namespaceField;

        public ExportHandler onExport;

        public void Awake()
        {
            exportSourceDropdown.AddOptions(Enum.GetNames(typeof(AssetsToExport)).Select(name => new TMP_Dropdown.OptionData(name.FormatAsSplitWords())).ToList());
            exportSourceDropdown.value = (int)ConfigHelper.ExportAssetsToExport.Value;
            classToggle.isOn = ConfigHelper.ExportGenerateClass.Value;
            namespaceField.text = ConfigHelper.ExportNamespace.Value;
        }

        public void SelectPath()
        {
            DialogController.ShowFilePathPicker(
                true,
                true,
                "idrsjson",
                $"{ModelPicker.Instance.ModelInfo.bodyName}IDRS.idrsjson",
                (path) =>
                {
                    pathField.text = path;
                });
        }

        public void Ok()
        {
            var path = pathField.text;
            if (path.Length == 0 || path.EndsWith('\\'))
            {
                DialogController.ShowError("File name not specified");
                return;
            }

            path = Path.ChangeExtension(path, ".idrsjson");

            if (Directory.Exists(pathField.text))
            {
                DialogController.ShowError("There is already a directory with specified file name");
                return;
            }

            if (!Directory.Exists(Path.GetDirectoryName(pathField.text)))
            {
                DialogController.ShowError("Directory does not exist");
                return;
            }

            var assetsToExport = (AssetsToExport)exportSourceDropdown.value;
            var generateClass = classToggle.isOn;
            var @namespace = namespaceField.text;

            ConfigHelper.ConfigFile.SaveOnConfigSet = false;
            ConfigHelper.ExportAssetsToExport.Value = assetsToExport;
            ConfigHelper.ExportGenerateClass.Value = generateClass;
            ConfigHelper.ExportNamespace.Value = @namespace;
            ConfigHelper.ConfigFile.SaveOnConfigSet = true;
            ConfigHelper.ConfigFile.Save();

            onExport?.Invoke(pathField.text, assetsToExport, generateClass, @namespace);

            DialogController.HideTopmost();
        }

        public void Cancel()
        {
            DialogController.HideTopmost();
        }
    }
}
