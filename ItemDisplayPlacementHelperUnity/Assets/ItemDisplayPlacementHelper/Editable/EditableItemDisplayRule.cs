using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ItemDisplayPlacementHelper.Editable
{
    public class EditableItemDisplayRule
    {
        public ItemDisplayRuleType ruleType;
        public GameObject followerPrefab;
        public AssetReferenceGameObject followerPrefabAddress;
        public string childName;
        public Vector3 localPos;
        public Vector3 localAngles;
        public Vector3 localScale;
        public LimbFlags limbMask;
        public string assetBundle;
        public string assetPath;
        public bool referenceDiscovered;

        public bool enabled;

        public GameObject instance;

        public static EditableItemDisplayRule From(ItemDisplayRule itemDisplayRule)
        {
            return new EditableItemDisplayRule
            {
                childName = itemDisplayRule.childName,
                followerPrefab = itemDisplayRule.followerPrefab,
                followerPrefabAddress = new AssetReferenceGameObject(itemDisplayRule.followerPrefabAddress?.AssetGUID),
                limbMask = itemDisplayRule.limbMask,
                localAngles = itemDisplayRule.localAngles,
                localPos = itemDisplayRule.localPos,
                localScale = itemDisplayRule.localScale,
                ruleType = itemDisplayRule.ruleType,
                referenceDiscovered = !string.IsNullOrEmpty(itemDisplayRule.followerPrefabAddress?.AssetGUID)
            };
        }

        public void Enable(CharacterModel characterModel)
        {
            if (enabled)
            {
                return;
            }

            enabled = true;

            switch (ruleType)
            {
                case ItemDisplayRuleType.ParentedPrefab:
                {
                    TrySpawnInstance(characterModel);
                    break;
                }
                case ItemDisplayRuleType.LimbMask:
                {
                    characterModel.limbFlagSet.AddFlags(limbMask);
                    characterModel.materialsDirty = true;
                    break;
                }
            }
        }

        public void TrySpawnInstance(CharacterModel characterModel)
        {
            if (instance)
            {
                return;
            }

            var transform = characterModel.childLocator.transformPairs.FirstOrDefault(p => p.name == childName).transform;
            if (!transform)
            {
                return;
            }

            var display = new CharacterModel.ParentedPrefabDisplay();
            var localRotation = Quaternion.Euler(localAngles);
            if (followerPrefab)
            {
                display.Apply(characterModel, followerPrefab, transform, localPos, localRotation, localScale);
            }
            else if (followerPrefabAddress?.RuntimeKeyIsValid() ?? false)
            {
                display.Apply(characterModel, followerPrefabAddress, transform, localPos, localRotation, localScale);
                followerPrefab = display.prefabReference.Result;
            }
            else
            {
                return;
            }

            instance = display.instance;

            var itemFollower = instance.GetComponent<ItemFollower>();
            if (itemFollower && itemFollower.followerPrefab)
            {
                itemFollower.StartCoroutine(AddComponentToFollowerInstanceCoroutine(itemFollower));
            }
        }

        public void Disable(CharacterModel characterModel)
        {
            if (!enabled)
            {
                return;
            }

            enabled = false;

            if (instance)
            {
                GameObject.Destroy(instance);
                instance = null;
            }

            if (ruleType == ItemDisplayRuleType.LimbMask)
            {
                characterModel.limbFlagSet.RemoveFlags(limbMask);
                characterModel.materialsDirty = true;
            }
        }

        private IEnumerator AddComponentToFollowerInstanceCoroutine(ItemFollower itemFollower)
        {
            yield return new WaitUntil(() => itemFollower.followerInstance);

            if (!itemFollower.followerInstance.GetComponent<MatchLocalScale>())
            {
                var matchLocalScale = itemFollower.followerInstance.AddComponent<MatchLocalScale>();
                matchLocalScale.target = itemFollower.transform;
            }
        }
    }
}
