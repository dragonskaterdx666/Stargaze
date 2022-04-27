using System;
using UnityEngine;

namespace Stargaze.Mono.Interactions.Inspection
{
    public class InspectInteractable : MonoBehaviour, IInteractable
    {
        private bool _isInteractable = true;
        
        private GameObject _inspectObject;

        [SerializeField] private bool switchable;

        [SerializeField] private GameObject interactionUI;
        
        public bool Switchable => switchable;

        public bool IsInteractable => _isInteractable;

        public void OnInteractionStart()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            interactionUI.SetActive(true);
            
            _inspectObject = Instantiate(gameObject, Vector3.zero, Quaternion.identity);
            _inspectObject.AddComponent<InspectionTurn>();
            _inspectObject.layer = LayerMask.NameToLayer("UI");
        }

        public void OnInteractionEnd()
        {
            RestoreUI();
        }

        private void RestoreUI()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            interactionUI.SetActive(false);
            Destroy(_inspectObject);
        }
    }
}