using Environment.Terrain;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

public class TerrainController : MonoBehaviour
{
    private TileList[] _terrainTiles;
    private const int TileSize = 1000;

    void Start()
    {
        _terrainTiles = new TileList[4];
        for (var idx = 0; idx < 4; idx++)
        {
            var mapCenter = new Vector3(0, 0, 0);
            var xQuart = idx < 2;
            var zQuart = idx % 2 == 0;
            _terrainTiles[idx] = new TileList(gameObject, mapCenter, xQuart, zQuart, TileSize);

            var terrainNumber = new Random().Next(1, 10);
            var data = AssetDatabase.LoadAssetAtPath<TerrainData>($"Assets/Scenes/Terrains/DefaultSurfaces/0{terrainNumber}.asset");
            var terrainObject = Terrain.CreateTerrainGameObject(data);
            _terrainTiles[idx].AddTile(terrainObject);
        }
    }

    public void AddTile(GameObject newTile)
    {
        (int idx, int count) smallestTile = (0, _terrainTiles[0].TileCount);
        for (var i = 1; i < 4; i++)
        {
            var count = _terrainTiles[i].TileCount;
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

        for (var idx = 1; idx < 4; idx++)
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