using RoR2;
using RoR2.ContentManagement;
using System.Collections.Generic;
using UnityEngine;

namespace ItemDisplayPlacementHelper
{
    //Literally `LobbySkinsFix` except IL stuff is removed
    public class ReverseSkin
    {
        private readonly List<CharacterModel.RendererInfo> baseRendererInfos = new List<CharacterModel.RendererInfo>();
        private readonly List<SkinDef.GameObjectActivationTemplate> gameObjectActivationTemplates = new List<SkinDef.GameObjectActivationTemplate>();
        private readonly List<SkinDef.MeshReplacementTemplate> meshReplacementTemplates = new List<SkinDef.MeshReplacementTemplate>();

        private readonly GameObject modelObject;

        public ReverseSkin(GameObject modelObject, SkinDef skinDef)
        {
            this.modelObject = modelObject;
            var bakeEnumerator = skinDef.BakeAsync();
            while (bakeEnumerator.MoveNext());
            var runtimeSkin = skinDef.runtimeSkin;

            baseRendererInfos.AddRange(modelObject.GetComponent<CharacterModel>().baseRendererInfos);
            foreach (var objectActivation in runtimeSkin.gameObjectActivationTemplates)
            {
                gameObjectActivationTemplates.Add(new SkinDef.GameObjectActivationTemplate
                {
                    transformPath = objectActivation.transformPath,
                    shouldActivate = !objectActivation.shouldActivate
                });
            }
            foreach (var meshReplacement in runtimeSkin.meshReplacementTemplates)
            {
                var renderer = modelObject.transform.Find(meshReplacement.transformPath).GetComponent<Renderer>();

                Mesh mesh = null;
                switch (renderer)
                {
                    case MeshRenderer _:
                        mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                        break;
                    case SkinnedMeshRenderer skinnedMeshRenderer:
                        mesh = skinnedMeshRenderer.sharedMesh;
                        break;
                }

                meshReplacementTemplates.Add(new SkinDef.MeshReplacementTemplate
                {
                    transformPath = meshReplacement.transformPath,
                    meshReference = new AssetOrDirectReference<Mesh> { directRef = mesh }
                });
            }
        }

        public void Apply()
        {
            if (!modelObject)
            {
                return;
            }

            var transform = modelObject.transform;
            modelObject.GetComponent<CharacterModel>().baseRendererInfos = baseRendererInfos.ToArray();

            foreach (var objectActivation in gameObjectActivationTemplates)
            {
                transform.Find(objectActivation.transformPath).gameObject.SetActive(objectActivation.shouldActivate);
            }
            foreach (var meshReplacement in meshReplacementTemplates)
            {
                Renderer component = transform.Find(meshReplacement.transformPath).GetComponent<Renderer>();
                switch (component)
                {
                    case MeshRenderer _:
                        component.GetComponent<MeshFilter>().sharedMesh = meshReplacement.meshReference.directRef;
                        break;
                    case SkinnedMeshRenderer skinnedMeshRenderer:
                        skinnedMeshRenderer.sharedMesh = meshReplacement.meshReference.directRef;
                        break;
                }
            }
        }
    }
}