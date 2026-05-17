using UnityEngine;
using ProjectOni.Combat.Data;

namespace ProjectOni.Combat
{
    public enum ActionSlot
    {
        Primary,
        Secondary,
        Spell1,
        Spell2
    }

    [System.Serializable]
    public struct VisualRequest
    {
        public string animationTrigger;
        public AudioClip sfx;
        public GameObject projectilePrefab;
        public float projectileSpeed;
        public float damage;
        public Vector2 spawnOffset;
        public GameObject hitVFXPrefab;
        public float lifetime;
        
        public float hitboxStartTime;
        public float hitboxDuration;

        public static VisualRequest Default => new VisualRequest();
    }

    public class ComboState
    {
        public int Index { get; private set; }
        private float _lastAdvanceTime;
        private int _maxSteps;

        public ComboState(int maxSteps)
        {
            _maxSteps = maxSteps;
        }

        public void Advance(float window)
        {
            if (Time.time < _lastAdvanceTime + window)
            {
                Index = (Index + 1) % _maxSteps;
            }
            else
            {
                Index = 0;
            }
            _lastAdvanceTime = Time.time;
        }

        public void Reset()
        {
            Index = 0;
            _lastAdvanceTime = 0;
        }
    }

    public struct AttackContext
    {
        public GameObject Caster;
        public LayerMask TargetLayer;
        public Vector2 Direction;
        public Vector3 Position;
        public int SkillLevel;
    }

    public interface IAttackBehavior
    {
        VisualRequest Execute(AttackContext ctx);
    }
}
