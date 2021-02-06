using RoR2;
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
            skinDef.Bake();
            var runtimeSkin = skinDef.runtimeSkin;

            baseRendererInfos.AddRange(modelObject.GetComponent<CharacterModel>().baseRendererInfos);
            foreach (var objectActivation in runtimeSkin.gameObjectActivationTemplates)
            {
                gameObjectActivationTemplates.Add(new SkinDef.GameObjectActivationTemplate
                {
                    path = objectActivation.path,
                    shouldActivate = !objectActivation.shouldActivate
                });
            }
            foreach (var meshReplacement in runtimeSkin.meshReplacementTemplates)
            {
                var renderer = modelObject.transform.Find(meshReplacement.path).GetComponent<Renderer>();

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
                    path = meshReplacement.path,
                    mesh = mesh
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
                transform.Find(objectActivation.path).gameObject.SetActive(objectActivation.shouldActivate);
            }
            foreach (var meshReplacement in meshReplacementTemplates)
            {
                Renderer component = transform.Find(meshReplacement.path).GetComponent<Renderer>();
                switch (component)
                {
                    case MeshRenderer _:
                        component.GetComponent<MeshFilter>().sharedMesh = meshReplacement.mesh;
                        break;
                    case SkinnedMeshRenderer skinnedMeshRenderer:
                        skinnedMeshRenderer.sharedMesh = meshReplacement.mesh;
                        break;
                }
            }
        }
    }
}