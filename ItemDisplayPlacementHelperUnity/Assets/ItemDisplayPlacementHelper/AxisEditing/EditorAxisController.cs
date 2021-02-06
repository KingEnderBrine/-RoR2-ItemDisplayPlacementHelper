using UnityEngine;

namespace ItemDisplayPlacementHelper.AxisEditing
{
    public class EditorAxisController : MonoBehaviour
    {
        public Transform SelectedObject { get; private set; }

        public float normalScaleCameraDistance = 7.5F;
        public float minScale = 0.001F;
        [SerializeField]
        private GameObject moveAxisObject;
        [SerializeField]
        private GameObject scaleAxisObject;
        [SerializeField]
        private GameObject rotateAxisObject;

        public static EditorAxisController Instance { get; private set; }

        [HideInInspector]
        public Axis SelectedAxis { get; set; }

        private EditMode editMode;
        [HideInInspector]
        public EditMode EditMode
        {
            get => editMode;
            set
            {
                if (editMode == value)
                {
                    return;
                }

                editMode = value;

                OnEditModeChanged();
            }
        }

        public bool OverrideToLocalSpace { get; private set; }
        public EditSpace ActualEditSpace { get; set; }
        public EditSpace EditSpace => OverrideToLocalSpace ? EditSpace.Local : ActualEditSpace;

        private void Awake()
        {
            Instance = this;

            DisableAxisObjects();
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void SetSelectedObject(Transform newObject)
        {
            SelectedObject = newObject;

            OnEditModeChanged();
        }

        private void OnEditModeChanged()
        {
            DisableAxisObjects();

            if (!SelectedObject)
            {
                return;
            }

            switch (EditMode)
            {
                case EditMode.Move:
                    moveAxisObject.SetActive(true);
                    break;
                case EditMode.Scale:
                    scaleAxisObject.SetActive(true);
                    break;
                case EditMode.Rotate:
                    rotateAxisObject.SetActive(true);
                    break;
            }
        }

        private void DisableAxisObjects()
        {
            moveAxisObject.SetActive(false);
            scaleAxisObject.SetActive(false);
            rotateAxisObject.SetActive(false);
            OverrideToLocalSpace = EditMode == EditMode.Scale;
        }

        private void LateUpdate()
        {
            if (!SelectedObject)
            {
                return;
            }
            
            transform.position = SelectedObject.transform.position;

            var positionInScreenSpace = Camera.main.WorldToScreenPoint(transform.position);
            var distanceToCamera = positionInScreenSpace.z;

            if (distanceToCamera < 0)
            {
                transform.localScale = Vector3.one * minScale;
            }
            else
            {
                var newScale = distanceToCamera / normalScaleCameraDistance;

                if (newScale < 1)
                {
                    newScale = Mathf.Pow(newScale, 0.85F);
                }

                transform.localScale = Vector3.one * Mathf.Max(minScale, newScale);
            }
            switch (EditSpace)
            {
                case EditSpace.Global:
                    transform.rotation = Quaternion.identity;
                    break;
                case EditSpace.Local:
                    transform.rotation = SelectedObject.transform.rotation;
                    break;
            }
        }
    }
}
