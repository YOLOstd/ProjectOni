using System;
using UnityEngine;

namespace ProjectOni.Player
{
    public class DodgeController : MonoBehaviour
    {
        private PlayerController _playerController;

        public bool CanDodge { get; private set; } = true;
        public bool IsDodging { get; private set; }
        public bool IsInvincible { get; private set; }
        private bool _hasAirDodged;
        private float _dodgeCooldownTimer;
        private Vector2 _dodgeDir;
        private float _activeDodgePower;

        public event Action OnDodgeStarted;
        public event Action OnDodgeEnded;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
            if (_playerController == null) _playerController = GetComponentInParent<PlayerController>();
            if (_playerController == null) _playerController = GetComponentInChildren<PlayerController>();
        }

        private void OnEnable()
        {
            if (_playerController != null)
            {
                _playerController.GroundedChanged += OnGroundedChanged;
            }
        }

        private void OnDisable()
        {
            if (_playerController != null)
            {
                _playerController.GroundedChanged -= OnGroundedChanged;
            }
        }

        public void Initiate(Vector2 inputDir)
        {
            CanDodge = false;
            IsDodging = true;
            IsInvincible = true;
            _dodgeCooldownTimer = _playerController.Stats.DodgeCooldown;

            if (!_playerController.IsGrounded)
            {
                _hasAirDodged = true;
            }

            _dodgeDir = inputDir != Vector2.zero ? inputDir.normalized : new Vector2(_playerController.FacingDir, 0);
            _activeDodgePower = _playerController.IsGrounded ? _playerController.Stats.DodgePower : _playerController.Stats.AirDodgePower;

            _playerController.SetVelocity(_dodgeDir * _activeDodgePower);

            OnDodgeStarted?.Invoke();
        }

        public void HandleMovement()
        {
            _playerController.SetVelocity(_dodgeDir * _activeDodgePower);
        }

        public void End()
        {
            IsDodging = false;
            IsInvincible = false;

            if (!_playerController.IsGrounded)
            {
                _hasAirDodged = true;
                Vector2 currentVel = _playerController.GetVelocity();
                float endSpeed = _playerController.Stats.DodgeEndSpeed;
                _playerController.SetVelocity(new Vector2(
                    Mathf.Clamp(currentVel.x, -endSpeed, endSpeed),
                    Mathf.Clamp(currentVel.y, -endSpeed, endSpeed)
                ));
            }

            OnDodgeEnded?.Invoke();
        }

        public void UpdateCooldown()
        {
            if (_dodgeCooldownTimer > 0)
            {
                _dodgeCooldownTimer -= Time.deltaTime;
                if (_dodgeCooldownTimer <= 0)
                {
                    if (_playerController.IsGrounded || !_hasAirDodged)
                    {
                        CanDodge = true;
                      
                    }
                }
            }
        }

        private void OnGroundedChanged(bool grounded, float impact)
        {
            if (grounded)
            {
                _hasAirDodged = false;
                // Only restore CanDodge if the cooldown has actually finished
                if (_dodgeCooldownTimer <= 0)
                {
                    CanDodge = true;
                }
            }
        }
    }
}
