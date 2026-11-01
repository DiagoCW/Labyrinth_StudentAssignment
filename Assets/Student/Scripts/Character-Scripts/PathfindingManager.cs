using System.Collections.Generic;
using UnityEngine;

public class PathfinderManager : MonoBehaviour
{
    [Header("References")]
    public JsonLoader jsonLoader;
    public GridCharacterMovement characterMovement;
    public PathfindingUI pathfindingUI;

    [Header("Settings")]
    public bool autoStart = true;
    public float startDelay = 2f;
    [Tooltip("If true, use BFS; if false, use Dijkstra")]
    public bool useBFS = true;

    [Header("Debug Visualization")]
    public bool showDebugPath = true;
    public Color pathColor = Color.yellow;
    public Color jumpPathColor = Color.cyan;
    public float pathNodeSize = 0.3f;

    private readonly List<GameObject> debugPathObjects = new();
    private IMapData mapDataInterface;

    private void Start()
    {
        if (pathfindingUI == null) pathfindingUI = FindAnyObjectByType<PathfindingUI>();
        if (autoStart) Invoke(nameof(StartPathfinding), startDelay);
    }

    public void StartPathfinding()
    {
        MapData mapData = jsonLoader.GetMapData();
        if (mapData == null)
        {
            Debug.LogError("No map data available.");
            pathfindingUI?.UpdatePathNotFound();
            return;
        }

        mapDataInterface = new MapDataAdapter(mapData);
        int questIndex = jsonLoader.GetQuestIndex();
        Vector2Int startPos = mapDataInterface.GetQuestStart(questIndex);
        Vector2Int goalPos = mapDataInterface.GetQuestGoal(questIndex);

        List<Vector2Int> path = PathfindingAlgorithm.FindShortestPath(startPos, goalPos, mapDataInterface, useBFS);
        if (path == null || path.Count == 0)
        {
            Debug.LogError("No path found.");
            pathfindingUI?.UpdatePathNotFound();
            return;
        }

        float totalCost = CalculatePathCost(path);
        int totalMoves = path.Count - 1;
        pathfindingUI?.UpdatePathInfo(totalCost, totalMoves, path.Count);

        if (showDebugPath) VisualizePath(path);
        characterMovement.SetPath(path);
    }

    private float CalculatePathCost(List<Vector2Int> path)
    {
        float totalCost = 0f;
        for (int i = 0; i < path.Count - 1; i++)
            totalCost += GetMovementCost(path[i], path[i + 1]);
        return totalCost;
    }

    private float GetMovementCost(Vector2Int from, Vector2Int to)
    {
        if (mapDataInterface.TryGetJumpCost(from, to, out float jumpCost))
            return jumpCost;

        float baseCost = 1f;
        int deltaX = to.x - from.x;
        int deltaY = to.y - from.y;

        if (deltaX != 0)
        {
            int wallX = deltaX > 0 ? to.x : from.x;
            float wallCost = mapDataInterface.GetVerticalWallCost(wallX, from.y);
            if (wallCost > 1f) baseCost = wallCost;
        }
        else if (deltaY != 0)
        {
            int wallY = deltaY > 0 ? to.y : from.y;
            float wallCost = mapDataInterface.GetHorizontalWallCost(from.x, wallY);
            if (wallCost > 1f) baseCost = wallCost;
        }

        return baseCost;
    }

    private void VisualizePath(List<Vector2Int> path)
    {
        ClearDebugPath();

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int position = path[i];
            bool arrivedByJump = i > 0 && mapDataInterface.TryGetJumpCost(path[i - 1], position, out _);

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = characterMovement.GetGrid().GetCellCenter(position.x, position.y) + Vector3.up * 0.1f;
            sphere.transform.localScale = Vector3.one * pathNodeSize;
            sphere.GetComponent<Renderer>().material.color = arrivedByJump ? jumpPathColor : pathColor;
            Destroy(sphere.GetComponent<Collider>());
            debugPathObjects.Add(sphere);

            if (arrivedByJump)
                CreateJumpLine(path[i - 1], position);
        }
    }

    private void CreateJumpLine(Vector2Int from, Vector2Int to)
    {
        GameObject lineObj = new GameObject("JumpLine");
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = jumpPathColor;
        line.endColor = jumpPathColor;
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.positionCount = 2;
        line.SetPosition(0, characterMovement.GetGrid().GetCellCenter(from.x, from.y) + Vector3.up * 0.2f);
        line.SetPosition(1, characterMovement.GetGrid().GetCellCenter(to.x, to.y) + Vector3.up * 0.2f);
        debugPathObjects.Add(lineObj);
    }

    private void ClearDebugPath()
    {
        foreach (GameObject obj in debugPathObjects)
            if (obj != null) Destroy(obj);
        debugPathObjects.Clear();
    }

    [ContextMenu("Find Path")]
    public void FindPathManual() => StartPathfinding();

    [ContextMenu("Clear Path Visualization")]
    public void ClearPathManual() => ClearDebugPath();

    private void OnDestroy() => ClearDebugPath();
}
