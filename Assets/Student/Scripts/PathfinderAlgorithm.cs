using System.Collections.Generic;
using UnityEditor.TerrainTools;
using UnityEngine;

public static class PathfindingAlgorithm
{
    // TODO: Implement the pathfinding assignment here.
    // Find the lowest-cost path from start to goal using the map dimensions, wall costs and directed jumps exposed by IMapData.
    public static List<Vector2Int> FindShortestPath(Vector2Int start, Vector2Int goal, IMapData mapData, bool searchFlag)
    {
        return searchFlag ? 
            FindShortestPathBFS(start, goal, mapData) : FindShortestPathDijkstra(start, goal, mapData);
    }

    public static List<Vector2Int> FindShortestPathBFS(Vector2Int start, Vector2Int goal, IMapData mapData) 
    {
        Graph graph = new(mapData);
        // BFS implementation
        // För en BFS behövs en Queue för o hålla ordning på vilka noder (rutor) som ska checkas next (FIFO)
        // Ett HashSet för att hålla koll på vilka rutor som är redan besökta
        // Ett dictionary som sparar information var man kom ifrån när man når en viss ruta. Behövs för att få ihop vägen baklänges näör man hittat målet

        Queue<Vector2Int> adjacentSQUARES = new(graph.V);
        HashSet<Vector2Int> visited = new(graph.V);
        Dictionary<Vector2Int, Vector2Int> cameFrom = new(graph.V);

        // start pos
        adjacentSQUARES.Enqueue(start);
        Vector2Int current = start;
        visited.Add(start);

        while(adjacentSQUARES.Count > 0)
        {
            current = adjacentSQUARES.Dequeue();
            if (current == goal) break;

            foreach (var edge in graph.Adjacent(current))
            {
                Vector2Int neighbor = edge.To;
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    adjacentSQUARES.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        //// fyra riktningarna som finns
        //Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left };

        //bool foundPath = false;

        //while (adjacentSQUARES.Count > 0)
        //{
        //    Vector2Int currentSquare = adjacentSQUARES.Dequeue();

        //    if (currentSquare == goal)
        //    {
        //        foundPath = true;
        //        break;
        //    }

        //    foreach (Vector2Int dir in directions)
        //    {
        //        Vector2Int adjacentSquare = currentSquare + dir;

        //        // Om rutan bredvid är utanför mapen så skit i den
        //        if (adjacentSquare.x < 0 || adjacentSquare.x >= mapData.Width || adjacentSquare.y < 0 || adjacentSquare.y >= mapData.Height)
        //            continue;

        //        // Om rutan är redan besökt, skit i den
        //        if (visited.Contains(adjacentSquare))
        //            continue;

        //        // Om pathen e blockerad av en mur eller något, skit i den
        //        if (IsMovementBlocked(currentSquare, adjacentSquare, mapData))
        //            continue;

        //        // Lägg till rutan som ligger granne längst bak i kön så man kan checka dess grannar senare
        //        adjacentSQUARES.Enqueue(adjacentSquare);

        //        // Markera rutan som besökt
        //        visited.Add(adjacentSquare);

        //        // Spara vägen man gått
        //        cameFrom[adjacentSquare] = currentSquare;
        //    }
        //}

        if (current != goal) return null; // om vi inte hittade någott.... ja då var det synd

        // Få ihop pathen
        List<Vector2Int> path = new();
        Vector2Int currentPathNode = goal;

        while (currentPathNode != start)
        {
            path.Add(currentPathNode);
            currentPathNode = cameFrom[currentPathNode];
        }

        // Strukturera om så det det start till goal
        path.Add(start);
        path.Reverse();

        return path;
    }

    public static bool IsMovementBlocked(Vector2Int from, Vector2Int to, IMapData mapData)
    {
        if (mapData.TryGetJumpCost(from, to, out _))
            return false;

        int deltaX = to.x - from.x;
        int deltaY = to.y - from.y;

        if (Mathf.Abs(deltaX) + Mathf.Abs(deltaY) != 1)
            return true;

        if (deltaX != 0)
        {
            int wallX = deltaX > 0 ? to.x : from.x;
            float wallCost = mapData.GetVerticalWallCost(wallX, from.y);
            return float.IsPositiveInfinity(wallCost) || wallCost >= float.MaxValue;
        }

        int wallY = deltaY > 0 ? to.y : from.y;
        float horizontalWallCost = mapData.GetHorizontalWallCost(from.x, wallY);
        return float.IsPositiveInfinity(horizontalWallCost) || horizontalWallCost >= float.MaxValue;
    }
    
    public static List<Vector2Int> FindShortestPathDijkstra(Vector2Int start, Vector2Int goal, IMapData mapData)
    {
        // Dijkstras algoritm utifrån bokens implementation s. 655 anpassad för att fungera här
        
        WeightedGraph graph = new(mapData); // skapa grafen

        Dictionary<Vector2Int, float> distTo = new(graph.V); // Håller koll på minsta kostnaden från start till x ruta.
        Dictionary<Vector2Int, Vector2Int> edgeTo = new(graph.V); // sparar vilken ruta man kom från för att nå hit billigast
        PriorityQueue<Vector2Int> pq = new(graph.V); // Sorterar rutorna så man går till den med lägst kostnad först

        foreach(var node in graph.Adj.Keys)
        {
            distTo[node] = float.PositiveInfinity;
        }

        // start
        // segt att pq här tar ba int , måste omvandla float -> int sen
        distTo[start] = 0f; // Kostanden för startrutan är 0
        Vector2Int v = start;
        pq.Enqueue(v, 0); // pq.insert(s, 0 ,0) 

        do
        {
            v = pq.Dequeue(); // pq.delMin

            // bryt loopen
            if (v == goal) break;

            // reelllaaaaxxx
            foreach (var e in graph.Adjacent(v))  // loopa genom alla edges/väger man kan nå från ruta/node 'v' - for(DirectedEdge e : G.adj(v))
            {
                Vector2Int w = e.To; // w är grannen - int w = e.to();

                // om vägen till w via vår nuvarande nod/ruta v är billigare än det bästa man sett hittils
                // if (distTo[w] > distTo[v] + e.weight())
                if (distTo[w] > distTo[v] + e.Weight)
                {
                    distTo[w] = distTo[v] + e.Weight; // Update kortaste avstånded
                    edgeTo[w] = v; // spara att detta är bästa man kan nå w från v
                    // Lägg grannen i kön så man kan checka dess edges senare
                    // * 10 för varje weight eftersom vissa weights är 5.5, blir slutändan samma resultat när man letar kortast avstånd
                    // pq.insert/change
                    pq.Enqueue(w, (int)(distTo[w] * 10));
                }

            }
        }
        while (pq.Count > 0); // while (!pq.isEmpty()) -> relax

        if (v != goal) return null; 

        List<Vector2Int> path = new List<Vector2Int>();

        while (v != start) // följer edgeTo från mål till start
        {
            path.Add(v);
            v = edgeTo[v];
        }

        path.Add(start);
        path.Reverse();

        return path;
    }
}
