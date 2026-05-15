using UnityEngine;
using ProjectOni.Player;
using ProjectOni.Data;
using PurrNet;

namespace ProjectOni.Player
{
    /// <summary>
    /// VERY primitive animator example.
    /// </summary>
    public class PlayerAnimator : NetworkBehaviour
    {
        [SerializeField]
        private Animator _anim;

        [SerializeField] private NetworkAnimator _netAnimator;
        [SerializeField] private SpriteRenderer _sprite;

        [Header("Settings")] [SerializeField, Range(1f, 3f)]
        private float _maxIdleSpeed = 2;

        [Header("Particles")] [SerializeField] private ParticleSystem _jumpParticles;
        [SerializeField] private ParticleSystem _launchParticles;
        [SerializeField] private ParticleSystem _moveParticles;
        [SerializeField] private ParticleSystem _landParticles;
        [SerializeField] private ParticleSystem _dodgeParticles;

        [Header("Audio Clips")] [SerializeField]
        private AudioClip[] _footsteps;

        private AudioSource _source;
        private PlayerController _player;
        private ProjectOni.Player.Movement.PlayerMovementStateMachine _stateMachine;
        private EquipmentManager _equipment;
        private bool _grounded;
        private ParticleSystem.MinMaxGradient _currentGradient;
        
        private System.Collections.Generic.HashSet<int> _validParameters = new();

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _player = GetComponentInParent<PlayerController>();
            _stateMachine = GetComponentInParent<ProjectOni.Player.Movement.PlayerMovementStateMachine>();
            _equipment = GetComponentInParent<EquipmentManager>();

            if (_netAnimator == null) _netAnimator = GetComponentInChildren<NetworkAnimator>();
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();
            if (isOwner)
            {
                _player.Jumped += SendJumpRpc;
                _player.GroundedChanged += SendGroundedRpc;
            }
            _stateMachine.machine.onStateChanged += OnStateChanged;

            if (_equipment != null)
            {
                _equipment.activeWeaponVisual.onChangedWithOld += OnWeaponChanged;
                // Initial update for late joiners or current state
                UpdateWeaponVisual(default, _equipment.activeWeaponVisual.value);
            }
        }

        protected override void OnDespawned(bool asServer)
        {
            base.OnDespawned(asServer);
            if (isOwner)
            {
                _player.Jumped -= SendJumpRpc;
                _player.GroundedChanged -= SendGroundedRpc;
            }
            _stateMachine.machine.onStateChanged -= OnStateChanged;

            if (_equipment != null)
            {
                _equipment.activeWeaponVisual.onChangedWithOld -= OnWeaponChanged;
            }
        }

        private void OnEnable()
        {
            if (isSpawned && isOwner)
            {
                _player.Jumped += SendJumpRpc;
                _player.GroundedChanged += SendGroundedRpc;
            }

            CacheParameters();

            if (_moveParticles != null) _moveParticles.Play();
        }

        private void CacheParameters()
        {
            _validParameters.Clear();
            if (_anim == null) return;
            foreach (var param in _anim.parameters) _validParameters.Add(param.nameHash);
        }

        private void OnDisable()
        {
            if (isSpawned && isOwner)
            {
                _player.Jumped -= SendJumpRpc;
                _player.GroundedChanged -= SendGroundedRpc;
            }

            if (_moveParticles != null) _moveParticles.Stop();
        }

        private Vector3 _lastPosition;
        private Vector2 _visualVelocity;

        private void Update()
        {
            if (_player == null) return;

            _visualVelocity = (transform.position - _lastPosition) / Time.deltaTime;
            _lastPosition = transform.position;

            DetectGroundColor();
            HandleSpriteFlip();
            
            HandleMovementStats();
            UpdateWallVisuals();
        }

        private void UpdateWallVisuals()
        {
            if (_player.isOwner)
            {
                SafeSetBool(WallSlidingKey, _player.IsWallSliding.value);
            }
        }

        private void OnStateChanged(PurrNet.StateMachine.StateNode prev, PurrNet.StateMachine.StateNode next)
        {
            if (next is ProjectOni.Player.Movement.PlayerDodgeState)
            {
                if (_dodgeParticles != null) _dodgeParticles.Play();
            }
            else if (prev is ProjectOni.Player.Movement.PlayerDodgeState)
            {
                if (_dodgeParticles != null) _dodgeParticles.Stop();
            }
        }

        private void HandleSpriteFlip()
        {
            if (_player.FacingDir != 0) 
                _sprite.flipX = _player.FacingDir < 0;
        }

        private void HandleMovementStats()
        {
            float inputStrength = 0;
            if (_player.isOwner)
            {
                var input = ProjectOni.Managers.InputManager.Instance;
                if (input != null) inputStrength = Mathf.Abs(input.MoveDirection.x);
            }
            else
            {
                inputStrength = Mathf.Abs(_visualVelocity.x) > 0.1f ? 1f : 0f;
            }

            var velocity = _player.isOwner ? _player.CurrentVelocity : _visualVelocity;

            if (_player.isOwner)
            {
                SafeSetFloat(MoveSpeedKey, inputStrength);
                SafeSetFloat(VerticalVelocityKey, velocity.y);
                SafeSetBool(GroundedBoolKey, _player.IsGrounded);
                SafeSetFloat(IdleSpeedKey, Mathf.Lerp(1, _maxIdleSpeed, inputStrength));
            }

            if (_moveParticles != null) 
                _moveParticles.transform.localScale = Vector3.MoveTowards(_moveParticles.transform.localScale, Vector3.one * inputStrength, 2 * Time.deltaTime);
        }

        private void SendJumpRpc()
        {
            if (_netAnimator != null) _netAnimator.SetTrigger(JumpKey);
            RpcOnJumped();
        }

        private void SendGroundedRpc(bool grounded, float impact)
        {
            if (grounded && _netAnimator != null) _netAnimator.SetTrigger(GroundedKey);
            RpcOnGroundedChanged(grounded, impact);
        }

        [ObserversRpc(runLocally: true, requireServer: false)]
        private void RpcOnJumped()
        {
            // Animator trigger handled by NetworkAnimator.SetTrigger on owner.
            // This RPC now only handles side-effects.

            if (_grounded) // Avoid coyote particles if we're technically in air
            {
                SetColor(_jumpParticles);
                SetColor(_launchParticles);
                if (_jumpParticles != null) _jumpParticles.Play();
            }
        }

        [ObserversRpc(runLocally: true, requireServer: false)]
        private void RpcOnGroundedChanged(bool grounded, float impact)
        {
            _grounded = grounded;
            
            if (grounded)
            {
                DetectGroundColor();
                SetColor(_landParticles);

                // Side-effects only. Triggers are handled via NetworkAnimator.

                if (_source != null && _footsteps != null && _footsteps.Length > 0) 
                    _source.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Length)]);
                if (_moveParticles != null) _moveParticles.Play();

                if (_landParticles != null) _landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, 40, impact);
                if (_landParticles != null) _landParticles.Play();
            }
            else
            {
                if (_moveParticles != null) _moveParticles.Stop();
            }
        }

        private void DetectGroundColor()
        {
            var hit = Physics2D.Raycast(transform.position, Vector3.down, 2);

            if (!hit || hit.collider.isTrigger || !hit.transform.TryGetComponent(out SpriteRenderer r)) return;
            var color = r.color;
            _currentGradient = new ParticleSystem.MinMaxGradient(color * 0.9f, color * 1.2f);
            if (_moveParticles != null) SetColor(_moveParticles);
        }

        private void SetColor(ParticleSystem ps)
        {
            if (ps == null) return;
            var main = ps.main;
            main.startColor = _currentGradient;
        }

        #region Safe Animator Helpers
        private void SafeSetFloat(int hash, float value) { if (_validParameters.Contains(hash)) _anim.SetFloat(hash, value); }
        private void SafeSetBool(int hash, bool value) { if (_validParameters.Contains(hash)) _anim.SetBool(hash, value); }
        private void SafeSetTrigger(int hash) { if (_validParameters.Contains(hash)) _anim.SetTrigger(hash); }
        private void SafeResetTrigger(int hash) { if (_validParameters.Contains(hash)) _anim.ResetTrigger(hash); }
        #endregion

        [SerializeField] private Transform _weaponHoldPoint;
        private GameObject _spawnedWeaponVisual;

        private void OnWeaponChanged(EquipmentInstance oldVal, EquipmentInstance newVal)
        {
            UpdateWeaponVisual(oldVal, newVal);
        }

        private void UpdateWeaponVisual(EquipmentInstance oldVal, EquipmentInstance newVal)
        {
            // Destroy previous visual
            if (_spawnedWeaponVisual != null)
            {
                Destroy(_spawnedWeaponVisual);
                _spawnedWeaponVisual = null;
            }

            // Only spawn if new item is valid and has a prefab
            if (!newVal.IsValid)
            {
                Debug.Log($"[PlayerAnimator] Weapon visual updated to invalid/empty for {gameObject.name}");
                return;
            }

            if (newVal.blueprint == null)
            {
                Debug.LogError($"[PlayerAnimator] FAILED to sync EquipmentBlueprint! Is it in a Resources folder? {gameObject.name}");
                return;
            }

            if (newVal.blueprint.visualPrefab == null)
            {
                Debug.LogWarning($"[PlayerAnimator] Weapon {newVal.blueprint.itemName} has no visual prefab!");
                return;
            }

            if (_weaponHoldPoint == null)
            {
                Debug.LogWarning($"[PlayerAnimator] No weapon hold point assigned on {gameObject.name}!");
                return;
            }

            // Spawn and parent
            _spawnedWeaponVisual = Instantiate(newVal.blueprint.visualPrefab, _weaponHoldPoint);
            _spawnedWeaponVisual.transform.localPosition = Vector3.zero;
            _spawnedWeaponVisual.transform.localRotation = Quaternion.identity;
            
            Debug.Log($"[PlayerAnimator] Visual SPAWNED: {newVal.blueprint.itemName} on {gameObject.name} (isOwner: {isOwner})");
        }

        private static readonly int GroundedKey = Animator.StringToHash("Grounded");
        private static readonly int GroundedBoolKey = Animator.StringToHash("IsGrounded");
        private static readonly int IdleSpeedKey = Animator.StringToHash("IdleSpeed");
        private static readonly int MoveSpeedKey = Animator.StringToHash("MoveSpeed");
        private static readonly int VerticalVelocityKey = Animator.StringToHash("VerticalVelocity");
        private static readonly int JumpKey = Animator.StringToHash("Jump");
        private static readonly int DodgingKey = Animator.StringToHash("Dodging");

        private static readonly int WallSlidingKey = Animator.StringToHash("WallSliding");
        private static readonly int DodgeTriggerKey = Animator.StringToHash("Dodge");
    }
}