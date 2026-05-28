using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper.Dialogs
{
    public delegate void ImportHandler(string path, ImportType importType);
    public class ImportDialogController : MonoBehaviour
    {
        public TMP_Dropdown importTypeDropdown;
        public TMP_InputField pathField;

        public ImportHandler onImport;

        public void Awake()
        {
            importTypeDropdown.AddOptions(Enum.GetNames(typeof(ImportType)).Select(name => new TMP_Dropdown.OptionData(name.FormatAsSplitWords())).ToList());
            importTypeDropdown.value = (int)ConfigHelper.ImportImportType.Value;
        }

        public void SelectPath()
        {
            DialogController.ShowFilePathPicker(
                true,
                false,
                "idrsjson",
                "",
                (path) =>
                {
                    pathField.text = path;
                });
        }

        public void Ok()
        {
            if (!File.Exists(pathField.text))
            {
                DialogController.ShowError("File not found");
                return;
            }

            var importType = (ImportType)importTypeDropdown.value;

            ConfigHelper.ConfigFile.SaveOnConfigSet = false;
            ConfigHelper.ImportImportType.Value = importType;
            ConfigHelper.ConfigFile.SaveOnConfigSet = true;
            ConfigHelper.ConfigFile.Save();

            onImport?.Invoke(pathField.text, importType);

            DialogController.HideTopmost();
        }

        public void Cancel()
        {
            DialogController.HideTopmost();
        }
    }
}
