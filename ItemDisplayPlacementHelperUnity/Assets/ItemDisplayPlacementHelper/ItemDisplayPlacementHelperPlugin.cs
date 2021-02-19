using BepInEx;
using BepInEx.Logging;
using R2API.Utils;
using RoR2;
using RoR2.Networking;
using System.Collections;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace ItemDisplayPlacementHelper
{
    [R2APISubmoduleDependency(nameof(CommandHelper))]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("com.KingEnderBrine.ItemDisplayPlacementHelper", "Item Display Placement Helper", "1.1.0")]
    public class ItemDisplayPlacementHelperPlugin : BaseUnityPlugin
    {
        internal static ItemDisplayPlacementHelperPlugin Instance { get; private set; }
        internal static ManualLogSource InstanceLogger { get => Instance?.Logger; }

        private void Awake()
        {
            Instance = this;

            AssetsHelper.LoadAssetBundle();
            ConfigHelper.InitConfigs(Config);

            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
        }

        private void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            On.RoR2.UI.MainMenu.MainMenuController.Start -= MainMenuController_Start;

            PostProcessingValuesInit.PostProcessResources = self.cameraTransform.GetComponent<PostProcessLayer>().GetFieldValue<PostProcessResources>("m_Resources");
            PostProcessingValuesInit.PostProcessProfile = self.cameraTransform.GetComponentInChildren<PostProcessVolume>().sharedProfile;
        }

        private void Destroy()
        {
            Instance = null;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                StartCoroutine(StartSceneCoroutine());
            }
        }

        //For now I didn't found a better way to start my scene that will allow me to use pause screen.
        //Pause screen require NetworkManager.singleton.isNetworkActive == true
        private IEnumerator StartSceneCoroutine()
        {
            if (NetworkManager.networkSceneName == "KingEnderBrine_IDRS_Editor")
            {
                yield break;
            }
            GameNetworkManager.singleton.desiredHost = new GameNetworkManager.HostDescription(new GameNetworkManager.HostDescription.HostingParameters
            {
                listen = false,
                maxPlayers = 1
            });
            yield return new WaitUntil(() => PreGameController.instance != null);
            NetworkManager.singleton.ServerChangeScene("KingEnderBrine_IDRS_Editor");
        }
    }
}