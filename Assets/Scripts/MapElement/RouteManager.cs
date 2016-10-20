using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;


public class RouteManager : MonoBehaviour
{
    //********* Graphics Asset ***********
    public GameObject horizontalBar;
    public GameObject verticalBar;
    public GameObject horizontalBarMain;
    public GameObject verticalBarMain;
    public GameObject horizontalBarHighlight;
    public GameObject verticalBarHighlight;
    public GameObject fullTable;
    public GameObject halfTable;
    public GameObject stall;

    //******** Special Nodes **********
    public Node[] map_entries { get; private set; }
    public GameObject[] map_stalls { get; private set; }


    //******** Map Element **********
    //Use a graph of coordinates to represent the grid
    //Each edge should be used to represent a path

    //Initialize the map
    public void initialize()
    {
        map_entries = new Node[4];
        map_stalls = new GameObject[10];
        Node[,] tempMap = new Node[24, 19];
        //Initialize Node Types
        //IMPORTANT TODO: Change the path to an executable one after publishing
        int[,] grid = readLayoutFromFile(Environment.CurrentDirectory + "\\Assets\\Scripts\\MapElement\\layout_cornerGrid.txt");
        int[,] table = readLayoutFromFile(Environment.CurrentDirectory + "\\Assets\\Scripts\\MapElement\\layout_withTable.txt");

        for (int i = 0; i < tempMap.GetLength(0); ++i)
        {
            for (int j = 0; j < tempMap.GetLength(1); ++j)
            {
                switch(grid[i, j])
                {
                    case 9: tempMap[i, j] = new Node(new Coordinates(i, j), Node.NodeType.Stall);
                        break;
                    case 7: tempMap[i, j] = new Node(new Coordinates(i, j), Node.NodeType.Entry);
                        break;
                    case 2: tempMap[i, j] = new Node(new Coordinates(i, j), Node.NodeType.Corner);
                        break;
                    case 1: tempMap[i, j] = new Node(new Coordinates(i, j), Node.NodeType.Common);
                        break;
                }
            }
        }

        //Connect them according to types
        for (int i = 0; i < tempMap.GetLength(0); ++i)
        {
            for (int j = 0; j < tempMap.GetLength(1); ++j)
            {
                Node cur = tempMap[i, j];
                bool hasBottom = false, hasLeft = false;
                if (cur == null)
                    continue;
                
                //Connect to neighbors
                if (j < tempMap.GetLength(1) - 1 && tempMap[i, j + 1] != null)
                    cur.addConnection(tempMap[i, j + 1]);
                if (j > 0 && tempMap[i, j - 1] != null)
                {
                    hasBottom = true;
                    cur.addConnection(tempMap[i, j - 1]);
                }
                if (i < tempMap.GetLength(0) - 1 && tempMap[i + 1, j] != null)
                    cur.addConnection(tempMap[i + 1, j]);
                if (i > 0 && tempMap[i - 1, j] != null)
                {
                    hasLeft = true;
                    cur.addConnection(tempMap[i - 1, j]);
                }

                //Special Nodes - Stalls
                if (cur.nodeType == Node.NodeType.Stall)
                {
                    GameObject g = Instantiate(stall);
                    Stall stallScript = g.GetComponent<Stall>();
                    stallScript.ID = GlobalConstants.STALL_IDS[i - 6];
                    map_stalls[i - 6] = g;
                    //g.transform.parent = this.transform;
                    Vector3 v = new Vector3(i, j, GlobalConstants.Z_BOTTOM_NETWORK_2);
                    g.transform.position = v;
                }
                //Tables
                else if(table[i, j] == 2)
                    addElement(ElementType.Table, i - 0.5f, j - 0.5f);
                else if (table[i, j] == 3)
                    addElement(ElementType.HalfTable, i - 0.5f, j - 0.5f);
                //Special Nodes - Entry Point
                if (cur.nodeType == Node.NodeType.Entry)
                {
                    switch (i)
                    {
                        case 0: map_entries[0] = cur; break;
                        case 5: map_entries[1] = cur; break;
                        case 23: if (j > 0) map_entries[2] = cur;
                            else map_entries[3] = cur; break;
                        default: throw new Exception("Invalid Entry Position? " + i + " " + j);
                    }
                }
                //Graphic Add
                if (hasLeft)
                {
                    addElement(ElementType.HorizontalBar, i - 0.5f, j);
                }
                if (hasBottom)
                {
                    addElement(ElementType.VerticalBar, i, j - 0.5f);
                }
            }
        }
    }

    private int[,] readLayoutFromFile(string path)
    {
        String input_grid = File.ReadAllText(path);

        int x = 18, y = 0;
        int[,] grid = new int[24, 19];
        foreach (var row in input_grid.Split('\n'))
        {
            y = 0;  //y is col, x is row here
            foreach (var col in row.Trim().Split(' '))
            {
                grid[y, x] = int.Parse(col.Trim());
                y++;
            }
            x--;
        }
        return grid;
    }

    public enum ElementType
    {
        HorizontalBar, VerticalBar, Stall, Table, HalfTable
    }

    private GameObject addElement(ElementType type, float x, float y)
    {
        //larger the z, lower the layer
        GameObject obj;
        float z = GlobalConstants.Z_BOTTOM_NETWORK_1;
        switch (type)
        {
            case ElementType.HorizontalBar:
                obj = GameObject.Instantiate(horizontalBar);
                break;
            case ElementType.VerticalBar:
                obj = GameObject.Instantiate(verticalBar);
                break;
                /*
            case ElementType.Stall:
                z = GlobalConstants.Z_BOTTOM_STATIC;
                obj = GameObject.Instantiate(stall);
                break;*/
            case ElementType.Table:
                z = GlobalConstants.Z_BOTTOM_STATIC;
                obj = GameObject.Instantiate(fullTable);
                break;
            case ElementType.HalfTable:
                z = GlobalConstants.Z_BOTTOM_STATIC;
                obj = GameObject.Instantiate(halfTable);
                break;
            default: obj = null;
                break;
        }
        obj.transform.parent = this.transform;
        Vector3 v = new Vector3(x, y, z);
        obj.transform.position = v;
        return obj;
    }
    
    //**************** Path finding ******************
    //A* Algorithm
    //Returns null if fail
    //Returns empty set if start is end
    public List<Node> getPath(Node start, Node end)
    {
        //Heuristic and current best
        Dictionary<Node, float> f = new Dictionary<Node, float>();  //heuristic estimate from start to destination
        Dictionary<Node, float> g = new Dictionary<Node, float>();  //concrete current best from start
        Dictionary<Node, Node> bestFrom = new Dictionary<Node, Node>();
        
        Func<Node, List<Node>> retrievePath = null;
        retrievePath = (last) =>
        {
            if (last == null)   //Base case
                return new List<Node>();
            List<Node> res = retrievePath(bestFrom[last]);
            res.Add(last);
            return res;
        };
        List<Node> closed = new List<Node>();
        List<Node> open = new List<Node>();
        Func<Dictionary<Node, float>, Node, float> read = (m, k) =>
        {
            float res = float.PositiveInfinity;
            m.TryGetValue(k, out res);
            return res;
        };

        //Adding the first node
        open.Add(start);
        g[start] = 0;
        bestFrom[start] = null;
        //Iteration
        while(open.Count > 0)
        {
            Node cur = open.OrderBy(n => read(f, n)).ToList().First();   //Get the best from openSet
            if (cur == end)
            {
                return retrievePath(cur);
            }
            open.Remove(cur);
            closed.Add(cur);
            foreach(Node neighbor in cur.connection)
            {
                if (closed.Contains(neighbor))
                    continue;   //ignore evaluated nodes
                float score = read(g, cur) + 1;
                if (!open.Contains(neighbor))
                    open.Add(neighbor);  //Discovered new node!
                else if (score >= read(g, neighbor))
                    continue;
                bestFrom[neighbor] = cur;
                g[neighbor] = score;
                f[neighbor] = score + Coordinates.distGrid(cur.coordinates, end.coordinates);
            }
        }
        return null;
    }

    public GameObject motherForHighlight;
    private List<GameObject> highlightedPath = new List<GameObject>();
    //To remove highlight, pass in null
    public void Highlight(List<Node> path)
    {
        //Destroy All Current Highlighted
        foreach (GameObject g in highlightedPath)
            Destroy(g);
        highlightedPath = new List<GameObject>();

        //Add in new values
        if (path == null)
            return;
        Node pre = null, next = null;
        foreach(Node n in path)
        {
            if (next == null) //first node, put in next
            {
                next = n;
                continue;
            }
            pre = next;
            next = n;
            GameObject g;
            if (Utility.checkFloatEqual(next.coordinates.y, pre.coordinates.y))
            {
                //Horizontal
                g = Instantiate(horizontalBarHighlight);
            } else
            {
                g = Instantiate(verticalBarHighlight);
            }
            g.transform.parent = motherForHighlight.transform;
            g.transform.position = new Vector3((next.coordinates.x + pre.coordinates.x) / 2, (next.coordinates.y + pre.coordinates.y) / 2, GlobalConstants.Z_BOTTOM_NETWORK_3);
            highlightedPath.Add(g);
        }
    }
}

public class Node
{
    public enum NodeType
    {
        Common, Corner, Stall, Entry
    }
    public NodeType nodeType { get; private set; }
    public Coordinates coordinates { get; private set; }

    public List<Node> connection;
    //public readonly List<Node> connection_main;

    public Node(Coordinates c, NodeType t)
    {
        coordinates = c;
        nodeType = t;
        connection = new List<Node>();
    }

    public void addConnection(Node con)
    {
        connection.Add(con);
    }

    public override int GetHashCode()
    {
        return coordinates.GetHashCode();
    }

    public override string ToString()
    {
        return coordinates.ToString();
    }
}

public class Coordinates
{
    public readonly float x;
    public readonly float y;

    public Coordinates(float a, float b)
    {
        x = a; y = b;
    }

    public static float dist(Coordinates a, Coordinates b)
    {
        return (float)Math.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y));
    }

    public static float distGrid(Coordinates a, Coordinates b)
    {
        return Math.Abs(b.y - a.y) + Math.Abs(b.x - a.x);
    }

    public override bool Equals(object obj)
    {
        var item = obj as Coordinates;

        if (item == null)
        {
            return false;
        }

        return Utility.checkFloatEqual(this.x, item.x) && Utility.checkFloatEqual(this.y, item.y);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return x + " " + y;
    }
}



