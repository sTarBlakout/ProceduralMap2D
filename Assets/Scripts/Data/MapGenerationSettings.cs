using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data
{
    [CreateAssetMenu(fileName = "MapGenerationSettings", menuName = "Data/MapGenerationSettings")]
    public class MapGenerationSettings : ScriptableObject
    {
        [Header("General")]
        [SerializeField] private Vector2Int size;
        [SerializeField] private float scale;
        [SerializeField] private List<BiomeSettings> biomes;
        [SerializeField] private int maxRiversAmount;
        
        [Header("Waves")]
        [SerializeField] private List<NoiseWave> heightWaves;
        [SerializeField] private List<NoiseWave> moistureWaves;
        [SerializeField] private List<NoiseWave> temperatureWaves;

        public Vector2Int Size => size;
        public float Scale => scale;
        public int MaxRiversAmount => maxRiversAmount;
        public List<BiomeSettings> Biomes => biomes;
        public List<NoiseWave> HeightWaves => heightWaves;
        public List<NoiseWave> MoistureWaves => moistureWaves;
        public List<NoiseWave> TemperatureWaves => temperatureWaves;
    }

    [Serializable]
    public struct NoiseWave
    {
        public float frequency;
        public float amplitude;
    }
}
