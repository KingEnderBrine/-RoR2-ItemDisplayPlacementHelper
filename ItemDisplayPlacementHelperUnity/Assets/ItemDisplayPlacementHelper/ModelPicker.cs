using Generics.Dynamics;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper
{
    public class ModelPicker : MonoBehaviour
    {
        public SearchableDropdown dropdown;
        public TMP_Dropdown skinsDropdown;
        public Transform modelSpawnPosition;

        [Space]
        public RawImage icon;
        public TextMeshProUGUI bodyNameText;
        public TextMeshProUGUI modelNameText;

        public delegate void OnModelChangedHandler(CharacterModel characterModel);
        public static event OnModelChangedHandler OnModelChanged;

        private ModelPrefabInfo modelInfo;
        private ReverseSkin reverseSkin;
        public GameObject ModelInstance { get; private set; }
        public CharacterModel CharacterModel { get; private set; }
        public ModelSkinController ModelSkinController { get; private set; }
        public Dictionary<SkinnedMeshRenderer, MeshCollider> CachedSkinnedMeshRenderers { get; } = new Dictionary<SkinnedMeshRenderer, MeshCollider>();

        public static ModelPicker Instance { get; private set; }

        private List<SearchableDropdown.OptionData> allBodyOptions;
        private List<SearchableDropdown.OptionData> logbookBodyOptions;

        private void Awake()
        {
            Instance = this;

            logbookBodyOptions = MapBodiesToOptions(
                SurvivorCatalog.orderedSurvivorDefs.Select(survivorDef => BodyCatalog.GetBodyPrefabBodyComponent(SurvivorCatalog.GetBodyIndexFromSurvivorIndex(survivorDef.survivorIndex)))
                .Union(BodyCatalog.allBodyPrefabBodyBodyComponents.Where(characterBody => characterBody && characterBody.GetComponent<DeathRewards>()?.logUnlockableDef)));

            allBodyOptions = MapBodiesToOptions(BodyCatalog.allBodyPrefabBodyBodyComponents);

            dropdown.Options = logbookBodyOptions;

            (dropdown.OnItemSelected ?? (dropdown.OnItemSelected = new SearchableDropdown.DropdownEvent())).AddListener(SelectModel);

            List<SearchableDropdown.OptionData> MapBodiesToOptions(IEnumerable<CharacterBody> bodies)
            {
                return bodies
                    .Select(characterBody => (characterBody, characterModel: characterBody.GetComponentInChildren<CharacterModel>()))
                    .Where(el => el.characterModel)
                    .Select(el =>
                    {
                        var modelInfo = new ModelPrefabInfo
                        {
                            modelPrefab = el.characterModel.gameObject,
                            bodyName = el.characterBody.name,
                            modelName = el.characterModel.name,
                            localizedBodyName = Language.GetString(el.characterBody.baseNameToken),
                            characterBody = el.characterBody
                        };
                        return new SearchableDropdown.OptionData(modelInfo, $"{modelInfo.localizedBodyName} || {modelInfo.bodyName} || {modelInfo.modelName}");
                    })
                    .ToList();
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void SelectModel(object modelInfo)
        {
            if (modelInfo as ModelPrefabInfo == this.modelInfo)
            {
                return;
            }
            DestroyModelInstance();
            this.modelInfo = modelInfo as ModelPrefabInfo;
            BuildModelInstance();
            ConfigureSkinVariants();
            OnModelChanged?.Invoke(CharacterModel);
        }

        private void DestroyModelInstance()
        {
            CachedSkinnedMeshRenderers.Clear();
            Destroy(ModelInstance);

            ModelInstance = null;
            ModelSkinController = null;
            CharacterModel = null;

            icon.color = Color.clear;
            icon.texture = null;

            bodyNameText.text = null;
            modelNameText.text = null;

            reverseSkin = null;
        }

        private void BuildModelInstance()
        {
            if (modelInfo == null || ModelInstance)
            {
                return;
            }
            icon.color = Color.white;
            icon.texture = modelInfo.characterBody.portraitIcon;

            bodyNameText.text = modelInfo.localizedBodyName;
            modelNameText.text = modelInfo.modelName;

            ModelInstance = Instantiate(modelInfo.modelPrefab, modelSpawnPosition.position, modelSpawnPosition.rotation);

            CharacterModel = ModelInstance.GetComponent<CharacterModel>();

            ModelSkinController = ModelInstance.GetComponent<ModelSkinController>();
            if (ModelSkinController && ModelSkinController.skins.Length != 0)
            {
                reverseSkin = new ReverseSkin(ModelInstance, ModelSkinController.skins[Mathf.Clamp(ModelSkinController.currentSkinIndex, 0, ModelSkinController.skins.Length - 1)]);
            }

            foreach (var aimAnimator in ModelInstance.GetComponentsInChildren<AimAnimator>())
            {
                aimAnimator.inputBank = null;
                aimAnimator.directionComponent = null;
                aimAnimator.enabled = false;
            }
            foreach (var animator in ModelInstance.GetComponentsInChildren<Animator>())
            {
                animator.SetBool("isGrounded", true);
                animator.SetFloat("aimPitchCycle", 0.5f);
                animator.SetFloat("aimYawCycle", 0.5f);
                animator.Play("Idle");
                animator.Update(0.0f);
            }
            foreach (var ditherModel in ModelInstance.GetComponentsInChildren<DitherModel>())
            {
                ditherModel.enabled = false;
            }
            foreach (var ikSimpleChain in this.ModelInstance.GetComponentsInChildren<IKSimpleChain>())
            {
                ikSimpleChain.enabled = false;
            }
            foreach (var printController in this.ModelInstance.GetComponentsInChildren<PrintController>())
            {
                printController.enabled = false;
            }
            foreach (var lightIntensityCurve in this.ModelInstance.GetComponentsInChildren<LightIntensityCurve>())
            {
                if (!lightIntensityCurve.loop)
                {
                    lightIntensityCurve.enabled = false;
                }
            }
            foreach (var akEvent in this.ModelInstance.GetComponentsInChildren<AkEvent>())
            {
                akEvent.enabled = false;
            }
            foreach (var shakeEmmiter in ModelInstance.GetComponentsInChildren<ShakeEmitter>())
            {
                shakeEmmiter.enabled = false;
            }

            foreach (var collider in ModelInstance.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }

            foreach (var inverseKinematic in ModelInstance.GetComponentsInChildren<InverseKinematics>())
            {
                inverseKinematic.enabled = false;
            }

            foreach (var meshFilter in ModelInstance.GetComponentsInChildren<MeshFilter>())
            {
                var collider = meshFilter.gameObject.AddComponent<MeshCollider>();
                collider.convex = false;
                collider.sharedMesh = meshFilter.sharedMesh;
                meshFilter.gameObject.layer = 11;
            }

            foreach (var meshRenderer in ModelInstance.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var collider = meshRenderer.gameObject.AddComponent<MeshCollider>();
                collider.convex = false;
                collider.sharedMesh = new Mesh();
                meshRenderer.gameObject.layer = 11;
                CachedSkinnedMeshRenderers[meshRenderer] = collider;
            }
        }

        private void ConfigureSkinVariants()
        {
            skinsDropdown.ClearOptions();
            if (!ModelSkinController)
            {
                skinsDropdown.gameObject.SetActive(false);
                return;
            }

            skinsDropdown.AddOptions(ModelSkinController.skins.Select(el =>
            {
                var name = Language.GetString(el.nameToken);
                return new TMP_Dropdown.OptionData(string.IsNullOrWhiteSpace(name) ? el.name : name, el.icon);
            }).ToList());

            skinsDropdown.gameObject.SetActive(true);
        }

        public void SelectSkin(int index)
        {
            if (!ModelSkinController)
            {
                return;
            }

            var oldIDRS = CharacterModel.itemDisplayRuleSet;

            reverseSkin?.Apply();

            ModelSkinController.ApplySkin(index);
            reverseSkin = new ReverseSkin(ModelInstance, ModelSkinController.skins[ModelSkinController.currentSkinIndex]);

            if (oldIDRS != CharacterModel.itemDisplayRuleSet)
            {
                ItemDisplayRuleSetController.Instance.DisableAll();
                OnModelChanged?.Invoke(CharacterModel);
            }
        }

        public void ToggleBodyOptions(bool enabled)
        {
            if (enabled)
            {
                dropdown.Options = allBodyOptions;
            }
            else
            {
                dropdown.Options = logbookBodyOptions;
            }
        }
    }
}
