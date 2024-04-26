using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Logic
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] private Grid grid;
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private MapGenerationSettings settings;
        
        private MapTile[,] _generatedTiles;
        private readonly List<MapTile> _riverTiles = new ();

        private void Start()
        {
            GenerateMap();
        }

        public void GenerateMap()
        {
            GenerateTerrain();
            GenerateRivers();
        }

        #region Terrain Generation
        
        public void GenerateTerrain()
        {
            _riverTiles.Clear();
            var heightMap = GenerateNoise(settings.Size.x, settings.Size.y, settings.Scale, settings.HeightWaves);
            var moistureMap = GenerateNoise(settings.Size.x, settings.Size.y, settings.Scale, settings.MoistureWaves);
            var temperatureMap = GenerateNoise(settings.Size.x, settings.Size.y, settings.Scale, settings.TemperatureWaves);

            _generatedTiles = new MapTile[settings.Size.x, settings.Size.y];
            
            for (var x = 0; x < settings.Size.x; x++)
            for (var y = 0; y < settings.Size.y; y++)
            {
                var biome = GetBiome(heightMap[x, y], moistureMap[x, y], temperatureMap[x, y]);
                var tile = biome.GetTile();
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                _generatedTiles[x, y] = new MapTile(biome.Type, x, y, biome.GetHeightForCell());
            }

            OffsetMapForCamera();
        }
        
        private void OffsetMapForCamera()
        {
            if (Camera.main != null) Camera.main.orthographicSize = settings.Size.x;
            var cellSize = grid.cellSize;
            tilemap.transform.position = new Vector3(-cellSize.x * settings.Size.x / 2, -cellSize.y * settings.Size.y / 3, 0);
        }

        private float[,] GenerateNoise(int width, int height, float scale, List<NoiseWave> waves)
        {
            var seed = DateTime.Now.Millisecond;
            var noiseMap = new float[width, height];
            for(var x = 0; x < width; ++x)
            {
                for(var y = 0; y < height; ++y)
                {
                    var samplePosX = x * scale;
                    var samplePosY = y * scale;
                    
                    var normalization = 0.0f;
                    foreach(var wave in waves)
                    {
                        noiseMap[x, y] += wave.amplitude * Mathf.PerlinNoise(samplePosX * wave.frequency + seed, samplePosY * wave.frequency + seed);
                        normalization += wave.amplitude;
                    }
                    noiseMap[x, y] /= normalization;
                }
            }
        
            return noiseMap;
        }
        
        private BiomeSettings GetBiome(float height, float moisture, float heat)
        {
            var biomeTempList = new List<BiomeSettings>();
            foreach(var biome in settings.Biomes)
            {
                if(biome.MatchCondition(height, moisture, heat))
                {
                    biomeTempList.Add(biome);                
                }
            }

            var curVal = 0.0f;
            BiomeSettings biomeToReturn = null;
            foreach(var biome in biomeTempList)
            {
                if (biomeToReturn == null)
                {
                    biomeToReturn = biome;
                    curVal = biome.GetDifference(height, moisture, heat);
                }
                else
                {
                    if (biome.GetDifference(height, moisture, heat) < curVal)
                    {
                        biomeToReturn = biome;
                        curVal = biome.GetDifference(height, moisture, heat);
                    }
                }
            }
            
            if(biomeToReturn == null)
                biomeToReturn = settings.Biomes[0];
            
            return biomeToReturn;
        }
        
        #endregion
 
        #region River Generation
        
        public void GenerateRivers()
        {
            Random.InitState(DateTime.Now.Millisecond);
            for (var i = 0; i < settings.MaxRiversAmount; i++)
            {
                var borderWaterTileList = GetBorderWaterTiles();
                if (borderWaterTileList.Count == 0)
                {
                    Debug.Log("Can't generate rivers, because no water bodies on map.");
                    break;
                }

                
                GenerateRiver(borderWaterTileList[Random.Range(0, borderWaterTileList.Count)]);
            }
        }

        private List<MapTile> GetBorderWaterTiles()
        {
            var borderWaterTileList = new List<MapTile>();
            for (int x = 0; x < settings.Size.x; x++)
            {
                for (int y = 0; y < settings.Size.y; y++)
                {
                    if (_generatedTiles[x, y].BiomeType == BiomeType.Water)
                    {
                        if (GetAdjacentTiles(_generatedTiles[x, y]).Count(tile => tile.BiomeType == BiomeType.Water) > 3) continue;
                        borderWaterTileList.Add(_generatedTiles[x, y]);
                    }
                }
            }

            return borderWaterTileList;
        }

        private void GenerateRiver(MapTile startTile)
        {
            MapTile currentTile = startTile;
            var tilesToFillWithWater = new List<MapTile>();

            while (true)
            {
                var adjacentTiles = GetAdjacentTiles(currentTile);
                adjacentTiles.RemoveAll(tile => tilesToFillWithWater.Contains(tile) || tile.BiomeType == BiomeType.Water || _riverTiles.Contains(tile));
                
                if (adjacentTiles.Count == 0) break;
                
                var chosenNeighbour = adjacentTiles.OrderByDescending(tile => tile.Height).ToList()[0];
                var tile = _generatedTiles[chosenNeighbour.Coordinates.x, chosenNeighbour.Coordinates.y];
                if (tile.BiomeType == BiomeType.Mountain) break;

                tilesToFillWithWater.Add(chosenNeighbour);
                tilesToFillWithWater.AddRange(GetRandomTiles(adjacentTiles, Random.Range(0, Mathf.RoundToInt(4 / settings.Scale))));
                _riverTiles.AddRange(adjacentTiles.Where(t=> !_riverTiles.Contains(t)));
                currentTile = tile;
            }

            var waterTile = settings.Biomes.First(biome => biome.Type == BiomeType.Water).GetTile();
            foreach (var tile in tilesToFillWithWater)
            {
                tilemap.SetTile(new Vector3Int(tile.Coordinates.x, tile.Coordinates.y, 0), waterTile);
            }
        }
        
        private List<MapTile> GetRandomTiles(List<MapTile> list, int count)
        {
            var randomValues = new List<MapTile>();
            for (var i = 0; i < count; i++)
            {
                var randomIndex = Random.Range(0, list.Count);
                randomValues.Add(list[randomIndex]);
            }

            return randomValues;
        }

        private List<MapTile> GetAdjacentTiles(MapTile tile)
        {
            var adjacentTilesCoords = new List<Vector2Int>();
            if (tile.Coordinates.y % 2 == 0) // Even row
            {
                adjacentTilesCoords.Add(new Vector2Int(tile.Coordinates.x - 1, tile.Coordinates.y)); // West
                adjacentTilesCoords.Add(new Vector2Int(tile.Coordinates.x - 1, tile.Coordinates.y + 1)); // Northwest
                adjacentTilesCoords.Add(new Vector2Int(tile.Coordinates.x - 1, tile.Coordinates.y - 1)); // Southwest
                adjacentTilesCoords.Add(new Vector2Int(tile.Coordinates.x + 1, tile.Coordinates.y)); // East
                adjacentTilesCoords.Add(new Vector2Int(tile.Coordinates.x, tile.Coordinates.y + 1)); // Northeast
                adjacentTilesCoords.Add(new Vector2Int(tile.Coordinates.x, tile.Coordinates.y - 1)); // Southeast
            }
            else // Odd row
            {
                adjacentTilesCoords.Add(new Vector2Int(tile.Coordinates.x - 1, tile.Coordinates.y)); // West
                adjacentTilesCoords.Add(new Vector2Int(tile.Coordinates.x, tile.Coordinates.y + 1)); // Northwest
                adjacentTilesCoords.Add(new Vector2Int(tile.Coordinates.x, tile.Coordinates.y - 1)); // Southwest
                adjacentTilesCoords.Add(new Vector2Int(tile.Coordinates.x + 1, tile.Coordinates.y)); // East
                adjacentTilesCoords.Add(new Vector2Int(tile.Coordinates.x + 1, tile.Coordinates.y + 1)); // Northeast
                adjacentTilesCoords.Add(new Vector2Int(tile.Coordinates.x + 1, tile.Coordinates.y - 1)); // Southeast
            }

            var adjacentTiles = new List<MapTile>();
            foreach (var coord in adjacentTilesCoords)
            {
                if (coord.x >= settings.Size.x || coord.y >= settings.Size.y || coord.x < 0 || coord.y < 0)
                {
                    adjacentTiles.Clear();
                    break;
                }
                adjacentTiles.Add(_generatedTiles[coord.x, coord.y]);
            }
            
            return adjacentTiles;
        }
        
        #endregion
    }
}
