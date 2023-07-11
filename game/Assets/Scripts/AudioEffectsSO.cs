using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Audio/AudioEffects")]
    public class AudioEffectsSO : ScriptableObject
    {
        [Header("Combat")]
        public List<AudioClip> Windup = new List<AudioClip>();
        public List<AudioClip> Swing = new List<AudioClip>();
        public List<AudioClip> Hit = new List<AudioClip>();
        public List<AudioClip> Parry = new List<AudioClip>();
        
        public List<AudioClip> BowDraw = new List<AudioClip>();
        public List<AudioClip> BowRelease = new List<AudioClip>();
        public List<AudioClip> BowHit = new List<AudioClip>();

        [Header("General")]
        public List<AudioClip> Step = new List<AudioClip>();
        public AudioClip OpenInventory;
        public AudioClip Coins;

    }
}
