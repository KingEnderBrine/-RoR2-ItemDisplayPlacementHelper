using System.Collections.Generic;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper.AnimatorEditing
{
    public class AnimatorPanelController : MonoBehaviour
    {
        private CharacterModel currentModel;
        private Animator currentAnimator;

        public TMP_Dropdown layerDropdown;
        public TMP_InputField stateInput;
        public Button playButton;

        [Space]
        public Toggle animatorToggle;

        private void Awake()
        {
            ModelPicker.OnModelChanged += OnModelChanged;
        }

        private void OnDestroy()
        {
            ModelPicker.OnModelChanged -= OnModelChanged;
        }

        private void Update()
        {
            layerDropdown.interactable = currentAnimator;
            stateInput.interactable = currentAnimator;
            playButton.interactable = currentAnimator;
            animatorToggle.interactable = currentAnimator;
        }

        private void OnModelChanged(CharacterModel model)
        {
            layerDropdown.options.Clear();
            currentAnimator = null;

            currentModel = model;
            if (currentModel)
            {
                currentAnimator = currentModel.GetComponent<Animator>();
            }
            if (currentAnimator)
            {
                currentAnimator.enabled = animatorToggle.isOn;
                var options = new List<TMP_Dropdown.OptionData>();
                for (var i = 0; i < currentAnimator.layerCount; i++)
                {
                    options.Add(new TMP_Dropdown.OptionData(currentAnimator.GetLayerName(i)));
                }
                layerDropdown.AddOptions(options);
            }
        }

        public void PlayAnimation()
        {
            if (!currentAnimator)
            {
                return;
            }

            currentAnimator.Play(stateInput.text, layerDropdown.value);
        }

        public void ToggleAnimator(bool enabled)
        {
            if (currentAnimator)
            {
                currentAnimator.enabled = enabled;
            }
        }
    }
}
