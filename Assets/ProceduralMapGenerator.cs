using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

[System.Serializable]
public class Wave
{
    public float seed;
    public float frequency;
    public float amplitude;
}

public class BiomeTempData
{
    public BiomePreset biome;
    public BiomeTempData(BiomePreset preset)
    {
        biome = preset;
    }

    public float GetDiffValue(float height, float moisture, float heat)
    {
        return (height - biome.minHeight) + (moisture - biome.minMoisture) + (heat - biome.minHeat);
    }
}

public class ProceduralMapGenerator : SerializedMonoBehaviour
{
    [Header("WaveFunction")]
    public BiomePreset[] biomes;
    public GameObject tilePrefab;

    [Header("Dimensions")]
    [SerializeField] Vector3Int mapSize = new Vector3Int(100,0,100);
    public float scale = 1.0f;
    public Vector2 offset;

    [Header("Height Map")]
    public Wave[] heightWaves;
    private float[,] heightMap;
    [Header("Moisture Map")]
    public Wave[] moistureWaves;
    private float[,] moistureMap;
    [Header("Heat Map")]
    public Wave[] heatWaves;
    private float[,] heatMap;

    [Space]
    [OdinSerialize]
    [ShowInInspector]
    Dictionary<Vector3Int, GameObject> cloneDict;

    [SerializeField] float dalaySpawn = .2f;
    public bool isFallInTiles;
    [ShowIfGroup("isFallInTiles")]
    [BoxGroup("isFallInTiles/FallInTilesProps")]
    [SerializeField] float fallingHeight = 5f, fallingDuration = 1f;

    [ButtonGroup("genLevel")]
    void GenerateMap()
    {
        // height map
        heightMap = NoiseGenerator.Generate(mapSize.x, mapSize.z, scale, heightWaves, offset);
        // moisture map
        moistureMap = NoiseGenerator.Generate(mapSize.x, mapSize.z, scale, moistureWaves, offset);
        // heat map
        heatMap = NoiseGenerator.Generate(mapSize.x, mapSize.z, scale, heatWaves, offset);

        
        for (int x = 0; x < mapSize.x; ++x)
        {
            for (int y = 0; y <  mapSize.z; ++y)
            {
                Instantiate(GetBiome(heightMap[x, y], moistureMap[x, y], heatMap[x, y]).GetTilePrefab(), new Vector3(x, 0, y), Quaternion.identity,this.transform);
            }
        }
        
        
    }

    [ButtonGroup("genLevel")]
    public void SpawnTiles()
    {
        StartCoroutine(SpawnTile(Vector3Int.zero));
    }


    [ButtonGroup("genLevel")]
    public void ResetLevel()
    {
        foreach (var t in cloneDict.Values)
        {
            Destroy(t);
        }
        cloneDict.Clear();
    }

    BiomePreset GetBiome(float height, float moisture, float heat)
    {
        List<BiomeTempData> biomeTemp = new List<BiomeTempData>();

        foreach (BiomePreset biome in biomes)
        {
            if (biome.MatchCondition(height, moisture, heat))
            {
                biomeTemp.Add(new BiomeTempData(biome));
            }
        }

        float curVal = Mathf.Infinity;
        BiomePreset biomeToReturn;

        biomeToReturn = biomes[0];

        foreach (BiomeTempData biome in biomeTemp)
        {
            if (biome.GetDiffValue(height, moisture, heat) < curVal)
            {
                Debug.Log("yes");
                biomeToReturn = biome.biome;
                curVal = biome.GetDiffValue(height, moisture, heat);
            }
        }

        return biomeToReturn;
    }

    IEnumerator SpawnTile(Vector3Int cellPos)
    {
        var x = cellPos.x;
        var y = cellPos.z;
        GameObject clone = Instantiate(GetBiome(heightMap[x, y], moistureMap[x, y], heatMap[x, y]).GetTilePrefab(), new Vector3(x, 0, y), Quaternion.identity, this.transform);
        //GameObject clone = Instantiate(tilePrefabDict[tileKeyDict[cellPos]], new Vector3(cellPos.x + tileHalfExtent.x, 0, cellPos.z + tileHalfExtent.z), Quaternion.identity, transform);
        clone.name = string.Format("Node {0},{1}", cellPos.x, cellPos.z);
        //cloneDict.Add(cellPos, clone);

        if (isFallInTiles)
            StartCoroutine(FallInTile(clone.transform));

        if (cellPos.x + 1 < mapSize.x)
        {
            yield return new WaitForSeconds(dalaySpawn);
            yield return SpawnTile(cellPos + Vector3Int.right);
        }
        else
        {
            if (cellPos.z + 1 < mapSize.z)
            {
                yield return new WaitForSeconds(dalaySpawn);
                yield return SpawnTile(new Vector3Int(0, cellPos.y, cellPos.z + 1));
            }
            else
            {
                yield return null;
            }
        }
    }

    IEnumerator FallInTile(Transform transform)
    {
        Vector3 startPos = transform.position + Vector3.up * fallingHeight;
        Vector3 targetPos = transform.position;

        float timeElapsed = 0f;

        while (timeElapsed < fallingDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, timeElapsed / fallingDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
            transform.position = targetPos;
        }
    }
}
