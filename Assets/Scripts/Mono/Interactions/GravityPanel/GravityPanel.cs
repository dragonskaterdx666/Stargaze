using System;
using Mirror;
using Stargaze.Mono.Puzzle;
using UnityEngine;

namespace Stargaze.Mono.Interactions.GravityPanel
{
    public class GravityPanel : NetworkBehaviour
    {
        public delegate void OnGravityStatusChange(bool gravityStatus);
        
        public static OnGravityStatusChange OnGravitySwitch;

        private PuzzleManager _puzzleManager;
        
        [Header("Codes")]
        [SerializeField] private string deactivateGravityCode;
        [SerializeField] private string activateGravityCode;

        [Header("Audio")]
        [SerializeField] private AudioClip gravityOn;
        [SerializeField] private AudioClip gravityOff;
        [SerializeField] private AudioClip codeReset;
        [SerializeField] private AudioClip buttonPress;

        private string _currentUnlockCode;
        private string _currentCode = "";
        private bool _isGravityOn = true;

        private AudioSource _source;

        public override void OnStartServer()
        {
            deactivateGravityCode = deactivateGravityCode.ToLower();
            activateGravityCode = activateGravityCode.ToLower();

            _currentUnlockCode = deactivateGravityCode;
        }

        private void Start()
        {
            _puzzleManager = PuzzleManager.Instance;
            _source = GetComponent<AudioSource>();
        }

        private void OnButtonPressed(char value)
        {
            CmdAddCharToCode(value);
            _source.PlayOneShot(buttonPress);
        }

        [Command(requiresAuthority = false)]
        private void CmdAddCharToCode(char input)
        {
            //Add the touched button input to the code
            _currentCode += input;

            if (_currentCode.Length > _currentUnlockCode.Length)
            {
                _currentCode = $"{input}";
            }
            
            //If the player didn't put the correct button pattern, the panel will reset the code
            if (input != _currentUnlockCode[_currentCode.Length - 1])
            {
                _currentCode = "";
                
                _source.PlayOneShot(codeReset);
            }

#if DEBUG
            Debug.Log("Code: " + _currentCode);
#endif
            
            if (_currentCode != _currentUnlockCode) return;

            if (_currentUnlockCode == deactivateGravityCode)
                DeactivateGravity();
            else
                ActivateGravity();

            _currentCode = "";
        }

        [Server]
        private void DeactivateGravity()
        {
            _puzzleManager.DeactivateGravity();
            
            _isGravityOn = _puzzleManager.GravityStatus;
            OnGravitySwitch?.Invoke(_isGravityOn);
            
            RpcGravityStatusChanged(_puzzleManager.GravityStatus);
#if DEBUG
            Debug.Log("Gravity successfully deactivated");
#endif
            
            _currentUnlockCode = activateGravityCode;
        }

        [Server]
        private void ActivateGravity()
        {
            _puzzleManager.ActivateGravity();
            
            _isGravityOn = _puzzleManager.GravityStatus;

            RpcGravityStatusChanged(_puzzleManager.GravityStatus);
#if DEBUG
            Debug.Log("Gravity successfully activated");
#endif
            _currentUnlockCode = deactivateGravityCode;
        }

        [ClientRpc]
        private void RpcGravityStatusChanged(bool status)
        {
#if DEBUG
            Debug.Log($"{nameof(GravityPanel)}::{nameof(RpcGravityStatusChanged)}({status})");
#endif
            
            OnGravitySwitch?.Invoke(status);

            _source.PlayOneShot(status == true ? gravityOn : gravityOff);
        }

        private void OnEnable()
        {
            GravityPanelButton.SendButtonInput += OnButtonPressed;
        }

        private void OnDisable()
        {
            GravityPanelButton.SendButtonInput -= OnButtonPressed;
        }
    }
}
