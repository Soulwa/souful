using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace soulful
{
    public class PopupUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField songNameInput;
        [SerializeField]
        private Button okButton;
        [SerializeField]
        private CanvasGroup popupGroup;

        public event EventHandler<string> nameChosen;

        private void Start()
        {
            okButton.onClick.AddListener(ValidateName);
        }

        public void Show()
        {
            Debug.Log("showing popup");
            popupGroup.alpha = 1f;
            popupGroup.interactable = true;
        }

        public void Hide()
        {
            popupGroup.alpha = 0f;
            popupGroup.interactable = false;
        }

        public void ValidateName()
        {
            if (string.IsNullOrWhiteSpace(songNameInput.text) || string.IsNullOrWhiteSpace(songNameInput.text))
            {
                return;
            }
            else
            {
                nameChosen?.Invoke(this, songNameInput.text);
            }
        }
    }
}

