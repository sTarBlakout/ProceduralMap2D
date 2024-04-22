using System;
using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Logic
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] private Grid grid;
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private MapGenerationSettings settings;

        private int _seed;

        private void Start()
        {
            GenerateMap();
        }

        public void GenerateMap()
        {
            _seed = DateTime.Now.Millisecond;

            var heightMap = GenerateNoise(settings.Size.x, settings.Size.y, settings.Scale, settings.HeightWaves);
            var moistureMap = GenerateNoise(settings.Size.x, settings.Size.y, settings.Scale, settings.MoistureWaves);
            var temperatureMap = GenerateNoise(settings.Size.x, settings.Size.y, settings.Scale, settings.TemperatureWaves);
            
            for (var x = 0; x < settings.Size.x; x++)
            for (var y = 0; y < settings.Size.y; y++)
            {
                var tile = GetBiome(heightMap[x, y], moistureMap[x, y], temperatureMap[x, y]).GetTile();
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }

            OffsetMapForCamera();
        }

        private void OffsetMapForCamera()
        {
            if (Camera.main != null) Camera.main.orthographicSize = settings.Size.x;
            tilemap.transform.position = new Vector3(-grid.cellSize.x * settings.Size.x / 2, -grid.cellSize.y * settings.Size.y / 3, 0);
        }
        
        private float[,] GenerateNoise(int width, int height, float scale, List<NoiseWave> waves)
        {
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
                        noiseMap[x, y] += wave.amplitude * Mathf.PerlinNoise(samplePosX * wave.frequency + _seed, samplePosY * wave.frequency + _seed);
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
    }
}
