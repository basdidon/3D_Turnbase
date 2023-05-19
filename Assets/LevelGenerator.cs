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

    [Space]
    [OdinSerialize]
    [ShowInInspector]
    Dictionary<Vector3Int, GameObject> tiles;

    public bool isFallInTiles;
    [ShowIfGroup("isFallInTiles")]
    [BoxGroup("isFallInTiles/FallInTilesProps")]
    [SerializeField] float fallingHeight = 5f, fallingDuration = 1f;

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
        tilePrefab = Random.Range(0f, 1f) < 0.5f ? tilePrefabDict["Grass"] : tilePrefabDict["Water"];
        GameObject clone = Instantiate(tilePrefab, new Vector3(cellPos.x + tileHalfExtent.x, 0, cellPos.z + tileHalfExtent.z), Quaternion.identity, transform);
        clone.name = string.Format("Node {0},{1}", cellPos.x, cellPos.z);
        tiles.Add(cellPos, clone);

        if(isFallInTiles)
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

    [ButtonGroup("genLevel",ButtonHeight = 20)]
    public void GenerateLevel()
    {
        StartCoroutine(SpawnTile(Vector3Int.zero));
    }

    [ButtonGroup("genLevel")]
    public void ResetLevel()
    {
        foreach(var t in tiles.Keys)
        {
            Destroy(tiles[t]);
        }
        tiles.Clear();
    }
}
