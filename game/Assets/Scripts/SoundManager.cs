using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [DefaultExecutionOrder(-5)]
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        public CombatAudioSO CombatAudio { get; private set; }

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
                Destroy(gameObject);
                return;
            }

            CombatAudio = Resources.Load<CombatAudioSO>("CombatAudio");
        }


        public void PlayEffect(AudioClip clip, Vector3 position)
        {
            AudioSource.PlayClipAtPoint(clip, position, 1f);
        }

        public static AudioClip Choose(List<AudioClip> clips)
        {
            int roll = Random.Range(0,clips.Count);
            return clips[roll];
        }
    }
}
