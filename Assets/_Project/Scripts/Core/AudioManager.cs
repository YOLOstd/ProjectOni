using UnityEngine;

namespace ProjectOni.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlayMusic(string musicName)
        {
            // Implementation for playing music
        }

        public void PlaySFX(string sfxName)
        {
            // Implementation for playing global SFX
        }
    }
}
