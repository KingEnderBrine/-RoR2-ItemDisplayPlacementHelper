using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ItemDisplayPlacementHelper.Assets.ItemDisplayPlacementHelper;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper.Dialogs
{
    public delegate void KeyAssetPickHandler(UnityEngine.Object keyAsset);
    public class KeyAssetPickerDialogController : MonoBehaviour
    {
        public TMP_InputField searchInput;
        public GameObject rowPrefab;
        public Transform container;

        public KeyAssetPickHandler onPick;

        private readonly List<KeyAssetRowController> rows = new List<KeyAssetRowController>();
        private KeyAssetRowController selectedRow;

        public void CreateRows(IEnumerable<UnityEngine.Object> keyAssets)
        {
            foreach (var row in rows)
            {
                Destroy(row.gameObject);
            }
            rows.Clear();

            foreach (var (keyAsset, (icon, iconRef, name, subName)) in keyAssets.Select(a => (a, GetItemInfo(a))).OrderBy(e => string.IsNullOrEmpty(e.Item2.name)).ThenBy(e => e.Item2.name))
            {
                var rowInstance = Instantiate(rowPrefab, container);
                var row = rowInstance.GetComponent<KeyAssetRowController>();

                row.text.text = name;
                row.subText.text = subName;
                row.keyAsset = keyAsset;
                rowInstance.SetActive(true);

                if (iconRef != null && iconRef.RuntimeKeyIsValid())
                {
                    row.StartCoroutine(LoadIconCoroutine(iconRef, row));
                }
                else
                {
                    row.icon.sprite = icon;
                }

                rows.Add(row);
            }
        }

        private IEnumerator LoadIconCoroutine(AssetReferenceT<Sprite> iconRef, KeyAssetRowController row)
        {
            var handle = iconRef.LoadAssetAsync();
            while (!handle.IsDone)
            {
                yield return null;
            }
            row.icon.sprite = handle.Result;
        }

        public void ApplyFilter(string filter)
        {
            var filterIsEmpty = string.IsNullOrEmpty(filter);

            foreach (var row in rows)
            {
                var active = filterIsEmpty || row.text.text.ContainsInSequence(filter) || row.subText.text.ContainsInSequence(filter);
                row.gameObject.SetActive(active);
            }
        }

        public void SelectRow(KeyAssetRowController row)
        {
            if (selectedRow)
            {
                selectedRow.button.interactable = true;
            }

            row.button.interactable = false;
            selectedRow = row;
        }

        public void Ok()
        {
            if (!selectedRow)
            {
                return;
            }

            onPick?.Invoke(selectedRow.keyAsset);

            DialogController.HideTopmost();
        }

        public void Cancel()
        {
            DialogController.HideTopmost();
        }

        private (Sprite icon, AssetReferenceT<Sprite>, string name, string subName) GetItemInfo(UnityEngine.Object keyAsset)
        {
            if (keyAsset is ItemDef item)
            {
                return (item.GetIcon(), item.GetIconReference(), Language.GetString(item.nameToken), item.name);
            }

            if (keyAsset is EquipmentDef equipment)
            {
                return (equipment.GetIcon(), equipment.GetIconReference(), Language.GetString(equipment.nameToken), equipment.name);
            }

            return default;
        }
    }
}
