using System.Collections.Generic;
using JetBrains.Annotations;

namespace Environment.Terrain
{
    public class TileList
    {
        private readonly List<List<UnityEngine.Terrain>> _terrainField;
        private readonly List<(int x, int y)> _emptyTiles;

        private int _tileCount;

        public int TileCount
        {
            get => _tileCount;
            private set
            {
                if (value >= 0)
                    _tileCount = value;
            }
        }

        public TileList()
        {
            _terrainField = new List<List<UnityEngine.Terrain>>();
            _terrainField.Add(new List<UnityEngine.Terrain>());
            _emptyTiles = new List<(int x, int y)>();
        }

        public void AddTile(UnityEngine.Terrain tile)
        {
            foreach (var emptyTileLocation in _emptyTiles)
            {
                if (_terrainField[emptyTileLocation.x][emptyTileLocation.y] != null)
                    continue;

                _terrainField[emptyTileLocation.x][emptyTileLocation.y] = tile;
                _emptyTiles.RemoveAt(0);
                return;
            }

            (int x, int prod) firstField = (0, _terrainField[0].Count + 1);
            for (var x = 1; x < _terrainField.Count; x++)
            {
                var newProduct = (x + 1) * (_terrainField[x].Count + 1);
                if (newProduct < firstField.prod)
                    firstField = (x, newProduct);
            }

            if (_terrainField.Count + 1 < firstField.prod)
            {
                _terrainField.Add(new List<UnityEngine.Terrain>());
                firstField = (_terrainField.Count, _terrainField.Count + 1);
            }
            
            _terrainField[firstField.x].Add(tile);
        }

        public bool RemoveTile(UnityEngine.Terrain tile)
        {
            var x = 0;
            var y = -1;
            foreach (var terrainList in _terrainField)
            {
                if (terrainList.Contains(tile))
                {
                    y = terrainList.IndexOf(tile);
                    break;
                }

                x++;
            }

            if (y < 0) return false;

            RemoveTile(x, y);
            return true;
        }

        private bool RemoveTile(int x, int y)
        {
            if (x < 0 || y < 0)
                return false;
            if (x == 0 && y == 0)
                return false;
            if (_terrainField.Count <= x)
                return false;
            if (_terrainField[x].Count <= y)
                return false;

            if (_terrainField[x].Count == y + 1)
            {
                _terrainField[x].RemoveAt(y);
                _emptyTiles.Remove((x, y));
            }
            else if (_terrainField[x][y] == null)
            {
                return false;
            }
            else
            {
                _terrainField[x][y] = null;
                _emptyTiles.Add((x, y));
            }

            if (_terrainField.Count == x - 1 && _terrainField[x].Count == 0)
                _terrainField.RemoveAt(x);
            TileCount--;
            return true;
        }

        [CanBeNull]
        public UnityEngine.Terrain RemoveLastTile()
        {
            (int x, int y, int prod) lastTile = (0, 0, 0);
            for (var x = 0; x < _terrainField.Count; x++)
            {
                for (var y = 0; y < _terrainField[x].Count; y++)
                {
                    var newProduct = (x + 1) * (y + 1);
                    if (lastTile.prod < newProduct)
                        lastTile = (x, y, newProduct);
                }
            }

            var removedTile = _terrainField[lastTile.x][lastTile.y];
            return RemoveTile(lastTile.x, lastTile.y) ? removedTile : null;
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

                    if (_terrainField[emptyTileLocation.x].Count <= emptyTileLocation.y)
                    {
                        _emptyTiles.Remove(emptyTileLocation);
                        change = true;
                        continue;
                    }

                    if (_terrainField[emptyTileLocation.x].Count == emptyTileLocation.y + 1)
                    {
                        _emptyTiles.Remove(emptyTileLocation);
                        _terrainField[emptyTileLocation.x].RemoveAt(emptyTileLocation.y);
                        change = true;
                    }

                    if (_terrainField[emptyTileLocation.x].Count == 0)
                        _terrainField.RemoveAt(emptyTileLocation.x);
                }
            }
        }
    }
}