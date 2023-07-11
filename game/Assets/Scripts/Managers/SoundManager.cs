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

        // [SerializeField]
        // private AudioSource _effectSource;
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


        public void PlayRandomEffect(List<AudioClip> clips, Vector3 position, float volume = 1f)
        {
            AudioSource.PlayClipAtPoint(Choose(clips), position, volume);
        }

        public void PlayEffect(AudioClip clip, Vector3 position, float volume = 1f)
        {
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }

        private static AudioClip Choose(List<AudioClip> clips)
        {
            int roll = Random.Range(0,clips.Count);
            return clips[roll];
        }
    }
}
