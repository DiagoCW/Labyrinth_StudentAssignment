using System.Collections.Generic;
using UnityEditor.TerrainTools;
using UnityEngine;

public static class PathfindingAlgorithm
{
    // TODO: Implement the pathfinding assignment here.
    // Find the lowest-cost path from start to goal using the map dimensions, wall costs and directed jumps exposed by IMapData.
    public static List<Vector2Int> FindShortestPath(Vector2Int start, Vector2Int goal, IMapData mapData)
    {
        // BFS implementation
        // För en BFS behövs en Queue för o hålla ordning på vilka noder (rutor) som ska checkas next (FIFO)
        // Ett HashSet för att hålla koll på vilka rutor som är redan besökta
        // Ett dictionary som sparar information var man kom ifrån när man når en viss ruta. Behövs för att få ihop vägen baklänges näör man hittat målet

        Queue<Vector2Int> adjacentSQUARES = new();
        HashSet<Vector2Int> visited = new();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new();

        // start pos
        adjacentSQUARES.Enqueue(start);
        visited.Add(start);

        // fyra riktningarna som finns
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left };

        bool foundPath = false;

        while (adjacentSQUARES.Count > 0) 
        {
            Vector2Int currentSquare = adjacentSQUARES.Dequeue();

            if (currentSquare == goal) 
            {
                foundPath = true;
                break;
            }

            foreach (Vector2Int dir in directions) 
            {
                Vector2Int adjacentSquare = currentSquare + dir;

                // Om rutan bredvid är utanför mapen så skit i den
                if (adjacentSquare.x < 0 || adjacentSquare.x >= mapData.Width || adjacentSquare.y < 0 || adjacentSquare.y >= mapData.Height)
                    continue;

                // Om rutan är redan besökt, skit i den
                if (visited.Contains(adjacentSquare))
                    continue;

                // Om pathen e blockerad av en mur eller något, skit i den
                if (IsMovementBlocked(currentSquare, adjacentSquare, mapData))
                    continue;

                // Lägg till rutan som ligger granne längst bak i kön så man kan checka dess grannar senare
                adjacentSQUARES.Enqueue(adjacentSquare); 

                // Markera rutan som besökt
                visited.Add(adjacentSquare); 

                // Spara vägen man gått
                cameFrom[adjacentSquare] = currentSquare;
            }
        }

        if (!foundPath) return null; // om vi inte hittade någott.... ja då var det synd

        // Få ihop pathen
        List<Vector2Int> path = new();
        Vector2Int currentPathNode = goal;

        while(currentPathNode != start) 
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
}
