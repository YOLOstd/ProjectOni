using System.Collections.Generic;
using PurrNet;
using UnityEngine;

namespace ProjectOni.Managers
{
    public class NetworkAudioManager : NetworkIdentity
    {
        [SerializeField] private AudioSource _sourcePrefab;
        [SerializeField] private int _initialPoolSize = 10;

        private readonly List<AudioSource> _pool = new();

        private void Awake()
        {
            InstanceHandler.RegisterInstance(this);
            for (int i = 0; i < _initialPoolSize; i++)
                CreateSource();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (InstanceHandler.TryGetInstance(out NetworkAudioManager manager) && manager == this)
                InstanceHandler.UnregisterInstance<NetworkAudioManager>();
        }

        private AudioSource CreateSource()
        {
            var source = Instantiate(_sourcePrefab, transform);
            source.gameObject.SetActive(false);
            _pool.Add(source);
            return source;
        }

        private AudioSource GetAvailable()
        {
            for (var i = 0; i < _pool.Count; i++)
            {
                var source = _pool[i];
                if (!source.isPlaying)
                    return source;
            }

            return CreateSource();
        }

        [ObserversRpc(runLocally: true)]
        public void PlayClip(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
        {
            if (!clip) return;
            var source = GetAvailable();
            source.transform.position = position;
            source.gameObject.SetActive(true);
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.Play();
        }

        public void PlayRandom(List<AudioClip> clips, Vector3 position, float volume = 1f, float minPitch = 0.9f, float maxPitch = 1.1f)
        {
            if (clips == null || clips.Count == 0) return;
            float pitch = Random.Range(minPitch, maxPitch);
            PlayClip(clips[Random.Range(0, clips.Count)], position, volume, pitch);
        }
    }
}
