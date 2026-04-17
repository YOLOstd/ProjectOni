using UnityEngine;

namespace TarodevController
{
    [CreateAssetMenu(fileName = "PlayerMovementData", menuName = "Player/Movement Data")]
    public class PlayerMovementData : ScriptableObject
    {
        [Header("LAYERS")] [Tooltip("Set this to the layer your player is on")]
        public LayerMask PlayerLayer;

        [Tooltip("Set this to the layer your walls are on")]
        public LayerMask WallLayer;

        [Header("INPUT")] [Tooltip("Makes all Input snap to an integer. Prevents gamepads from walking slowly. Recommended value is true to ensure gamepad/keybaord parity.")]
        public bool SnapInput = true;

        [Tooltip("Minimum input required before you mount a ladder or climb a ledge. Avoids unwanted climbing using controllers"), Range(0.01f, 0.99f)]
        public float VerticalDeadZoneThreshold = 0.3f;

        [Tooltip("Minimum input required before a left or right is recognized. Avoids drifting with sticky controllers"), Range(0.01f, 0.99f)]
        public float HorizontalDeadZoneThreshold = 0.1f;

        [Header("MOVEMENT")] [Tooltip("The top horizontal movement speed")]
        public float MaxSpeed = 14;

        [Tooltip("Speed modifier when crouching"), Range(0.1f, 1f)]
        public float CrouchSpeedModifier = 0.5f;

        [Tooltip("The player's capacity to gain horizontal speed")]
        public float Acceleration = 120;

        [Tooltip("The pace at which the player comes to a stop")]
        public float GroundDeceleration = 60;

        [Tooltip("Deceleration in air only after stopping input mid-air")]
        public float AirDeceleration = 30;

        [Tooltip("A constant downward force applied while grounded. Helps on slopes"), Range(0f, -10f)]
        public float GroundingForce = -1.5f;

        [Tooltip("The detection distance for grounding and roof detection"), Range(0f, 0.5f)]
        public float GrounderDistance = 0.05f;

        [Header("JUMP")] [Tooltip("The immediate velocity applied when jumping")]
        public float JumpPower = 36;

        [Tooltip("The maximum vertical movement speed")]
        public float MaxFallSpeed = 40;

        [Tooltip("The player's capacity to gain fall speed. a.k.a. In Air Gravity")]
        public float FallAcceleration = 110;

        [Tooltip("The percentage of vertical velocity retained when the jump button is released early. 0.5 means 50% speed reduction."), Range(0f, 1f)]
        public float JumpCutMultiplier = 0.5f;

        [Tooltip("The time before coyote jump becomes unusable. Coyote jump allows jump to execute even after leaving a ledge")]
        public float CoyoteTime = .15f;

        [Tooltip("The amount of time we buffer a jump. This allows jump input before actually hitting the ground")]
        public float JumpBuffer = .2f;

        [Tooltip("Number of jumps allowed in the air")]
        public int MaxAirJumps = 1;

        [Header("WALLS")] [Tooltip("The immediate velocity applied when wall jumping")]
        public float WallJumpPower = 30;

        [Tooltip("The speed at which the player slides down walls"), Range(0.1f, 20f)]
        public float WallSlideSpeed = 5;

        [Header("DODGE")] [Tooltip("The velocity applied when dodging on the ground")]
        public float DodgePower = 30;

        [Tooltip("The velocity applied when dodging in the air")]
        public float AirDodgePower = 20;

        [Tooltip("How long the dodge lasts in seconds")]
        public float DodgeDuration = 0.2f;

        [Tooltip("The time before dodge becomes unusable again")]
        public float DodgeCooldown = 1f;

        [Tooltip("The maximum speed preserved after an air dodge ends")]
        public float DodgeEndSpeed = 15f;
    }
}