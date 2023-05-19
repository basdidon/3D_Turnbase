using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public struct GenerateProb
{
    NodeData nodeData;
    float genProb;
}

[CreateAssetMenu(fileName = "Node",menuName = "Node")]
public class NodeData : SerializedScriptableObject
{
    public string nodeName;
    public GameObject prefab;

    [OdinSerialize]
    public List<GenerateProb> generateProbs;
}
