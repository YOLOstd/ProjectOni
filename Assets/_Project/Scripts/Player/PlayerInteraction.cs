using UnityEngine;
using ProjectOni.Core;
using System.Collections.Generic;

namespace ProjectOni.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _interactionRadius = 1.5f;
        [SerializeField] private LayerMask _interactableLayer;

        private InputReader _input;
        private IInteractable _nearestInteractable;

        private void Awake()
        {
            _input = GetComponentInParent<InputReader>();
            if (_input == null) _input = GetComponentInChildren<InputReader>();
        }

        private void OnEnable()
        {
            if (_input == null) _input = GetComponentInParent<InputReader>();
            
            if (_input != null)
            {
                _input.InteractPressed += OnInteractPressed;
            }
        }

        private void OnDisable()
        {
            if (_input != null)
                _input.InteractPressed -= OnInteractPressed;
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
            if (_input == null) _input = GetComponentInParent<InputReader>();
            
            if (_input == null) return;

            if (_nearestInteractable != null)
            {
                _nearestInteractable.Interact();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _interactionRadius);
        }
    }
}
