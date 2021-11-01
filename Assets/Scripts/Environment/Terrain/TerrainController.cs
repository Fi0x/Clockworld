using Environment.Terrain;
using UnityEditor;
using UnityEngine;

public class TerrainController : MonoBehaviour
{
    private TileList[] _terrainTiles;
    private const int TileSize = 1000;

    void Start()
    {
        _terrainTiles = new TileList[4];
        for (int idx = 0; idx < 4; idx++)
        {
            var mapCenter = new Vector3(0, 0, 0);
            var xQuart = idx < 2;
            var zQuart = idx % 2 == 0;
            _terrainTiles[idx] = new TileList(gameObject, mapCenter, xQuart, zQuart, TileSize);

            TerrainData data = AssetDatabase.LoadAssetAtPath<TerrainData>("Assets/Scenes/Terrains/DefaultTerrain1.asset");
            Debug.Log(data);
            GameObject terrainObject = Terrain.CreateTerrainGameObject(data); //TODO: Get correct default terrain
            _terrainTiles[idx].AddTile(terrainObject);
        }
    }

    public void AddTile(GameObject newTile)
    {
        (int idx, int count) smallestTile = (0, _terrainTiles[0].TileCount);
        for (int i = 1; i < 4; i++)
        {
            int count = _terrainTiles[i].TileCount;
            if (smallestTile.count > count)
                smallestTile = (i, count);
        }

        _terrainTiles[smallestTile.idx].AddTile(newTile);
    }

    public void RemoveTile(GameObject oldTile)
    {
        foreach (var mapQuarter in _terrainTiles)
        {
            if (mapQuarter.RemoveTile(oldTile))
                return;
        }
    }

    public void ReShuffleTiles()
    {
        (int idx, int size) smallestQuarter = (0, _terrainTiles[0].TileCount);
        (int idx, int size) largestQuarter = (0, _terrainTiles[0].TileCount);

        for (int idx = 1; idx < 4; idx++)
        {
            var count = _terrainTiles[idx].TileCount;
            if (count < smallestQuarter.size) smallestQuarter = (idx, count);
            else if (count > largestQuarter.size) largestQuarter = (idx, count);
        }

        var difference = largestQuarter.size - smallestQuarter.size;
        if (difference < 10) return;

        for (var i = 0; i < difference / 2; i++)
        {
            var moveTile = _terrainTiles[largestQuarter.idx].RemoveLastTile();
            if (moveTile != null)
            {
                _terrainTiles[smallestQuarter.idx].AddTile(moveTile);
            }
        }
    }
}