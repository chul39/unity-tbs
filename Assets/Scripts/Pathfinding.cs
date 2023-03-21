using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public static Pathfinding Instance { get; private set; }
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private LayerMask obstacleLayerMask;

    private int width;
    private int height;
    private float cellSize;
    private GridSystem<PathNode> gridSystem;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"Only a single instance of Pathfinding can exist at a time. ({transform} - {Instance})");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Setup(int width, int height, float cellSize)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        gridSystem = new GridSystem<PathNode>(
            width, height, cellSize, 
            (GridSystem<PathNode> g, GridPosition gridPosition) => new PathNode(gridPosition)
        );
        // gridSystem.CreateDebugObjects(gridDebugObjectPrefab);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                Vector3 worldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
                float raycastOffset = 5f;
                bool isHitObstacle = Physics.Raycast(
                    worldPosition + Vector3.down * raycastOffset, 
                    Vector3.up, 
                    raycastOffset * 2f,
                    obstacleLayerMask
                );
                if (isHitObstacle)
                {
                    GetNode(x, z).SetIsWalkable(false);
                }

            }
        }
    }

    public List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition endGridPosition, out int pathLength)
    {
        List<PathNode> openList = new List<PathNode>();
        List<PathNode> closedList = new List<PathNode>();
        PathNode startNode = gridSystem.GetGridObject(startGridPosition);
        PathNode endNode = gridSystem.GetGridObject(endGridPosition);
        openList.Add(startNode);
        for (int x = 0; x < gridSystem.GetWidth(); x++)
        {
            for (int z = 0; z < gridSystem.GetHeight(); z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                PathNode pathNode = gridSystem.GetGridObject(gridPosition);
                pathNode.SetGCost(int.MaxValue);
                pathNode.SetHCost(0);
                pathNode.CalculateFCost();
                pathNode.ResetOriginFromPathNode();
            }
        }
        startNode.SetGCost(0);
        startNode.SetHCost(CalculateDistance(startGridPosition, endGridPosition));
        startNode.CalculateFCost();
        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostPathNode(openList);
            if (currentNode == endNode)
            {
                pathLength = endNode.GetFCost();
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);
            foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                if (closedList.Contains(neighbourNode)) continue;
                if (!neighbourNode.IsWalkable())
                {
                    closedList.Add(neighbourNode);
                    continue;
                }
                
                int tentativeGCost = currentNode.GetGCost() + CalculateDistance(currentNode.GetGridPosition(), neighbourNode.GetGridPosition());
                if (tentativeGCost < neighbourNode.GetGCost())
                {
                    neighbourNode.SetOriginNode(currentNode);
                    neighbourNode.SetGCost(tentativeGCost);
                    neighbourNode.SetHCost(CalculateDistance(neighbourNode.GetGridPosition(), endGridPosition));
                    neighbourNode.CalculateFCost();      
                    if (!openList.Contains(neighbourNode)) openList.Add(neighbourNode);       
                }
            }
        }
        pathLength = 0;
        return null;
    }

    public int CalculateDistance(GridPosition a, GridPosition b)
    {
        GridPosition gridPositionDistance = a - b;
        int xDistance = Mathf.Abs(gridPositionDistance.x);
        int zDistance = Mathf.Abs(gridPositionDistance.z);
        int remaining = Mathf.Abs(xDistance - zDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, zDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode GetLowestFCostPathNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostPathNode = pathNodeList[0];
        for (int i = 0; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].GetFCost() < lowestFCostPathNode.GetFCost())
            {
                lowestFCostPathNode = pathNodeList[i];
            }
        }
        return lowestFCostPathNode;
    }

    private PathNode GetNode(int x, int z)
    {
        return gridSystem.GetGridObject(new GridPosition(x, z));
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();
        GridPosition gridPosition = currentNode.GetGridPosition();

        if (gridPosition.x - 1 >= 0) 
        {
            neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z)); // LEFT
            if (gridPosition.z - 1 >= 0) neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z - 1)); // LEFT-DOWN
            if (gridPosition.z + 1 < gridSystem.GetHeight()) neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z + 1)); // LEFT-UP
        }

        if (gridPosition.x + 1 < gridSystem.GetWidth()) 
        {
            neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z)); // RIGHT
            if (gridPosition.z - 1 >= 0) neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z - 1)); // RIGHT-DOWN
            if (gridPosition.z + 1 < gridSystem.GetHeight()) neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z + 1)); // RIGHT-UP
        }
        
        if (gridPosition.z - 1 >= 0) neighbourList.Add(GetNode(gridPosition.x, gridPosition.z - 1)); // DOWN
        if (gridPosition.z + 1 < gridSystem.GetHeight()) neighbourList.Add(GetNode(gridPosition.x, gridPosition.z + 1)); // UP

        return neighbourList;
    }

    private List<GridPosition> CalculatePath(PathNode endNode)
    {
        List<PathNode> pathNodeList = new List<PathNode>();
        pathNodeList.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.GetOriginNode() != null)
        {
            pathNodeList.Add(currentNode.GetOriginNode());
            currentNode = currentNode.GetOriginNode();
        }
        pathNodeList.Reverse();
        List<GridPosition> gridPositionList = new List<GridPosition>();
        foreach (PathNode pathNode in pathNodeList)
        {
            gridPositionList.Add(pathNode.GetGridPosition());
        }
        return gridPositionList;
    }

    public bool IsWalkableGridPosition(GridPosition gridPosition)
    {
        return gridSystem.GetGridObject(gridPosition).IsWalkable();
    }

    public bool IsReachable(GridPosition startPosition, GridPosition endPosition)
    {
        return FindPath(startPosition, endPosition, out int pathLength) != null;
    }

    public int GetPathLength(GridPosition startPosition, GridPosition endPosition)
    {
        FindPath(startPosition, endPosition, out int pathLength);
        return pathLength;
    }

}
