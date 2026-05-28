using System.Collections;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using ItemDisplayPlacementHelper.Native;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace ItemDisplayPlacementHelper
{
    [BepInPlugin(Guid, Name, Version)]
    public class ItemDisplayPlacementHelperPlugin : BaseUnityPlugin
    {
        public const string Guid = "com.KingEnderBrine.ItemDisplayPlacementHelper";
        public const string Name = "Item Display Placement Helper";
        public const string Version = "1.8.0";

        internal static ItemDisplayPlacementHelperPlugin Instance { get; private set; }
        internal static ManualLogSource InstanceLogger { get => Instance?.Logger; }

        private void Awake()
        {
            Instance = this;

            NativeHelpers.Init();
            AssetsHelper.LoadAssetBundle();
            ConfigHelper.InitConfigs(Config);
        }

        private void OnDestroy()
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

            Console.instance.SubmitCmd(null, "host 0");
            yield return new WaitUntil(() => PreGameController.instance != null);
            NetworkManager.singleton.ServerChangeScene("KingEnderBrine_IDRS_Editor");

            if (ConsoleWindow.instance)
            {
                Destroy(ConsoleWindow.instance.gameObject);
            }
        }
    }
}