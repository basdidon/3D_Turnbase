using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class LevelGenerator : SerializedMonoBehaviour
{
    [Required]
    [SerializeField]
    GameObject tilePrefab;
    [SerializeField]
    Vector3 tileHalfExtent = Vector3.one / 2;

    [HorizontalGroup("mapSize", Title = "Horizontal Group Title")]
    [SerializeField] Vector3Int mapSize;
    [SerializeField] float dalaySpawn = .2f;

    [Space]
    [OdinSerialize]
    [ShowInInspector]
    Dictionary<string, GameObject> tilePrefabDict;

    [HorizontalGroup("grassProbs")]
    public float grassToGrass = 3f, grassToWater = 1f;
    [HorizontalGroup("waterProbs")]
    public float waterToGrass = 1f, waterToWater = 2f;
    [Space]
    [OdinSerialize]
    [ShowInInspector]
    Dictionary<Vector3Int, string> tileKeyDict;

    [Space]
    [OdinSerialize]
    [ShowInInspector]
    Dictionary<Vector3Int, GameObject> cloneDict;

    public bool isFallInTiles;
    [ShowIfGroup("isFallInTiles")]
    [BoxGroup("isFallInTiles/FallInTilesProps")]
    [SerializeField] float fallingHeight = 5f, fallingDuration = 1f;

    readonly List<Vector3Int> vector3IntDirs = new List<Vector3Int>() { Vector3Int.forward ,Vector3Int.back,Vector3Int.left,Vector3Int.right};

    private void Start()
    {
        tileKeyDict = new Dictionary<Vector3Int, string>();
        cloneDict = new Dictionary<Vector3Int, GameObject>();
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

    IEnumerator SpawnTile(Vector3Int cellPos)
    {
        GameObject clone = Instantiate(tilePrefabDict[tileKeyDict[cellPos]], new Vector3(cellPos.x + tileHalfExtent.x, 0, cellPos.z + tileHalfExtent.z), Quaternion.identity, transform);
        clone.name = string.Format("Node {0},{1}", cellPos.x, cellPos.z);
        cloneDict.Add(cellPos, clone);

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

    [ButtonGroup("genLevel", ButtonHeight = 50)]
    public void GenerateLevel()
    {
        tileKeyDict.Clear();

        for (int i = 0; i < mapSize.z; i++)
        {
            for (int j = 0; j < mapSize.x; j++)
            {
                Vector3Int currentCell = new Vector3Int(j, 0, i);
                // peek at neighbor tile
                //  grass [grass:3,water:1]
                //  water [grass:1,water:2]
                var probs = new float[]{0, 0};    //[grassProb,waterProb]

                foreach(var dir in vector3IntDirs)
                {
                    if (tileKeyDict.TryGetValue(currentCell + dir, out string key))
                    {
                        if (key == "Grass")
                        {
                            probs[0] += grassToGrass;
                            probs[1] += grassToWater;
                        }
                        else
                        {
                            probs[0] += waterToGrass;
                            probs[1] += waterToWater;
                        }
                    }
                }
                
                tileKeyDict.Add(currentCell, Random.Range(0f, probs[0]+probs[1]) < probs[0] ? "Grass" : "Water");
            }
        }

        var grassCount = 0;

        foreach(var v in tileKeyDict.Values)
        {
            if(v == "Grass")
            {
                grassCount++;
            }
        }

        Debug.Log(string.Format("Grass/Water : {0}/{1}", grassCount, (mapSize.x * mapSize.z) - grassCount));

    }

    [ButtonGroup("genLevel")]
    public void SpawnTiles()
    {
        if (tileKeyDict.Count == 0)
            GenerateLevel();
        if (cloneDict.Count > 0)
            ResetLevel();
        cloneDict.Clear();
        StartCoroutine(SpawnTile(Vector3Int.zero));
    }

    [ButtonGroup("genLevel")]
    public void ResetLevel()
    {
        foreach(var t in cloneDict.Values)
        {
            Destroy(t);
        }
        cloneDict.Clear();
    }
}
