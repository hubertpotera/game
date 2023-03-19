using UnityEngine;

namespace Game
{
    public static class Misc
    {
        /// <summary>1 for right, -1 for left</summary>
        public static Vector2Int RotateV2Int(Vector2Int v, int dir)
        {
            if (dir == 1)       return new Vector2Int(v.y, -v.x);
            else if (dir == -1) return new Vector2Int(-v.y, v.x);
            else
            {
                Debug.LogError("wrong dir value mate");
                return v;
            }
        }

        public static int Sign(float x)
        {
            return (x < 0f) ? -1 : 1;
        }
    }
}
