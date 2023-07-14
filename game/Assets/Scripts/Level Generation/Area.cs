using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Game
{
    public class Area : IMapBlock
    {
        public readonly Vector2Int Coords;

        private GameObject _go;
        private Vector2Int _lowerLeftCorner;
        private Vector2Int _upperRightCorner;
        private List<Vector2Int> _entrances;

        public IMapBlock.BlockType GetBlockType() => IMapBlock.BlockType.Empty;
        public Vector2Int[] GetConnectionDirs() => new Vector2Int[0];


        public Area(Vector2Int entrancePos, Vector2Int entranceDir, GameObject prefab, WorldPrefabsSO prefabs, 
            ref Dictionary<Vector2Int, IMapBlock> map, ref List<Path> paths, Transform parent) 
        {
            Coords = entrancePos;
            _entrances = new List<Vector2Int>();
            _go = GameObject.Instantiate(prefab);

            AreaPrefabInfo areaInfo = prefab.GetComponent<AreaPrefabInfo>();
            int accessPointsCount = areaInfo.AccessPoints.Length;
            int areaSize = areaInfo.Size;
            int entranceIdx = Random.Range(0, accessPointsCount);

            // Initial placement
            Transform transform = _go.transform;
            Vector2Int coords = entrancePos + entranceDir - areaInfo.AccessPoints[entranceIdx].Pos;
            transform.position = Tile.TILE_WIDTH * (new Vector3(coords.x, 0f, coords.y) + new Vector3(0.5f, 0f, 0.5f));
            transform.localScale = Tile.TILE_WIDTH * Vector3.one;

            // Rotate everything untill matching
            Vector2Int[] dirs = new Vector2Int[accessPointsCount];
            Vector2Int[] relPossToEntrance = new Vector2Int[accessPointsCount];
            Vector2Int relUpperRight = areaInfo.AccessPoints[entranceIdx].Pos - Vector2Int.one * (areaSize - 1);
            Vector2Int lowerLeftDir = -Vector2Int.one;
            for (int i = 0; i < accessPointsCount;i++)
            {
                dirs[i] = areaInfo.AccessPoints[i].Dir;
                relPossToEntrance[i] = areaInfo.AccessPoints[i].Pos - areaInfo.AccessPoints[entranceIdx].Pos;
            }
            int rotations = 0;
            while (dirs[entranceIdx] != -entranceDir)
            {
                rotations += 1;
                relUpperRight = Misc.RotateV2Int(relUpperRight, 1);
                lowerLeftDir = Misc.RotateV2Int(lowerLeftDir, 1);
                for (int i = 0; i < accessPointsCount; i++)
                {
                    dirs[i] = Misc.RotateV2Int(dirs[i], 1);
                    relPossToEntrance[i] = Misc.RotateV2Int(relPossToEntrance[i], 1);
                }
            }
            
            // Rotate the GameObject
            Vector2Int axisCoords = entrancePos + entranceDir;
            Vector3 axisPoint = Tile.TILE_WIDTH * (new Vector3(axisCoords.x, 0f, axisCoords.y) + new Vector3(0.5f, 0f, 0.5f));
            transform.RotateAround(axisPoint, Vector3.up, 90f * rotations);

            // Create outgoing paths
            for (int i = 0; i < accessPointsCount; i++)
            {
                Vector2Int coord = axisCoords + relPossToEntrance[i] + dirs[i];
                _entrances.Add(coord);

                if (i == entranceIdx)
                {
                    // Place entrance indicator
                    // This may fuck up one day and you will need a different way to place indicators
                    if(paths[0].EventIndicator != null)
                    {
                        Vector2Int offset = Misc.RotateV2Int(paths[0].LastDir, 1);
                        ((Tile)map[coord]).PlaceIndicator(paths[0].EventIndicator, new Vector3(offset.x, 0f, offset.y));
                    }
                    continue;
                }

                float angle = -Vector2.SignedAngle(Vector2.up, dirs[i]);
                Path path = new Path(coord, angle, dirs[i], prefabs, !_go.name.Contains("Shop"));
                paths.Add(path);
                Tile tile = new Tile(coord, IMapBlock.BlockType.Path, new Vector2Int[] { dirs[i], -dirs[i] }, prefabs, parent);
                map.Add(coord, tile);

                // Add indicator
                if(path.EventIndicator != null)
                {
                    Vector2Int offset = Misc.RotateV2Int(path.LastDir, 1);
                    tile.PlaceIndicator(path.EventIndicator, new Vector3(offset.x, 0f, offset.y));
                }
            }

            // Calculate the area, which when left will delete this Area
            Vector2Int corner1 = axisCoords - relUpperRight;
            Vector2Int corner2 = corner1 + lowerLeftDir * (areaSize - 1);
            int minX = Mathf.Min(corner1.x, corner2.x);
            int minY = Mathf.Min(corner1.y, corner2.y);
            _lowerLeftCorner = new Vector2Int(minX, minY);
            _upperRightCorner = _lowerLeftCorner + Vector2Int.one*(areaSize - 1);
            for(int xOffset = 0; xOffset < areaSize; xOffset++)
            {
                for (int yOffset = 0; yOffset < areaSize; yOffset++)
                {
                    map.Add(new Vector2Int(minX + xOffset, minY + yOffset), this);
                }
            }
        }

        public bool ConciderDeletion(Vector2Int playerPos, Vector2Int goingDir, int generationDist, 
            ref List<Path> pathsReference, ref Dictionary<Vector2Int, IMapBlock> map, WorldPrefabsSO worldPrefabs)
        {
            // If all but one entrances are fully retracted and the player is far enough, initialize deletion
            List<Vector2Int> nonRetractedEntrances = new List<Vector2Int>(_entrances);
            foreach(Path path in pathsReference)
            {
                if (nonRetractedEntrances.Contains(path.LastTile)) nonRetractedEntrances.Remove(path.LastTile);
            }
            if (nonRetractedEntrances.Count != 1) return false;

            Vector2Int southWestLimit = _lowerLeftCorner - Vector2Int.one * generationDist;
            Vector2Int northEastLimit = _upperRightCorner + Vector2Int.one * generationDist;
            if (!(playerPos.x <= southWestLimit.x || playerPos.x >= northEastLimit.x
                || playerPos.y <= southWestLimit.y || playerPos.y >= northEastLimit.y))
                return false;

            // Deleting the GameObject
            Object.Destroy(_go);

            // Clearing the map
            for (int x = _lowerLeftCorner.x; x <= _upperRightCorner.x; x++)
            {
                for (int y = _lowerLeftCorner.y; y <= _upperRightCorner.y; y++)
                {
                    map.Remove(new Vector2Int(x, y));
                }
            }

            // Removing paths
            for (int i = 0; i < pathsReference.Count; i++)
            {
                if (_entrances.Contains(pathsReference[i].LastTile))
                {
                    map[pathsReference[i].LastTile].Delete(ref map);
                    pathsReference.Remove(pathsReference[i]);
                    i--;
                }
            }

            // Creating a path instead
            float angle = -Vector2.SignedAngle(Vector2.up, -goingDir);
            bool shopIsBeingCreated = false;
            foreach (var path in pathsReference)
            {
                if(path.EventIndicator != null && path.PathEventPrefab.name.Contains("Shop"))
                {
                    shopIsBeingCreated = true;
                }
            }
            pathsReference.Add(new Path(nonRetractedEntrances[0], angle, -goingDir, worldPrefabs, !shopIsBeingCreated));

            return true;
        }

        public void Delete(ref Dictionary<Vector2Int, IMapBlock> map) { }
        public void PlaceTree(WorldPrefabsSO worldPrefabs, int treeRange) { }
        public void RemovePlaceable() { }
    }
}
