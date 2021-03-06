using DG.Tweening;
using Mirror;
using NaughtyAttributes;
using Stargaze.Mono.Puzzle;
using UnityEngine;

namespace Stargaze.Mono.Door
{
    public class Door : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnDoorStateChangedCallback))]
        [SerializeField] private bool isOpened;
        [SyncVar]
        [SerializeField] private bool requirePower = true;

        private Animator _doorAnimator;

        private void Awake()
        {
            _doorAnimator = GetComponent<Animator>();
        }

        public override void OnStartClient()
        {
            _doorAnimator.SetBool("Opened" , isOpened);
        }

        private void OnDoorStateChangedCallback(bool oldValue, bool newValue)
        {
            
            _doorAnimator.SetBool("Opened" , newValue);
        }

        [Server]
        public void OpenDoor()
        {
            isOpened = true;
            
            RpcOpenDoor();
        }

        [ClientRpc]
        private void RpcOpenDoor()
        {
            Debug.Log($"Door: {gameObject.name} has been opened");

            _doorAnimator.SetBool("Opened", true);
            GetComponent<AudioSource>().Play();
        }

        [Server]
        public void CloseDoor()
        {
            isOpened = false;

            RpcCloseDoor();
        }

        [ClientRpc]
        private void RpcCloseDoor()
        {
            Debug.Log($"Door: {gameObject.name} has been closed");
            
            _doorAnimator.SetBool("Opened", false);
            GetComponent<AudioSource>().Play();
        }

        [Command(requiresAuthority = false)]
        public void CmdToggleDoor()
        {
            if (requirePower)
            {
                if (!PuzzleManager.Instance.IsPowerOn() || !PuzzleManager.Instance.GravityStatus)
                {
                    Debug.Log("Can't open door! Turn Power On or activate gravity");
                    return;
                }
            }
            
            if (isOpened)
                CloseDoor();
            else
                OpenDoor();
        }
    }
}
