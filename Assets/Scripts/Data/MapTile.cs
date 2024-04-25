using UnityEngine;

namespace Data
{
    public class MapTile
    {
        private Vector2Int _coordinates;
        private BiomeType _biomeType;
        private float _height;

        public Vector2Int Coordinates => _coordinates;
        public BiomeType BiomeType => _biomeType;
        public float Height => _height;

        public MapTile(BiomeType biomeType, int x, int y, float height)
        {
            _biomeType = biomeType;
            _coordinates = new Vector2Int(x, y);
            _height = height;
        }
    }
}
