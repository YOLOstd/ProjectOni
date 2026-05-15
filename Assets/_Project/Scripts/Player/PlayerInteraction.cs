using UnityEngine;
using ProjectOni.Core;
using System.Collections.Generic;
using PurrNet;

namespace ProjectOni.Player
{
    public class PlayerInteraction : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _interactionRadius = 1.5f;
        [SerializeField] private LayerMask _interactableLayer;

        private IInteractable _nearestInteractable;

        private void OnEnable()
        {
            if (isSpawned && isOwner)
            {
                var input = ProjectOni.Managers.InputManager.Instance;
                if (input != null) input.InteractPressed += OnInteractPressed;
            }
        }

        private void OnDisable()
        {
            if (isSpawned && isOwner)
            {
                var input = ProjectOni.Managers.InputManager.Instance;
                if (input != null) input.InteractPressed -= OnInteractPressed;
            }
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();
            if (isOwner)
            {
                var input = ProjectOni.Managers.InputManager.Instance;
                if (input != null) input.InteractPressed += OnInteractPressed;
            }
        }

        protected override void OnDespawned(bool asServer)
        {
            base.OnDespawned(asServer);
            if (isOwner)
            {
                var input = ProjectOni.Managers.InputManager.Instance;
                if (input != null) input.InteractPressed -= OnInteractPressed;
            }
        }

        private void Update()
        {
            FindNearestInteractable();
        }

        private void FindNearestInteractable()
        {
            Collider2D[] results = Physics2D.OverlapCircleAll(transform.position, _interactionRadius, _interactableLayer);
            
            float minDistance = float.MaxValue;
            _nearestInteractable = null;

            foreach (var col in results)
            {
                if (col.TryGetComponent(out IInteractable interactable))
                {
                    float distance = Vector2.Distance(transform.position, col.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        _nearestInteractable = interactable;
                    }
                }
            }
        }

        private void OnInteractPressed()
        {
            if (_nearestInteractable != null)
            {
                // If we are the server (host), we can interact immediately
                if (isServer)
                {
                    _nearestInteractable.Interact(gameObject);
                }
                else
                {
                    // Otherwise, request the server to interact for us
                    // We pass the GameObject of the interactable
                    var interactableComp = _nearestInteractable as Component;
                    if (interactableComp != null)
                    {
                        RequestInteractServerRpc(interactableComp.gameObject);
                    }
                }
            }
        }

        [ServerRpc]
        private void RequestInteractServerRpc(GameObject interactableObject)
        {
            if (interactableObject != null && interactableObject.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact(gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _interactionRadius);
        }
    }
}
