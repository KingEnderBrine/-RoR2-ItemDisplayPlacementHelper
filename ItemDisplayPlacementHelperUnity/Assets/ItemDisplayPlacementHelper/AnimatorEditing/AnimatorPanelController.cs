using System;
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

        public TMP_Dropdown animatorDropdown;
        public TMP_Dropdown layerDropdown;
        public TMP_InputField stateInput;
        public Button playButton;

        [Space]
        public Toggle animatorToggle;

        private readonly List<Animator> animators = new List<Animator>();
        public static AnimatorPanelController Instance { get; private set; }
        public Animator CurrentAnimator { get; private set; }

        public static event Action<Animator> OnAnimatorChanged;

        private void Awake()
        {
            Instance = this;
            ModelPicker.OnModelChanged += OnModelChanged;
            ModelPicker.OnModelWillChange += OnModelWillChange;
        }

        private void OnDestroy()
        {
            ModelPicker.OnModelChanged -= OnModelChanged;
            ModelPicker.OnModelWillChange -= OnModelWillChange;
            Instance = null;
        }

        private void Update()
        {
            animatorDropdown.interactable = currentModel;
            layerDropdown.interactable = CurrentAnimator;
            stateInput.interactable = CurrentAnimator;
            playButton.interactable = CurrentAnimator;
            animatorToggle.interactable = CurrentAnimator;
        }

        private void OnModelWillChange()
        {
            layerDropdown.options.Clear();
            CurrentAnimator = null;
            animators.Clear();
            animatorDropdown.options.Clear();
            OnAnimatorChanged?.Invoke(null);
        }

        private void OnModelChanged(CharacterModel model)
        {
            currentModel = model;
            if (currentModel)
            {
                animators.AddRange(model.GetComponentsInChildren<Animator>(true));
                var animatorOptions = new List<TMP_Dropdown.OptionData>();
                for (var i = 0; i < animators.Count; i++)
                {
                    animatorOptions.Add(new TMP_Dropdown.OptionData(animators[i].name));
                }
                animatorDropdown.AddOptions(animatorOptions);
            }

            SelectAnimator(0);
        }

        public void SelectAnimator(int index)
        {
            if (animators.Count <= (uint)index)
            {
                return;
            }

            CurrentAnimator = animators[index];
            animatorToggle.isOn = CurrentAnimator.enabled;
            var layerOptions = new List<TMP_Dropdown.OptionData>();
            for (var i = 0; i < CurrentAnimator.layerCount; i++)
            {
                layerOptions.Add(new TMP_Dropdown.OptionData(CurrentAnimator.GetLayerName(i)));
            }
            layerDropdown.AddOptions(layerOptions);
            OnAnimatorChanged?.Invoke(CurrentAnimator);
        }

        public void PlayAnimation()
        {
            if (!CurrentAnimator)
            {
                return;
            }

            CurrentAnimator.Play(stateInput.text, layerDropdown.value);
        }

        public void ToggleAnimator(bool enabled)
        {
            if (CurrentAnimator)
            {
                CurrentAnimator.enabled = enabled;
            }
        }
    }
}
