using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Audio/AudioMusic")]
    public class AudioMusicSO : ScriptableObject
    {
        public List<AudioClip> Tracks = new();

    }
}
