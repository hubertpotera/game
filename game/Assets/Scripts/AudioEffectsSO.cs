using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Audio/AudioEffects")]
    public class AudioEffectsSO : ScriptableObject
    {
        [Header("Combat")]
        public List<AudioClip> Windup = new();
        public List<AudioClip> Swing = new();
        public List<AudioClip> Hit = new();
        public List<AudioClip> Parry = new();
        
        public List<AudioClip> BowDraw = new();
        public List<AudioClip> BowRelease = new();
        public List<AudioClip> BowHit = new();

        [Header("General")]
        public List<AudioClip> Step = new();
        public AudioClip OpenInventory;
        public AudioClip Coins;
        public AudioClip GateRumble;

    }
}
