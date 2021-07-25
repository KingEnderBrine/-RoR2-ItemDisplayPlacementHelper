using BepInEx;
using BepInEx.Logging;
using MonoMod.RuntimeDetour.HookGen;
using RoR2;
using RoR2.Networking;
using System;
using System.Collections;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace ItemDisplayPlacementHelper
{
    [BepInPlugin("com.KingEnderBrine.ItemDisplayPlacementHelper", "Item Display Placement Helper", "1.4.2")]
    public class ItemDisplayPlacementHelperPlugin : BaseUnityPlugin
    {
        private static readonly MethodInfo mainMenuControllerStart = typeof(RoR2.UI.MainMenu.MainMenuController).GetMethod(nameof(RoR2.UI.MainMenu.MainMenuController.Start), BindingFlags.NonPublic | BindingFlags.Instance);

        internal static ItemDisplayPlacementHelperPlugin Instance { get; private set; }
        internal static ManualLogSource InstanceLogger { get => Instance?.Logger; }

        private void Awake()
        {
            Instance = this;

            AssetsHelper.LoadAssetBundle();
            ConfigHelper.InitConfigs(Config);

            HookEndpointManager.Add(mainMenuControllerStart, (Action<Action<RoR2.UI.MainMenu.MainMenuController>, RoR2.UI.MainMenu.MainMenuController>)MainMenuController_Start);
        }

        private void MainMenuController_Start(Action<RoR2.UI.MainMenu.MainMenuController> orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            HookEndpointManager.Remove(mainMenuControllerStart, (Action<Action<RoR2.UI.MainMenu.MainMenuController>, RoR2.UI.MainMenu.MainMenuController>)MainMenuController_Start);

            PostProcessingValuesInit.PostProcessResources = typeof(PostProcessLayer).GetField("m_Resources", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(self.cameraTransform.GetComponentInChildren<PostProcessLayer>()) as PostProcessResources;
            PostProcessingValuesInit.PostProcessProfile = self.cameraTransform.GetComponentInChildren<PostProcessVolume>().sharedProfile;
        }

        private void Destroy()
        {
            Instance = null;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2) && SceneManager.GetActiveScene().name == "title")
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