using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Environment.Terrain
{
    public class TileList
    {
        private GameObject _terrainParentController;

        private readonly List<List<GameObject>> _terrainField;
        private readonly List<(int x, int z)> _emptyTiles;

        private int _tileCount;
        private Vector3 _origin;
        private int _xMultiplier, _zMultiplier;
        private int _tileSize;

        public int TileCount
        {
            get => _tileCount;
            private set
            {
                if (value >= 0)
                    _tileCount = value;
            }
        }

        public TileList(GameObject go, Vector3 origin, bool xIsPositive, bool zIsPositive, int tileSize)
        {
            _terrainField = new List<List<GameObject>>();
            _terrainField.Add(new List<GameObject>());
            _emptyTiles = new List<(int x, int z)>();

            _terrainParentController = go;

            _origin = origin;
            _xMultiplier = xIsPositive ? 1 : -1;
            _zMultiplier = zIsPositive ? 1 : -1;
            _tileSize = tileSize;
        }

        public void AddTile(GameObject tile)
        {
            tile.transform.parent = _terrainParentController.transform;
            var placedX = 0;
            var placedZ = 0;

            var foundEmpty = false;
            foreach (var emptyTileLocation in _emptyTiles)
            {
                if (_terrainField[emptyTileLocation.x][emptyTileLocation.z] != null)
                    continue;

                _terrainField[emptyTileLocation.x][emptyTileLocation.z] = tile;
                placedX = emptyTileLocation.x;
                placedZ = emptyTileLocation.z;
                
                _emptyTiles.Remove(emptyTileLocation);
                foundEmpty = true;
            }

            if (!foundEmpty)
            {
                (int x, int prod) firstField = (0, _terrainField[0].Count + 1);
                for (var x = 1; x < _terrainField.Count; x++)
                {
                    var newProduct = (x + 1) * (_terrainField[x].Count + 1);
                    if (newProduct < firstField.prod)
                        firstField = (x, newProduct);
                }

                if (_terrainField.Count + 1 < firstField.prod)
                {
                    _terrainField.Add(new List<GameObject>());
                    firstField = (_terrainField.Count, _terrainField.Count + 1);
                }

                _terrainField[firstField.x].Add(tile);
                placedX = firstField.x;
                placedZ = _terrainField[placedX].Count - 1;
            }

            var xOffset = (_tileSize / 2 + placedX * _tileSize) * _xMultiplier;
            var zOffset = (_tileSize / 2 + placedZ * _tileSize) * _zMultiplier;
            Vector3 tileOffset = new Vector3(xOffset, 0, zOffset);
            tile.transform.position = _origin + tileOffset;
        }

        public bool RemoveTile(GameObject tile)
        {
            var x = 0;
            var z = -1;
            foreach (var terrainList in _terrainField)
            {
                if (terrainList.Contains(tile))
                {
                    z = terrainList.IndexOf(tile);
                    break;
                }

                x++;
            }

            if (z < 0) return false;

            RemoveTile(x, z);
            return true;
        }

        private bool RemoveTile(int x, int z)
        {
            if (x < 0 || z < 0)
                return false;
            if (x == 0 && z == 0)
                return false;
            if (_terrainField.Count <= x)
                return false;
            if (_terrainField[x].Count <= z)
                return false;

            if (_terrainField[x].Count == z + 1)
            {
                _terrainField[x].RemoveAt(z);
                _emptyTiles.Remove((x, z));
            }
            else if (_terrainField[x][z] == null)
            {
                return false;
            }
            else
            {
                _terrainField[x][z] = null;
                _emptyTiles.Add((x, z));
            }

            if (_terrainField.Count == x - 1 && _terrainField[x].Count == 0)
                _terrainField.RemoveAt(x);
            TileCount--;
            //TODO: Remove tile from world if it doesn't happen automatically
            return true;
        }

        [CanBeNull]
        public GameObject RemoveLastTile()
        {
            (int x, int z, int prod) lastTile = (0, 0, 0);
            for (var x = 0; x < _terrainField.Count; x++)
            {
                for (var z = 0; z < _terrainField[x].Count; z++)
                {
                    var newProduct = (x + 1) * (z + 1);
                    if (lastTile.prod < newProduct)
                        lastTile = (x, z, newProduct);
                }
            }

            var removedTile = _terrainField[lastTile.x][lastTile.z];
            return RemoveTile(lastTile.x, lastTile.z) ? removedTile : null;
        }

        public void ReShuffleTiles(int rounds)
        {
            CleanEmptyTiles();
            if (rounds < 0)
            {
                while (_emptyTiles.Count > 0)
                {
                    for (var i = 0; i < _emptyTiles.Count; i++)
                    {
                        var moveTile = RemoveLastTile();
                        if (moveTile != null) AddTile(moveTile);
                    }

                    CleanEmptyTiles();
                }

                return;
            }

            for (var counter = 0; counter < rounds; counter++)
            {
                for (var i = 0; i < _emptyTiles.Count; i++)
                {
                    var moveTile = RemoveLastTile();
                    if (moveTile != null) AddTile(moveTile);
                }

                CleanEmptyTiles();
            }
        }

        private void CleanEmptyTiles()
        {
            var change = true;
            while (change)
            {
                change = false;
                foreach (var emptyTileLocation in _emptyTiles)
                {
                    if (_terrainField.Count <= emptyTileLocation.x)
                    {
                        _emptyTiles.Remove(emptyTileLocation);
                        change = true;
                        continue;
                    }

                    if (_terrainField[emptyTileLocation.x].Count <= emptyTileLocation.z)
                    {
                        _emptyTiles.Remove(emptyTileLocation);
                        change = true;
                        continue;
                    }

                    if (_terrainField[emptyTileLocation.x].Count == emptyTileLocation.z + 1)
                    {
                        _emptyTiles.Remove(emptyTileLocation);
                        _terrainField[emptyTileLocation.x].RemoveAt(emptyTileLocation.z);
                        change = true;
                    }

                    if (_terrainField[emptyTileLocation.x].Count == 0)
                        _terrainField.RemoveAt(emptyTileLocation.x);
                }
            }
        }
    }
}