using UnityEngine;

namespace Data
{
    public class MapTile
    {
        private Vector2Int _coordinates;
        private BiomeType _biomeType;

        public Vector2Int Coordinates => _coordinates;
        public BiomeType BiomeType => _biomeType;

        public MapTile(BiomeType biomeType, int x, int y)
        {
            _biomeType = biomeType;
            _coordinates = new Vector2Int(x, y);
        }
    }
}
