using System;
using System.Collections.Generic;
using Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Logic
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private MapGenerationSettings settings;

        private void Start()
        {
            GenerateMap();
        }

        private void GenerateMap()
        {
            var heightMap = GenerateNoise(settings.Size.x, settings.Size.y, settings.Scale, settings.HeightWaves, settings.Offset);
            var moistureMap = GenerateNoise(settings.Size.x, settings.Size.y, settings.Scale, settings.MoistureWaves, settings.Offset);
            var temperatureMap = GenerateNoise(settings.Size.x, settings.Size.y, settings.Scale, settings.TemperatureWaves, settings.Offset);
            
            for (var x = 0; x < settings.Size.x; x++)
            for (var y = 0; y < settings.Size.y; y++)
            {
                var tile = GetBiome(heightMap[x, y], moistureMap[x, y], temperatureMap[x, y]).Tile;
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
        
        private float[,] GenerateNoise(int width, int height, float scale, List<NoiseWave> waves, Vector2 offset)
        {
            var noiseMap = new float[width, height];
            for(var x = 0; x < width; ++x)
            {
                for(var y = 0; y < height; ++y)
                {
                    var samplePosX = x * scale + offset.x;
                    var samplePosY = y * scale + offset.y;
                    
                    var normalization = 0.0f;
                    foreach(var wave in waves)
                    {
                        noiseMap[x, y] += wave.amplitude * Mathf.PerlinNoise(samplePosX * wave.frequency + wave.seed, samplePosY * wave.frequency + wave.seed);
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
                    curVal = biome.GetDiffValue(height, moisture, heat);
                }
                else
                {
                    if (biome.GetDiffValue(height, moisture, heat) < curVal)
                    {
                        biomeToReturn = biome;
                        curVal = biome.GetDiffValue(height, moisture, heat);
                    }
                }
            }
            
            if(biomeToReturn == null)
                biomeToReturn = settings.Biomes[0];
            
            return biomeToReturn;
        }
    }
}
