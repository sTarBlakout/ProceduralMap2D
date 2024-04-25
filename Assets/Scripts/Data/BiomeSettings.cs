using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Data
{
    [CreateAssetMenu(fileName = "BiomeSettings", menuName = "Data/BiomeSettings")]
    public class BiomeSettings : ScriptableObject
    {
        [SerializeField] private BiomeType type;
        [SerializeField] private List<TileChance> tileStructList;
        [SerializeField] private float minHeight;
        [SerializeField] private float minMoisture;
        [SerializeField] private float minHeat;

        public BiomeType Type => type;

        public bool MatchCondition (float height, float moisture, float heat)
        {
            return height >= minHeight && moisture >= minMoisture && heat >= minHeat;
        }
        
        public float GetDifference(float height, float moisture, float heat)
        {
            return (height - minHeight) + (moisture - minMoisture) + (heat - minHeat);
        }
        
        public Tile GetTile()
        {
            if (tileStructList.Count == 1) return tileStructList[0].tile;

            var chance = Random.value;
            var orderedTiles = tileStructList.OrderByDescending(tile => tile.chance).ToList();
            var chosenTile = orderedTiles[0].tile;

            foreach (var tileStruct in tileStructList)
            {
                if (tileStruct.chance > chance)
                    chosenTile = tileStruct.tile;
                else
                    break;
            }

            return chosenTile;
        }

        public float GetHeightForCell()
        {
            return Mathf.Clamp01(minHeight + Random.Range(0, 0.1f));
        }
    }

    public enum BiomeType
    {
        Water,
        Mountain,
        Forest,
        Jungle,
        Plain,
        Desert,
        Winter
    }

    [Serializable]
    public struct TileChance
    {
        public Tile tile;
        public float chance;
    }
}
