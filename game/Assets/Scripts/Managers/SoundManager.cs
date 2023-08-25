using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [DefaultExecutionOrder(-5)]
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        public AudioEffectsSO AudioEffects;

        [SerializeField]
        private AudioSource _effectSource;
        [SerializeField]
        private AudioSource _musicSource;

        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogError("another singleton matey");
                Destroy(gameObject);
                return;
            }
        }


        public void PlayRandomEffect(List<AudioClip> clips, float volume = 1f)
        {
            _effectSource.PlayOneShot(Choose(clips), volume);
        }

        public void PlayEffect(AudioClip clip, float volume = 1f)
        {
            _effectSource.PlayOneShot(clip, volume);
        }

        public void StopEffects()
        {
            _effectSource.Stop();
        }


        private static AudioClip Choose(List<AudioClip> clips)
        {
            int roll = Random.Range(0,clips.Count);
            return clips[roll];
        }
    }
}
