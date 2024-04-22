using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Data
{
    [CreateAssetMenu(fileName = "BiomeSettings", menuName = "Data/BiomeSettings")]
    public class BiomeSettings : ScriptableObject
    {
        [SerializeField] private Tile tile;
        [SerializeField] private float minHeight;
        [SerializeField] private float minMoisture;
        [SerializeField] private float minHeat;

        public Tile Tile => tile;

        public bool MatchCondition (float height, float moisture, float heat)
        {
            return height >= minHeight && moisture >= minMoisture && heat >= minHeat;
        }
        
        public float GetDiffValue(float height, float moisture, float heat)
        {
            return (height - minHeight) + (moisture - minMoisture) + (heat - minHeat);
        }
    }
}
