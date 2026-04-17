using UnityEngine;
using ProjectOni.Player;

namespace ProjectOni.Player
{
    /// <summary>
    /// VERY primitive animator example.
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Animator _anim;

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
        private bool _grounded;
        private ParticleSystem.MinMaxGradient _currentGradient;
        
        private System.Collections.Generic.HashSet<int> _validParameters = new();

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _player = GetComponentInParent<PlayerController>();
        }

        private void OnEnable()
        {
            _player.Jumped += OnJumped;
            _player.GroundedChanged += OnGroundedChanged;
            _player.DodgingChanged += OnDodgingChanged;
            _player.CrouchingChanged += OnCrouchingChanged;

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
            _player.Jumped -= OnJumped;
            _player.GroundedChanged -= OnGroundedChanged;
            _player.DodgingChanged -= OnDodgingChanged;
            _player.CrouchingChanged -= OnCrouchingChanged;

            if (_moveParticles != null) _moveParticles.Stop();
        }

        private void Update()
        {
            if (_player == null) return;

            DetectGroundColor();
            HandleSpriteFlip();
            HandleMovementStats();
            UpdateWallVisuals();
        }

        private void UpdateWallVisuals()
        {
            SafeSetBool(WallSlidingKey, _player.IsWallSliding && !_player.IsCrouching);
            SafeSetBool(CrouchingKey, _player.IsCrouching);
            SafeSetBool(DodgingKey, _player.IsDodging);
        }

        private void HandleSpriteFlip()
        {
            if (_player.GetComponent<InputReader>().MoveDirection.x != 0) 
                _sprite.flipX = _player.GetComponent<InputReader>().MoveDirection.x < 0;
        }

        private void HandleMovementStats()
        {
            var velocity = _player.CurrentVelocity;
            var inputStrength = Mathf.Abs(_player.GetComponent<InputReader>().MoveDirection.x);

            // MoveSpeed for Idle -> Run transition
            SafeSetFloat(MoveSpeedKey, Mathf.Abs(velocity.x));
            
            // VerticalVelocity for Jump -> Fall transition
            SafeSetFloat(VerticalVelocityKey, velocity.y);

            // Keep IdleSpeed for potential animation variance
            SafeSetFloat(IdleSpeedKey, Mathf.Lerp(1, _maxIdleSpeed, inputStrength));

            if (_moveParticles != null) 
                _moveParticles.transform.localScale = Vector3.MoveTowards(_moveParticles.transform.localScale, Vector3.one * inputStrength, 2 * Time.deltaTime);
        }

        private void OnJumped()
        {
            SafeSetTrigger(JumpKey);
            SafeResetTrigger(GroundedKey);


            if (_grounded) // Avoid coyote
            {
                SetColor(_jumpParticles);
                SetColor(_launchParticles);
                if (_jumpParticles != null) _jumpParticles.Play();
            }
        }

        private void OnGroundedChanged(bool grounded, float impact)
        {
            _grounded = grounded;
            SafeSetBool(GroundedBoolKey, grounded);
            
            if (grounded)
            {
                DetectGroundColor();
                SetColor(_landParticles);

                SafeResetTrigger(JumpKey);
                SafeSetTrigger(GroundedKey);
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

        private void OnDodgingChanged()
        {
            if (_player.IsDodging)
            {
                SafeSetTrigger(DodgeTriggerKey);
                if (_dodgeParticles != null) _dodgeParticles.Play();
            }
            else
            {
                if (_dodgeParticles != null) _dodgeParticles.Stop();
            }
        }

        private void OnCrouchingChanged()
        {
            SafeSetBool(CrouchingKey, _player.IsCrouching);
        }

        #region Safe Animator Helpers
        private void SafeSetFloat(int hash, float value) { if (_validParameters.Contains(hash)) _anim.SetFloat(hash, value); }
        private void SafeSetBool(int hash, bool value) { if (_validParameters.Contains(hash)) _anim.SetBool(hash, value); }
        private void SafeSetTrigger(int hash) { if (_validParameters.Contains(hash)) _anim.SetTrigger(hash); }
        private void SafeResetTrigger(int hash) { if (_validParameters.Contains(hash)) _anim.ResetTrigger(hash); }
        #endregion

        private static readonly int GroundedKey = Animator.StringToHash("Grounded");
        private static readonly int GroundedBoolKey = Animator.StringToHash("IsGrounded");
        private static readonly int IdleSpeedKey = Animator.StringToHash("IdleSpeed");
        private static readonly int MoveSpeedKey = Animator.StringToHash("MoveSpeed");
        private static readonly int VerticalVelocityKey = Animator.StringToHash("VerticalVelocity");
        private static readonly int JumpKey = Animator.StringToHash("Jump");
        private static readonly int DodgingKey = Animator.StringToHash("Dodging");
        private static readonly int CrouchingKey = Animator.StringToHash("Crouching");
        private static readonly int WallSlidingKey = Animator.StringToHash("WallSliding");
        private static readonly int DodgeTriggerKey = Animator.StringToHash("Dodge");
    }
}