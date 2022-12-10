using UnityEngine;
using UnityEditor;

namespace Game
{
    [CustomEditor(typeof(TileSO))]
    public class TileSOEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            TileSO script = (TileSO)target;
    
            if(GUILayout.Button("UPDATE VARIABLES", GUILayout.Height(40)))
            {
                script.UpdateAllTiles();
            }
            
        }
    }
}
