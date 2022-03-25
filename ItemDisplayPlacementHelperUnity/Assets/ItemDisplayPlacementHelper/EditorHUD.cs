using ItemDisplayPlacementHelper.AxisEditing;
using RoR2;
using RoR2.UI;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace ItemDisplayPlacementHelper
{
    public class EditorHUD : MonoBehaviour
    {
        public static EditorHUD Instance { get; private set; }

        public EditorSceneCameraController cameraController;
        public CameraRigController cameraRigController;

        private float oldTimeScale;

        private void Awake()
        {
            Instance = this;
            
            oldTimeScale = Time.timeScale;
            Time.timeScale = 1;

            AkSoundEngine.PostEvent("Pause_All", null);
            PauseManager.onPauseEndGlobal += OnPauseEnd;
        }

        private void Start()
        {
            GetComponent<CursorOpener>().enabled = true;
        }

        private void OnDestroy()
        {
            Instance = null;

            PauseManager.onPauseEndGlobal -= OnPauseEnd;

            Time.timeScale = oldTimeScale;
         
            AkSoundEngine.PostEvent("Unpause_All", null);
        }

        private void Update()
        {
            if (Time.timeScale < 0.01)
            {
                Physics.SyncTransforms();
            }
        }

        private void OnPauseEnd()
        {
            AkSoundEngine.PostEvent("Pause_All", null);
        }
    }
}
