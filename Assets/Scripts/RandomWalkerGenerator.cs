using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RandomWalkerGenerator : MonoBehaviour
{
    public enum Grid
    {
        Floor,
        Wall,
        Empty
    }

    public Grid[,] grid;

    public List<Walker> walkers;
    public Tilemap floorMap;
    public Tilemap wallMap;

    public Tile floor;
    public Tile wall;

    public int mapWidth;
    public int mapHeight;

    public int maxWalkers = 10;
    public int tileCount = default;
    public float fillPercent = 0.5f;

    public Node nodePrefab;
    public List<Node> nodeList;

    public NPC_Controller npc;

    private bool canDrawGizmos;

    private void Awake()
    {
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        grid = new Grid[mapWidth, mapHeight];

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j] = Grid.Empty;
            }
        }

        walkers = new List<Walker>();

        Vector3Int tileCenterPos = new Vector3Int(grid.GetLength(0) / 2, grid.GetLength(1) / 2);

        Walker newWalker = new Walker(new Vector2(tileCenterPos.x, tileCenterPos.y), GetDirection(), 0.5f);
        grid[tileCenterPos.x, tileCenterPos.y] = Grid.Floor;
        floorMap.SetTile(tileCenterPos, floor);
        walkers.Add(newWalker);

        tileCount++;

        CreateFloors();
    }

    public Vector2 GetDirection()
    {
        int choice = Mathf.FloorToInt(UnityEngine.Random.value * 3.99f);

        switch (choice)
        {
            case 0:
                return Vector2.down;
            case 1:
                return Vector2.up;
            case 2:
                return Vector2.left;
            case 3:
                return Vector2.right;
            default: return Vector2.zero;
        }
    }

    void CreateFloors()
    {
        while ((float)tileCount / (float)grid.Length < fillPercent)
        {
            foreach (Walker curWalker in walkers)
            {
                Vector3Int curPos = new Vector3Int((int)curWalker.pos.x, (int)curWalker.pos.y, 0);

                if (grid[curPos.x, curPos.y] != Grid.Floor)
                {
                    floorMap.SetTile(curPos, floor);
                    tileCount++;
                    grid[curPos.x, curPos.y] = Grid.Floor;
                }
            }

            //Walker Methods
            ChanceToRemove();
            ChanceToRedirect();
            ChanceToCreate();
            UpdatePosition();
        }

        CreateWalls();
    }

    void ChanceToRemove()
    {
        int updatedCount = walkers.Count;
        for (int i = 0; i < updatedCount; i++)
        {
            if (UnityEngine.Random.value < walkers[i].chanceToChange && walkers.Count > 1)
            {
                walkers.RemoveAt(i);
                break;
            }
        }
    }

    void ChanceToRedirect()
    {
        for (int i = 0; i < walkers.Count; i++)
        {
            if (UnityEngine.Random.value < walkers[i].chanceToChange)
            {
                Walker curWalker = walkers[i];
                curWalker.dir = GetDirection();
                walkers[i] = curWalker;
            }
        }
    }

    void ChanceToCreate()
    {
        int updatedCount = walkers.Count;
        for (int i = 0; i < updatedCount; i++)
        {
            if (UnityEngine.Random.value < walkers[i].chanceToChange && walkers.Count < maxWalkers)
            {
                Vector2 newDirection = GetDirection();
                Vector2 newPosition = walkers[i].pos;

                Walker newWalker = new Walker(newPosition, newDirection, 0.5f);
                walkers.Add(newWalker);
            }
        }
    }

    void UpdatePosition()
    {
        for (int i = 0; i < walkers.Count; i++)
        {
            Walker foundWalker = walkers[i];
            foundWalker.pos += foundWalker.dir;
            foundWalker.pos.x = Mathf.Clamp(foundWalker.pos.x, 1, grid.GetLength(0) - 2);
            foundWalker.pos.y = Mathf.Clamp(foundWalker.pos.y, 1, grid.GetLength(1) - 2);
            walkers[i] = foundWalker;
        }
    }

    void CreateWalls()
    {
        for (int x = 0; x < grid.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < grid.GetLength(1) - 1; y++)
            {
                if (grid[x, y] == Grid.Floor)
                {
                    if (grid[x + 1, y] == Grid.Empty)
                    {
                        wallMap.SetTile(new Vector3Int(x + 1, y, 0), wall);
                        grid[x + 1, y] = Grid.Wall;
                    }
                    if (grid[x - 1, y] == Grid.Empty)
                    {
                        wallMap.SetTile(new Vector3Int(x - 1, y, 0), wall);
                        grid[x - 1, y] = Grid.Wall;
                    }
                    if (grid[x, y + 1] == Grid.Empty)
                    {
                        wallMap.SetTile(new Vector3Int(x, y + 1, 0), wall);
                        grid[x, y + 1] = Grid.Wall;
                    }
                    if (grid[x, y - 1] == Grid.Empty)
                    {
                        wallMap.SetTile(new Vector3Int(x, y - 1, 0), wall);
                        grid[x, y - 1] = Grid.Wall;
                    }
                }
            }
        }

        CreateNodes();
    }

    void CreateNodes()
    {
        for(int x = 0; x < grid.GetLength(0); x++)
        {
            for(int y = 0; y < grid.GetLength(1); y++)
            {
                if (grid[x,y] == Grid.Floor)
                {
                    Node node = Instantiate(nodePrefab, new Vector2(x+ 0.5f, y + 0.5f), Quaternion.identity);
                    nodeList.Add(node);
                }
            }
        }
        CreateConnections();
    }

    void CreateConnections()
    {
        for(int i = 0; i < nodeList.Count; i++)
        {
            for(int j = i + 1; j < nodeList.Count; j++)
            {
                if (Vector2.Distance(nodeList[i].transform.position, nodeList[j].transform.position) <= 1.0f)
                {
                    ConnectNodes(nodeList[i], nodeList[j]);
                    ConnectNodes(nodeList[j], nodeList[i]);
                }
            }
        }
        canDrawGizmos = true;
        SpawnAI();
    }

    void ConnectNodes(Node from, Node to)
    {
        if(from == to) { return; }

        from.connections.Add(to);
    }

    void SpawnAI()
    {
        Node randNode = nodeList[Random.Range(0, nodeList.Count)];

        NPC_Controller newNPC = Instantiate(npc, randNode.transform.position, Quaternion.identity);

        newNPC.currentNode = randNode;
    }

/*    private void OnDrawGizmos()
    {
        if (canDrawGizmos)
        {
            Gizmos.color = Color.blue;
            for(int i =0; i < nodeList.Count; i++)
            {
                for(int j = 0; j < nodeList[i].connections.Count; j++)
                {
                    Gizmos.DrawLine(nodeList[i].transform.position, nodeList[i].connections[j].transform.position);
                }
            }
        }
    }*/
}

public class Walker
{
    public Vector2 pos;
    public Vector2 dir;
    public float chanceToChange;

    public Walker(Vector2 p, Vector2 d, float c)
    {
        pos = p;
        dir = d;
        chanceToChange = c;
    }
}