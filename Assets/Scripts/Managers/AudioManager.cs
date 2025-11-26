using UnityEngine;
using Zenject;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;
using System;

namespace MiniIT.AUDIO
{
    public interface IAudioManager
    {
        /// <summary>
        /// Plays a sound by its ID.
        /// </summary>
        /// <param name="soundId">The ID of the sound to play.</param>
        /// <param name="position">Optional world position for 3D sound.</param>
        void PlaySound(string soundId, Vector3? position = null);

        /// <summary>
        /// Sets master volume for the whole game.
        /// </summary>
        /// <param name="volume">Volume level from 0 to 1.</param>
        void SetMasterVolume(float volume);
    }

    public class AudioManager : MonoBehaviour, IAudioManager
    {
        [System.Serializable]
        public class SoundConfig
        {
            public string id = null;
            public AudioClip clip = null;
            [Range(0f, 1f)] public float volume = 1f;
        }

        [SerializeField] private SoundConfig[] sounds = null;

        private Dictionary<string, SoundConfig> soundConfigDict = null;
        private ObjectPool<AudioSource> sourcePool = null;
        private Transform poolParent = null;

        private const int MaxPoolSize = 50;
        private const int PreloadCount = 10;

        private void Awake()
        {
            soundConfigDict = new Dictionary<string, SoundConfig>();

            foreach (SoundConfig config in sounds)
            {
                if (config == null || string.IsNullOrEmpty(config.id))
                {
                    Debug.LogWarning("Invalid sound configuration (null or empty ID). Skipping.");
                    continue;
                }

                if (soundConfigDict.ContainsKey(config.id))
                {
                    Debug.LogWarning($"Duplicate sound ID '{config.id}'");
                    continue;
                }

                soundConfigDict.Add(config.id, config);
            }

            GameObject poolObject = new GameObject("AudioPool");
            poolParent = poolObject.transform;
            poolParent.SetParent(transform);

            sourcePool = new ObjectPool<AudioSource>(
                createFunc: CreateSource,
                actionOnGet: source => source.gameObject.SetActive(true),
                actionOnRelease: source => source.gameObject.SetActive(false),
                actionOnDestroy: source => Destroy(source.gameObject),
                maxSize: MaxPoolSize
            );

            List<AudioSource> preloaded = new List<AudioSource>();

            for (int i = 0; i < PreloadCount; i++)
            {
                preloaded.Add(sourcePool.Get());
            }

            foreach (AudioSource source in preloaded)
            {
                sourcePool.Release(source);
            }
        }

        private AudioSource CreateSource()
        {
            GameObject go = new GameObject("PooledAudioSource");
            go.transform.SetParent(poolParent);
            AudioSource source = go.AddComponent<AudioSource>();
            return source;
        }

        #region IAudioManager

        public void PlaySound(string soundId, Vector3? position = null)
        {
            if (string.IsNullOrEmpty(soundId))
            {
                Debug.LogWarning("Invalid sound ID (null or empty).");
                return;
            }

            if (!soundConfigDict.TryGetValue(soundId, out SoundConfig config))
            {
                Debug.LogWarning($"Sound ID '{soundId}' not found");
                return;
            }

            if (config.clip == null)
            {
                Debug.LogWarning($"AudioClip for '{soundId}' is null.");
                return;
            }

            AudioSource source = sourcePool.Get();
            ConfigureSource(source, config, position);
            source.Play();

            ReturnToPoolAfterPlay(source, config.clip.length).Forget();
        }

        public void SetMasterVolume(float volume)
        {
            volume = Mathf.Clamp(volume, 0f, 1f);
            AudioListener.volume = volume;
        }

        #endregion

        private void ConfigureSource(AudioSource source, SoundConfig config, Vector3? position)
        {
            source.clip = config.clip;
            source.volume = config.volume;
            source.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            source.loop = false;
            source.priority = 128;
            source.spatialBlend = 0f;

            if (position.HasValue)
            {
                source.transform.position = position.Value;
                source.spatialBlend = 1f;
            }
        }

        private async UniTaskVoid ReturnToPoolAfterPlay(AudioSource source, float delay)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            sourcePool.Release(source);
        }
    }
}