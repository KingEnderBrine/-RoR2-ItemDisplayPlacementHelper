using System;
using System.Collections.Generic;
using UnityEngine;

namespace ItemDisplayPlacementHelper.Dialogs
{
    public class DialogController : MonoBehaviour
    {
        private static DialogController Instance { get; set; }

        public GameObject container;
        public Transform background;

        [Space]
        public GameObject textPrefab;
        public GameObject prefabPickerPrefab;
        public GameObject keyAssetPickerPrefab;
        public GameObject filePathPickerPrefab;
        public GameObject exportPrefab;
        public GameObject importPrefab;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public static void ShowError(string text)
        {
            if (!Instance)
            {
                return;
            }

            var controller = SpawnDialog<TextDialogController>(Instance.textPrefab);
            controller.textComponent.text = text;
        }

        public static void ShowPrefabPicker(string guid, string assetBundle, string path, PrefabPickHandler onPick)
        {
            if (!Instance)
            {
                return;
            }

            var controller = SpawnDialog<PrefabPickerDialogController>(Instance.prefabPickerPrefab);
            controller.onPick = onPick;
            if (!string.IsNullOrEmpty(guid))
            {
                controller.typeDropdown.value = (int)PrefabSource.Addressables;
                controller.guidInput.text = guid;
            }
            else
            {
                controller.typeDropdown.value = (int)PrefabSource.AssetBundle;
                controller.assetBundleInput.text = assetBundle;
                controller.pathInput.text = path;
            }
        }

        public static void ShowKeyAssetPicker(IEnumerable<UnityEngine.Object> rows, KeyAssetPickHandler onPick)
        {
            if (!Instance)
            {
                return;
            }

            var controller = SpawnDialog<KeyAssetPickerDialogController>(Instance.keyAssetPickerPrefab);
            controller.onPick = onPick;
            controller.CreateRows(rows);
        }

        public static void ShowFilePathPicker(bool selectFile, bool allowNonExisting, string fileExtension, string defaultFileName, FilePathPickHandler onPick)
        {
            if (!Instance)
            {
                return;
            }

            var controller = SpawnDialog<FilePathPickerDialogController>(Instance.filePathPickerPrefab);
            controller.onPick = onPick;
            controller.selectFile = selectFile;
            controller.allowNotExisting = allowNonExisting;
            controller.fileExtension = fileExtension;
            controller.fileField.text = defaultFileName ?? "";
        }

        public static void ShowExport(ExportHandler onExport)
        {
            if (!Instance)
            {
                return;
            }

            var controller = SpawnDialog<ExportDialogController>(Instance.exportPrefab);
            controller.onExport = onExport;
        }

        public static void ShowImport(ImportHandler onImport)
        {
            if (!Instance)
            {
                return;
            }

            var controller = SpawnDialog<ImportDialogController>(Instance.importPrefab);
            controller.onImport = onImport;
        }

        public static void HideTopmost()
        {
            if (!Instance)
            {
                return;
            }

            var childCount = Instance.container.transform.childCount;
            if (childCount == 1)
            {
                return;
            }

            GameObject.Destroy(Instance.container.transform.GetChild(childCount - 1).gameObject);
            Instance.background.SetSiblingIndex(childCount - 3);
            if (childCount == 2)
            {
                Instance.container.SetActive(false);
            }
        }

        private static T SpawnDialog<T>(GameObject prefab) where T : MonoBehaviour
        {
            Instance.container.SetActive(true);
            Instance.background.SetAsLastSibling();

            var instance = Instantiate(prefab, Instance.container.transform);
            return instance.GetComponent<T>();
        }
    }
}
