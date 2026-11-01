using System.Collections.Generic;
using UnityEngine;

public struct WeightedEdge 
{
    public Vector2Int To;
    public float Weight;

    public WeightedEdge(Vector2Int to, float weight) 
    {
        To = to;
        Weight = weight;
    }
}

/// <summary>
/// Weighted graph representation for Dijkstra; 
/// uses shortcuts through walls and portal jumps where each weight cost is considered
/// </summary>
public class WeightedGraph
{
    public Dictionary<Vector2Int, List<WeightedEdge>> Adj { get; private set; }
    public int V { get; private set; }
    public int E { get; private set; }
    public WeightedGraph(IMapData m)
    {
        V = m.Width * m.Height;
        Adj = new Dictionary<Vector2Int, List<WeightedEdge>>(V); // initialize adjacency list with node capacity
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        for (int i = 0; i < m.Width; i++)
        {
            for (int j = 0; j < m.Height; j++)
            {
                Vector2Int current = new Vector2Int(i, j);
                Adj[current] = new List<WeightedEdge>();

                foreach (Vector2Int dir in directions)
                {
                    Vector2Int neighbor = current + dir;

                    // Om rutan bredvid är utanför mapen så skit i den
                    if (neighbor.x < 0 || neighbor.x >= m.Width || neighbor.y < 0 || neighbor.y >= m.Height)
                        continue;

                    // Om pathen e blockerad av en mur eller något, skit i den
                    if (PathfindingAlgorithm.IsMovementBlocked(current, neighbor, m))
                        continue;

                    // default kostand att gå någonstans = 1
                    float cost = 1f;

                    // Checka om det finns vägg man kan klättra genom
                    if (dir.x != 0) // rör sig i x led
                    {
                        int wallX = dir.x > 0 ? neighbor.x : current.x;

                        float wallCost = m.GetVerticalWallCost(wallX, current.y);
                        if (wallCost > 1)
                            cost = wallCost;
                    }
                    else if (dir.y != 0) // rör sig i y led
                    {
                        int wallY = dir.y > 0 ? neighbor.y : current.y;
                        
                        float wallCost = m.GetHorizontalWallCost(current.x, wallY);
                        if (wallCost > 1)
                            cost = wallCost;
                    }

                    Adj[current].Add(new WeightedEdge(neighbor, cost));
                }

                // kolla jumps
                List<JumpConnection> jumps = m.GetJumpsFrom(current);

                if (jumps != null)
                {
                    foreach (var jump in jumps)
                    {
                        Adj[current].Add(new WeightedEdge(jump.To, jump.Cost));
                        //// borde varje jump vara riktad i endast en riktning?
                        //Adj[jump.To].Add(new WeightedEdge(current, jump.Cost));
                    }
                }
            }
        }

        // Count all edges
        foreach (var kvp in Adj)
        {
            E += kvp.Value.Count;
        }
        Debug.Log($"WeightedGraph created with {V} vertices and {E} edges");
    }

    public List<WeightedEdge> Adjacent(Vector2Int node)
    {
        if (Adj.TryGetValue(node, out var edges))
            return edges;

        return new List<WeightedEdge>(); // tom om inget finns
    }
}