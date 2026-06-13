using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HG.Reflection;
using RoR2;
using IDRSJsonLoader.Models;
using System.Linq;
using HG.Coroutines;
using UnityEngine.AddressableAssets;
using System;
using System.IO;
using R2API;

[assembly:SearchableAttribute.OptIn]
namespace IDRSJsonLoader
{
    public delegate void ParseCallback(ItemDisplayRuleSet idrs);

    [BepInPlugin(Guid, Name, Version)]
    public class IDRSJsonLoaderPlugin : BaseUnityPlugin
    {
        public const string Guid = "com.KingEnderBrine.IDRSJsonLoader";
        public const string Name = "IDRS Json Loader";
        public const string Version = "1.0.3";

        internal static IDRSJsonLoaderPlugin Instance { get; private set; }
        internal static ManualLogSource InstanceLogger { get => Instance?.Logger; }

        private static readonly List<DelayedReplacementInfo> delayedReplacementInfos = new List<DelayedReplacementInfo>();

        private void Awake()
        {
            Instance = this;

            foreach (var file in Directory.GetFiles(Paths.PluginPath, "*.idrsjson", SearchOption.AllDirectories))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    ParseAndUpdate(json, null);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"Couldn't load file {file}");
                    Logger.LogError(ex);
                }
            }
        }

        public static void ParseAndUpdate(string json, ParseCallback beforeGenerateRuntimeValues)
        {
            var info = new DelayedReplacementInfo
            {
                exportIdrs = JsonUtility.FromJson<ExportItemDisplayRuleSet>(json),
                beforeGenerateRuntimeValues = beforeGenerateRuntimeValues,
            };
            delayedReplacementInfos.Add(info);
        }

        public static void ParseAndUpdate(string json, ItemDisplayRuleSet idrs, ParseCallback beforeGenerateRuntimeValues)
        {
            var info = new DelayedReplacementInfo
            {
                exportIdrs = JsonUtility.FromJson<ExportItemDisplayRuleSet>(json),
                idrs = idrs,
                beforeGenerateRuntimeValues = beforeGenerateRuntimeValues,
            };
            delayedReplacementInfos.Add(info);
        }

        [SystemInitializer(typeof(ItemDisplayRuleSet), typeof(BodyCatalog), typeof(SkinCatalog))]
        internal static IEnumerator Init()
        {
            var assetBundles = AssetBundle.GetAllLoadedAssetBundles().ToDictionary(b => b.name);
            var assetsByIdrs = new Dictionary<ItemDisplayRuleSet, AssetsInfo>();
            var assetsBySkinDef = new Dictionary<SkinDef, AssetsInfo>();

            foreach (var replacementInfo in delayedReplacementInfos)
            {
                var exportIdrs = replacementInfo.exportIdrs;
                if (!replacementInfo.idrs)
                {
                    if (!string.IsNullOrEmpty(exportIdrs.bodyName))
                    {
                        var bodyPrefab = BodyCatalog.FindBodyPrefab(exportIdrs.bodyName);
                        if (bodyPrefab)
                        {
                            var body = bodyPrefab.GetComponent<CharacterBody>();
                            var model = bodyPrefab.GetComponent<ModelLocator>()?.modelTransform?.GetComponent<CharacterModel>();
                            if (model)
                            {
                                replacementInfo.characterModel = model;
                                if (!model.itemDisplayRuleSet)
                                {
                                    model.itemDisplayRuleSet = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
                                }
                                if (!string.IsNullOrEmpty(exportIdrs.skinName))
                                {
                                    var skinDefs = SkinCatalog.GetBodySkinDefs(body.bodyIndex);
                                    replacementInfo.skinDef = skinDefs.FirstOrDefault(s => s.name == exportIdrs.skinName);
                                }
                                else
                                {
                                    replacementInfo.idrs = model.itemDisplayRuleSet;
                                }
                            }
                        }
                    }
                }

                AssetsInfo assetsInfo = null;
                if (replacementInfo.idrs)
                {
                    if (!assetsByIdrs.TryGetValue(replacementInfo.idrs, out assetsInfo))
                    {
                        assetsByIdrs[replacementInfo.idrs] = assetsInfo = new AssetsInfo();
                    }
                }
                else if (replacementInfo.skinDef)
                {
                    if (!assetsBySkinDef.TryGetValue(replacementInfo.skinDef, out assetsInfo))
                    {
                        assetsBySkinDef[replacementInfo.skinDef] = assetsInfo = new AssetsInfo();
                    }
                }

                if (assetsInfo is not null)
                {
                    assetsInfo.characterModel ??= replacementInfo.characterModel;
                    for (int i = 0; i < exportIdrs.itemGroups.Count; i++)
                    {
                        var group = exportIdrs.itemGroups[i];
                        var keyAsset = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(group.name));
                        if (keyAsset)
                        {
                            assetsInfo.groups[keyAsset] = group;
                        }
                    }
                    for (int i = 0; i < exportIdrs.equipmentGroups.Count; i++)
                    {
                        var group = exportIdrs.equipmentGroups[i];
                        var keyAsset = EquipmentCatalog.GetEquipmentDef(EquipmentCatalog.FindEquipmentIndex(group.name));
                        if (keyAsset)
                        {
                            assetsInfo.groups[keyAsset] = group;
                        }
                    }
                }
            }

            ParallelCoroutine assetsCoroutine = null;
            foreach (var (idrs, assetsInfo) in assetsByIdrs)
            {
                var addedChildren = new HashSet<string>(0);
                for (var i = 0; i < idrs.keyAssetRuleGroups.Length; i++)
                {
                    var keyAsset = idrs.keyAssetRuleGroups[i].keyAsset;
                    if (keyAsset && assetsInfo.groups.Remove(keyAsset, out var group))
                    {
                        var enumerator = MapGroup(idrs, i, keyAsset, group, assetBundles, assetsInfo.characterModel, addedChildren);
                        if (enumerator.MoveNext())
                        {
                            assetsCoroutine ??= new ParallelCoroutine();
                            assetsCoroutine.Add(enumerator);
                        }
                    }
                }

                if (assetsInfo.groups.Count == 0)
                {
                    continue;
                }

                var offset = idrs.keyAssetRuleGroups.Length;
                Array.Resize(ref idrs.keyAssetRuleGroups, idrs.keyAssetRuleGroups.Length + assetsInfo.groups.Count);
                foreach (var (keyAsset, group) in assetsInfo.groups)
                {
                    var enumerator = MapGroup(idrs, offset++, keyAsset, group, assetBundles, assetsInfo.characterModel, addedChildren);
                    if (enumerator.MoveNext())
                    {
                        assetsCoroutine ??= new ParallelCoroutine();
                        assetsCoroutine.Add(enumerator);
                    }
                }
            }

            foreach (var (skinDef, assetsInfo) in assetsBySkinDef)
            {
                if (assetsInfo.groups.Count == 0)
                {
                    continue;
                }

                var addedChildren = new HashSet<string>(0);
                foreach (var (keyAsset, group) in assetsInfo.groups)
                {
                    var enumerator = MapGroup(skinDef, keyAsset, group, assetBundles, assetsInfo.characterModel, addedChildren);
                    if (enumerator.MoveNext())
                    {
                        assetsCoroutine ??= new ParallelCoroutine();
                        assetsCoroutine.Add(enumerator);
                    }
                }
            }

            if (assetsCoroutine is not null)
            {
                while (assetsCoroutine.MoveNext())
                {
                    yield return null;
                }
            }

            var idrsCoroutine = new ParallelCoroutine();
            foreach (var replacementInfo in delayedReplacementInfos)
            {
                if (replacementInfo.idrs)
                {
                    replacementInfo.beforeGenerateRuntimeValues?.Invoke(replacementInfo.idrs);
                    idrsCoroutine.Add(replacementInfo.idrs.GenerateRuntimeValuesAsync());
                }
            }

            while (idrsCoroutine.MoveNext())
            {
                yield return null;
            }

            delayedReplacementInfos.Clear();
        }

        private static IEnumerator MapGroup(ItemDisplayRuleSet idrs, int index, UnityEngine.Object keyAsset, ExportKeyAssetRuleGroup exportGroup, Dictionary<string, AssetBundle> assetBundles, CharacterModel characterModel, HashSet<string> addedChildren)
        {
            ParallelCoroutine coroutine = null;
            var group = new ItemDisplayRuleSet.KeyAssetRuleGroup();
            group.displayRuleGroup.rules = new ItemDisplayRule[exportGroup.rules.Count];
            group.keyAsset = keyAsset;
            for (var i = 0; i < exportGroup.rules.Count; i++)
            {
                var enumerator = MapRule(group.displayRuleGroup.rules, i, exportGroup.rules[i], assetBundles, characterModel, addedChildren);
                if (enumerator.MoveNext())
                {
                    coroutine ??= new ParallelCoroutine();
                    coroutine.Add(enumerator);
                }
            }

            if (coroutine is not null)
            {
                while (coroutine.MoveNext())
                {
                    yield return null;
                }
            }

            idrs.keyAssetRuleGroups[index] = group;
        }

        private static IEnumerator MapGroup(SkinDef skinDef, UnityEngine.Object keyAsset, ExportKeyAssetRuleGroup exportGroup, Dictionary<string, AssetBundle> assetBundles, CharacterModel characterModel, HashSet<string> addedChildren)
        {
            ParallelCoroutine coroutine = null;
            var group = new DisplayRuleGroup();
            group.rules = new ItemDisplayRule[exportGroup.rules.Count];
            for (var i = 0; i < exportGroup.rules.Count; i++)
            {
                var enumerator = MapRule(group.rules, i, exportGroup.rules[i], assetBundles, characterModel, addedChildren);
                if (enumerator.MoveNext())
                {
                    coroutine ??= new ParallelCoroutine();
                    coroutine.Add(enumerator);
                }
            }

            if (coroutine is not null)
            {
                while (coroutine.MoveNext())
                {
                    yield return null;
                }
            }

            SkinIDRS.AddGroupOverride(skinDef, keyAsset, group);
        }

        private static IEnumerator MapRule(ItemDisplayRule[] rules, int index, ExportItemDisplayRule exportRule, Dictionary<string, AssetBundle> assetBundles, CharacterModel characterModel, HashSet<string> addedChildren)
        {
            var childName = exportRule.childName;
            if (!string.IsNullOrEmpty(exportRule.childPath) && characterModel)
            {
                childName = StringHelpers.ChildNameFromPath(exportRule.childPath);
                if (!addedChildren.Contains(childName))
                {
                    addedChildren.Add(childName);
                    var child = characterModel.transform.Find(exportRule.childPath);
                    if (child)
                    {
                        var locator = characterModel.GetComponent<ChildLocator>();
                        locator.AddChild(childName, child);
                    }
                }
            }

            var rule = new ItemDisplayRule
            {
                childName = childName,
                followerPrefabAddress = new AssetReferenceGameObject(exportRule.guid ?? ""),
                limbMask = exportRule.limbMask,
                localAngles = exportRule.localAngles,
                localPos = exportRule.localPos,
                localScale = exportRule.localScale,
                ruleType = exportRule.ruleType,
            };

            if (!string.IsNullOrEmpty(exportRule.assetBundle))
            {
                if (assetBundles.TryGetValue(exportRule.assetBundle, out var assetBundle))
                {
                    var request = assetBundle.LoadAssetAsync<GameObject>(exportRule.assetPath);
                    while (!request.isDone)
                    {
                        yield return null;
                    }
                    rule.followerPrefab = request.asset as GameObject;
                }
                else
                {
                    InstanceLogger.LogError($"AssetBundle {exportRule.assetBundle} was not found.");
                }
            }

            rules[index] = rule;
        }
    }
}
