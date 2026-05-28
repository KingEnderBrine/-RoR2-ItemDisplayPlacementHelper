using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ItemDisplayPlacementHelper.Dialogs
{
    public delegate void PrefabPickHandler(GameObject gameObject, AssetReferenceGameObject reference, string assetBundle, string path);
    public class PrefabPickerDialogController : MonoBehaviour
    {
        public TMP_Dropdown typeDropdown;
        public TMP_InputField guidInput;
        public TMP_InputField assetBundleInput;
        public TMP_InputField pathInput;
        public TMP_Text errorText;

        public PrefabPickHandler onPick;

        private Dictionary<string, AssetBundle> allBundles;

        public void Awake()
        {
            typeDropdown.AddOptions(Enum.GetNames(typeof(PrefabSource)).Select(name => new TMP_Dropdown.OptionData(name)).ToList());
            OnTypeChanged(0);
        }

        public void OnTypeChanged(int index)
        {
            switch ((PrefabSource)index)
            {
                case PrefabSource.Addressables:
                {
                    guidInput.transform.parent.gameObject.SetActive(true);
                    assetBundleInput.transform.parent.gameObject.SetActive(false);
                    pathInput.transform.parent.gameObject.SetActive(false);
                    break;
                }
                case PrefabSource.AssetBundle:
                {
                    guidInput.transform.parent.gameObject.SetActive(false);
                    assetBundleInput.transform.parent.gameObject.SetActive(true);
                    pathInput.transform.parent.gameObject.SetActive(true);
                    break;
                }
            }
            ResetError();
        }

        public void ResetError()
        {
            errorText.text = "";
            errorText.transform.parent.gameObject.SetActive(false);
        }

        public void Ok()
        {
            GameObject prefab = null;
            AssetReferenceGameObject reference = null;
            var assetBundle = "";
            var path = "";
            try
            {
                switch ((PrefabSource)typeDropdown.value)
                {
                    case PrefabSource.Addressables:
                    {
                        reference = new AssetReferenceGameObject(guidInput.text);
                        if (reference.RuntimeKeyIsValid())
                        {
                            prefab = Addressables.LoadAssetAsync<GameObject>(reference).WaitForCompletion();
                        }
                        break;
                    }
                    case PrefabSource.AssetBundle:
                    {
                        reference = new AssetReferenceGameObject("");
                        assetBundle = assetBundleInput.text;
                        path = pathInput.text;

                        allBundles ??= AssetBundle.GetAllLoadedAssetBundles().ToDictionary(b => b.name);
                        if (allBundles.TryGetValue(assetBundle, out var bundle))
                        {
                            prefab = bundle.LoadAsset<GameObject>(path);
                        }
                        break;
                    }
                }

            }
            catch (Exception e)
            {
                ItemDisplayPlacementHelperPlugin.InstanceLogger.LogError(e);
            }

            if (!prefab)
            {
                errorText.text = "Failed to load prefab";
                errorText.transform.parent.gameObject.SetActive(true);
                return;
            }

            onPick?.Invoke(prefab, reference, assetBundle, path);

            DialogController.HideTopmost();
        }

        public void Cancel()
        {
            DialogController.HideTopmost();
        }
    }
}
