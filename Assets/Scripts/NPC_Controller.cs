using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class NPC_Controller : MonoBehaviour
{
    public Node currentNode;
    public List<Node> path = new List<Node>();

    public Button newPath;

    private void Awake()
    {
        newPath.onClick.AddListener(() =>
        {
            Node[] nodes = NodeManager.instance.GetNodes();
            path = AStarManager.instance.GeneratePath(currentNode, nodes[Random.Range(0, nodes.Length)]);
        });
    }

    private void Update()
    {
        CreatePath();
    }

    public void CreatePath()
    {
        if (path.Count > 0)
        {
            int x = path.Count - 1;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(path[x].transform.position.x, path[x].transform.position.y, -2), 3 * Time.deltaTime);

            if (Vector2.Distance(transform.position, path[x].transform.position) < 0.1f)
            {
                currentNode = path[x];
                path.RemoveAt(x);
            }
        }
    }
}
