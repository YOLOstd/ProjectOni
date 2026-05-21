using UnityEngine;

namespace ProjectOni.Core
{
    public class LevelComponent : MonoBehaviour
    {
        private EntityState _entityState;

        public int Level => _entityState != null ? _entityState.Level.value : 1;

        private void Awake()
        {
            _entityState = GetComponent<EntityState>();
        }

        public void SetLevel(int newLevel)
        {
            if (_entityState != null && _entityState.isServer)
                _entityState.Level.value = newLevel;
        }

        public void AddLevel(int amount)
        {
            if (_entityState != null && _entityState.isServer)
                _entityState.Level.value += amount;
        }
    }
}
