using UnityEngine;

namespace Game
{
    public static class Misc
    {
        /// <summary>1 for right, -1 for left</summary>
        public static Vector2Int RotateV2Int(Vector2Int v, int dir)
        {
            dir = Mathf.Clamp(dir, -1, 1);
            if(v.x == 0)    return new Vector2Int(dir*v.y, 0);
            else            return new Vector2Int(0, dir*-v.x);
        }
    }
}
