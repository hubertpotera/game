using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class AreaPrefabInfo : MonoBehaviour
    {
        [SerializeField]
        private AccessPoint[] _accessPoints;
        public AccessPoint[] AccessPoints { get { return _accessPoints; } }

        [SerializeField]
        private int _size;
        public int Size { get { return _size; } }

        [System.Serializable]
        public struct AccessPoint
        {
            public Vector2Int Pos;
            public Vector2Int Dir;
        }
    }
}
