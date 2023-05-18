using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public enum NodeState 
{
    Idle,
    OnFocus,
}

public class Node : SerializedMonoBehaviour
{
    [OdinSerialize]
    [EnumToggleButtons]
    [OnValueChanged("SetNodeState")]
    public NodeState NodeState { get; private set; }

    [OdinSerialize] public Image FocusImage { get; private set; }

    public void SetNodeState(NodeState newNodeState)
    {
        NodeState = newNodeState;

        if(NodeState == NodeState.Idle)
            FocusImage.gameObject.SetActive(false);
        if (NodeState == NodeState.OnFocus)
            FocusImage.gameObject.SetActive(true);
    }

    private void OnMouseEnter()
    {
        SetNodeState(NodeState.OnFocus);
    }

    private void OnMouseExit()
    {
        SetNodeState(NodeState.Idle);
    }
}
