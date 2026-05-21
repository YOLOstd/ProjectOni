using UnityEngine;

namespace ProjectOni.Combat
{
    [CreateAssetMenu(menuName = "Combat/Attack Node")]
    public class AttackNode : ScriptableObject, IAttackBehavior
    {
        [Header("UI & Identity")]
        public string attackName;
        public Sprite skillIcon;

        [Header("Base Timing")]
        [Tooltip("The natural duration of this attack state before resetting or automatically progressing if held/buffered.")]
        public float totalDuration = 0.5f; 

        [Header("Combo Paths")]
        public AttackNode normalNextNode;  
        
        [Header("Perfect Rhythm Timing")]
        public AttackNode perfectNextNode; 
        [Tooltip("Start window for a perfect rhythm action (seconds into the attack).")]
        public float perfectWindowStart = 0.40f;   
        [Tooltip("End window for a perfect rhythm action (seconds into the attack).")]
        public float perfectWindowEnd = 0.55f;     

        [Header("Execution Data")]
        public float baseGlobalLockTime;
        public float baseLungeForce;
        public float baseAntiGravityTime = 0.2f;
        public VisualRequest visualData;

        public AttackResult Execute(AttackContext ctx)
        {
            return new AttackResult
            {
                Success = true,
                GlobalLockTime = baseGlobalLockTime / ctx.AttackSpeedMultiplier,
                LungeForce = baseLungeForce,
                AntiGravityTime = baseAntiGravityTime,
                Visuals = visualData
            };
        }
    }
}
