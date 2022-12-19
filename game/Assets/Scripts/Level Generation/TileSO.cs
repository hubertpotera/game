using UnityEngine;
using System.Collections.Generic;

namespace Game
{
    [CreateAssetMenu(fileName = "Tile", menuName = "ScriptableObjects/Tile", order = 0)]
    public class TileSO : ScriptableObject
    {
        // ---------- VARIABLES ----------
        #region variables

        public Transform prefab;

        public Connection North;
        public Connection East;
        public Connection South;
        public Connection West;

        public HashSet<Connection> Connections { get; private set; } 
        public Connection[] ConnectionsOrdered { get; private set; } 

        public enum Connection
        {
            Empty,
            Path
        }

        #endregion





        // ----------- METHODS -----------
        #region methods

        public void UpdateVariabes()
        {
            Connections = new HashSet<Connection>();
            Connections.Add(North);
            Connections.Add(East);
            Connections.Add(South);
            Connections.Add(West);

            ConnectionsOrdered = new Connection[4];
            ConnectionsOrdered[0] = North;
            ConnectionsOrdered[1] = East;
            ConnectionsOrdered[2] = South;
            ConnectionsOrdered[3] = West;
        }

        public void UpdateAllTiles()
        {
            AllTiles.UpdateVariables();
        }

        #endregion
    }
}
