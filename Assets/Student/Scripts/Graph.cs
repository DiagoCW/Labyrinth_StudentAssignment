using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Graph
{
    int V; // nbr of nodes, W - 1 x H - 1 of map si<w
    public List<List<KeyValuePair<int, int>>> Adj { get; private set; } // Node 1 has an edge to Node 3 with a weight of 4
                                                                        // and an edge to Node 2 with a weight of 3:
                                                                        // adj[1] = { 3, 4 } { 2, 3 }
                                                                        // Example: Get edge weight from node 1 to node 2:
                                                                        // adj[1][2].Value = 3
    public PriorityQueue<int> MinPQ { get; private set; }
    // public bool[] visited { get; private set; }
    
    //public struct Node
    //{

    //}

    public Graph(IMapData m)
    {
        V = (m.Width - 1) * (m.Height) - 1;
        Adj = new(V);
        MinPQ = new();
    }

    public void AddEdge(int u, int v, int weight)
    {
        Adj[u].Add(new KeyValuePair<int, int>(v, weight));
    }

    public int GetWeight(int from, int to)
    {
        if (Adj[from].Contains(Adj[from][to]))
            return Adj[from][to].Value;
        return -1;
    }

    //// Flytta till PathfinderAlgorithm.cs
    //public void Relax()
    //{

    //}

}
