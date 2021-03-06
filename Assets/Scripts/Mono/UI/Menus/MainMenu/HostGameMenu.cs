using System;
using Stargaze.Mono.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Stargaze.Mono.UI.Menus.MainMenu
{
    public class HostGameMenu : MonoBehaviour
    {
        private InputAction _goBackAction;

        [Header("Fields")]
        [SerializeField] private TMP_InputField lobbyNameInput;
        [SerializeField] private TMP_Dropdown lobbyPrivacyDropdown;

        public Action OnMenuQuit;
        
        private void Awake()
        {
            _goBackAction = new InputAction(binding: "<Keyboard>/escape"); // TODO: How can I add a second binding for gamepad?

            _goBackAction.performed += _ =>
            {
                OnMenuQuit?.Invoke();
            };
        }

        public void OnHostGameButtonPressed()
        {
            string lobbyName = lobbyNameInput.text;
            bool isPublic = lobbyPrivacyDropdown.value == 0;
            
            SteamLobby.Instance.HostLobby(lobbyName, isPublic);
        }
        
        public void OnGoBackButtonPressed()
        {
            OnMenuQuit?.Invoke();
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        private void OnEnable()
        {
            _goBackAction.Enable();
        }

        private void OnDisable()
        {
            _goBackAction.Disable();
        }
    }
}