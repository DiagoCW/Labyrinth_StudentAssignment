using System.Collections.Generic;
using UnityEngine;

public struct EdgeToBajs 
{
    public Vector2Int To;
    public float Weight;

    public EdgeToBajs(Vector2Int to, float weight) 
    {
        To = to;
        Weight = weight;
    }
}

public class BajsScript
{
    private Dictionary<Vector2Int, List<EdgeToBajs>> adj;

    public BajsScript(IMapData m)
    {
        adj = new Dictionary<Vector2Int, List<EdgeToBajs>>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        for (int i = 0; i < m.Width; i++)
        {
            for (int j = 0; j < m.Height; j++)
            {
                Vector2Int current = new Vector2Int(i, j);
                adj[current] = new List<EdgeToBajs>();

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
                        int wallX;

                        if (dir.x > 0)
                        {
                            wallX = neighbor.x;
                        }
                        else
                        {
                            wallX = current.x;
                        }

                        float wallCost = m.GetVerticalWallCost(wallX, current.y);

                        if (wallCost > 1)
                            cost = wallCost;
                    }
                    else if (dir.y != 0) // rör sig i y led
                    {
                        int wallY;

                        if (dir.x > 0)
                        {
                            wallY = neighbor.y;
                        }
                        else
                        {
                            wallY = current.y;
                        }
                        float wallCost = m.GetHorizontalWallCost(current.x, wallY);
                        if (wallCost > 1)
                            cost = wallCost;
                    }

                    // Lägga i listan
                    adj[current].Add(new EdgeToBajs(neighbor, cost));
                }

                // kolla jumps
                List<JumpConnection> jumps = m.GetJumpsFrom(current);

                if (jumps != null)
                {
                    foreach (var jump in jumps)
                    {
                        adj[current].Add(new EdgeToBajs(jump.To, jump.Cost));
                    }
                }
            }
        }
    }

    public List<EdgeToBajs> Adjacent(Vector2Int node)
    {
        if (adj.ContainsKey(node))
            return adj[node];

        return new List<EdgeToBajs>(); // tom om inget finns
    }
}