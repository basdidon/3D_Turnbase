using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Player : SerializedMonoBehaviour
{
    InputActions InputActions { get; set; }
    [SerializeField] Camera cam;
    [SerializeField] LayerMask NodeMask { get; set; }

    private void OnEnable()
    {
        InputActions.Enable();
    }

    private void OnDisable()
    {
        InputActions.Disable();
    }

    private void Awake()
    {
        InputActions = new InputActions();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        Ray ray = Camera.main.ScreenPointToRay(InputActions.Player.CursorPosition.ReadValue<Vector2>());
        Debug.DrawRay(ray.origin, ray.direction * 20f);
        if (Physics.Raycast(ray, out RaycastHit hit, 20f, NodeMask))
        {
            Debug.Log("found");
            var node = hit.transform.gameObject.GetComponentInParent<Node>();
            node.SetNodeState(NodeState.OnFocus);
        }
        */
    }
}
