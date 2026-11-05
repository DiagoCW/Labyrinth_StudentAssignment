using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public struct Edge
{
    public Vector2Int To { get; private set; }
    public Edge(Vector2Int to)
    {
        To = to;
    }
}
/// <summary>
/// Unweighted graph representation for BFS
/// </summary>
public class Graph
{
    public int V { get; private set; }
    public int E { get; private set; }
    public int nbrWalls { get; private set; }
    public Dictionary<Vector2Int, List<Edge>> Adj { get; private set; }
    readonly Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    public Graph(IMapData m)
    {
        V = m.Width * m.Height;
        Adj = new Dictionary<Vector2Int, List<Edge>>();

        for (int i = 0; i < m.Width; i++)
        {
            for (int j = 0; j < m.Height; j++)
            {
                Vector2Int current = new Vector2Int(i, j);
                Adj[current] = new List<Edge>();

                foreach (Vector2Int dir in directions)
                {
                    Vector2Int neighbor = current + dir;

                    // Om rutan bredvid är utanför mapen så skit i den
                    if (neighbor.x < 0 || neighbor.x >= m.Width || neighbor.y < 0 || neighbor.y >= m.Height)
                        continue;

                    // Om pathen e blockerad av en mur eller något, skit i den
                    //if (PathfindingAlgorithm.IsMovementBlocked(current, neighbor, m))
                    //    continue;

                    // Om det finns en genväg (genvägg höhöhö) så är det ej en giltig edge för BFS
                    if (dir.x != 0) // rör sig i x led
                    {
                        int wallX = dir.x > 0 ? neighbor.x : current.x;

                        float wallCost = m.GetVerticalWallCost(wallX, current.y);
                        if (wallCost > 1f)
                        {
                            nbrWalls++;
                            continue;
                        }
                            
                    }
                    else if (dir.y != 0) // rör sig i y led
                    {
                        int wallY = dir.y > 0 ? neighbor.y : current.y;

                        float wallCost = m.GetHorizontalWallCost(current.x, wallY);
                        if (wallCost > 1f)
                        {
                            nbrWalls++;
                            continue;
                        }
                            
                    }
                    //Debug.Log($"Found edge from {current} to {neighbor}");
                    Adj[current].Add(new Edge(neighbor));
                    E++;
                }

            }
        }

        Debug.Log($"Unweighted Graph created with {V} vertices and {E} edges and {nbrWalls} walls.");
        foreach (var e in Adj)
        {
            Debug.Log($"Node ({e.Key}) has edge to: ");
            var edges = Adjacent(e.Key);
            foreach (var edge in edges)
            {
                Debug.Log($"{edge.To}, ");
            }
        }
    }

    public List<Edge> Adjacent(Vector2Int v)
    {
        if (Adj.TryGetValue(v, out var edges))
            return edges;
        return new List<Edge>();
    }

}
