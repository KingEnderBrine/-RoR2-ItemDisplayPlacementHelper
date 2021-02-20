using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ItemDisplayPlacementHelper
{
    [RequireComponent(typeof(TMP_InputField))]
    public class NavigateInputFieldOnTab : MonoBehaviour
    {
        private TMP_InputField inputField;

        public MoveDirection moveDirection;

        private void Awake()
        {
            inputField = GetComponent<TMP_InputField>();
        }

        private void Update()
        {
            if (!inputField.interactable || !inputField.isFocused || inputField.navigation.mode == Navigation.Mode.None)
            {
                return;
            }
            if (!Input.GetKeyDown(KeyCode.Tab))
            {
                return;
            }
            Selectable target = null;
            switch (moveDirection)
            {
                case MoveDirection.Left:
                    target = inputField.FindSelectableOnLeft();
                    break;
                case MoveDirection.Up:
                    target = inputField.FindSelectableOnUp();
                    break;
                case MoveDirection.Right:
                    target = inputField.FindSelectableOnRight();
                    break;
                case MoveDirection.Down:
                    target = inputField.FindSelectableOnDown();
                    break;
            }
            if (target)
            {
                target.Select();
            }
        }
    }
}