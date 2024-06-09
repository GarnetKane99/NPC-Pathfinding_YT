using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Node cameFrom;
    public List<Node> connections;

    public float gScore;
    public float hScore;

    public float FScore()
    {
        return gScore + hScore;
    }
}
