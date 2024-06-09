using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    public static NodeManager instance;

    private void Awake()
    {
        instance = this;
    }

    public Node[] GetNodes()
    {
        return FindObjectsOfType<Node>();
    }
}
