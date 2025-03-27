using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public static Pathfinding Instance { get; private set; }

    private const int MOVE_STRAIGHT_COST = 10;
    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private LayerMask obstaclesLayerMark;
    [SerializeField] private LayerMask floorLayerMask;
    [SerializeField] private Transform pathfindingLinksContainer;

    private int width;
    private int height;
    private int maxAlitude;
    private float cellSize;
    private List<GridSystemHex<PathNode>> gridSystemList;
    private List<PathfindingLink> pathfindingLinkList;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one Pathfinding! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Setup(int width, int height, float cellSize, int max_alitude)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.maxAlitude = max_alitude;

        gridSystemList = new List<GridSystemHex<PathNode>>();

        for (int altitude = 0; altitude < max_alitude; altitude++)
        {
            GridSystemHex<PathNode> gridSystem = new GridSystemHex<PathNode>(width, height, cellSize, altitude, LevelGrid.FLOOR_HEIGHT,
                (GridSystemHex<PathNode> g, GridPosition gridPosition) => new PathNode(gridPosition));
            
            gridSystemList.Add(gridSystem);
            //gridSystem.CreateDebugObject(gridDebugObjectPrefab);
        }
        
        

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                for (int altitude = 0; altitude < max_alitude; altitude++)
                {
                    GridPosition gridPosition = new GridPosition(x, z, altitude);
                    Vector3 worldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
                    float raycastOffsetDistance = .2f;

                    GetNode(x, z, altitude).SetIsWalkable(false);
                    
                    if (Physics.Raycast(
                        worldPosition + Vector3.up * raycastOffsetDistance, 
                        Vector3.down, 
                        raycastOffsetDistance * 2, 
                        floorLayerMask))
                    {
                        GetNode(x, z, altitude).SetIsWalkable(true);
                    }

                    if (Physics.Raycast(
                        worldPosition + Vector3.down * raycastOffsetDistance, 
                        Vector3.up, 
                        raycastOffsetDistance * 2, 
                        obstaclesLayerMark))
                    {
                        GetNode(x, z, altitude).SetIsWalkable(false);
                    }
                }
            }
        }

        pathfindingLinkList = new List<PathfindingLink>();
        foreach (Transform pathfindingLinkTransform in pathfindingLinksContainer)
        {
            if (pathfindingLinkTransform.TryGetComponent(out PathfindingLinkMonoBehaviour pathfindingLinkMonoBehaviour))
            {
                pathfindingLinkList.Add(pathfindingLinkMonoBehaviour.GetPathfindingLink());
            }
        }
    }

    public List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition endGridPosition, out int pathLength, bool ignoreTerrain = false)
    {
        List<PathNode> openList = new List<PathNode>();
        List<PathNode> closedList = new List<PathNode>();

        PathNode startNode = GetGridSystemAtAltitude(startGridPosition.floor).GetGridObject(startGridPosition);
        PathNode endNode = GetGridSystemAtAltitude(endGridPosition.floor).GetGridObject(endGridPosition);
        openList.Add(startNode);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                for (int altitude = 0; altitude < maxAlitude; altitude++)
                {
                    GridPosition gridPosition = new GridPosition(x, z, altitude);
                    PathNode pathNode = GetGridSystemAtAltitude(altitude).GetGridObject(gridPosition);

                    pathNode.SetGCost(int.MaxValue);
                    pathNode.SetHCost(0);
                    pathNode.CalculateFCost();
                    pathNode.ResetCameFromPathNode();
                }
            }
        }

        startNode.SetGCost(0);
        startNode.SetHCost(CalculateHeuristicDistance(startGridPosition, endGridPosition));
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
                if (closedList.Contains(neighbourNode))
                {
                    continue;
                }

                if (!neighbourNode.IsWalkable() && !ignoreTerrain)
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.GetGCost() + MOVE_STRAIGHT_COST;

                if (tentativeGCost < neighbourNode.GetFCost())
                {
                    neighbourNode.SetCameFromPathNode(currentNode);
                    neighbourNode.SetGCost(tentativeGCost);
                    neighbourNode.SetHCost(ignoreTerrain ? 0 : CalculateHeuristicDistance(neighbourNode.GetGridPosition(), endGridPosition));
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }
        // No Path Found
        pathLength = 0;
        return null;
    }

    public int CalculateHeuristicDistance(GridPosition gridPositionA, GridPosition gridPositionB)
    {
        // I have no clue what's going on here. Manhattan distance has been kicked to the curb and we're returning to straight distance.
        return Mathf.RoundToInt(MOVE_STRAIGHT_COST * Vector3.Distance(GetGridSystemAtAltitude(gridPositionA.floor).GetWorldPosition(gridPositionA), 
            GetGridSystemAtAltitude(gridPositionB.floor).GetWorldPosition(gridPositionB)));
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

    private GridSystemHex<PathNode> GetGridSystemAtAltitude(int altitude)
    {
        return gridSystemList[altitude];
    }

    private PathNode GetNode(int x, int z, int altitude)
    {
        return GetGridSystemAtAltitude(altitude).GetGridObject(new GridPosition(x, z, altitude));
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        GridPosition gridPosition = currentNode.GetGridPosition();

        

        if (gridPosition.x - 1 >=0)
        {
            // Left
            neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z + 0, gridPosition.floor));
        }

        if (gridPosition.x + 1 < width)
        {
            // Right
            neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z + 0, gridPosition.floor));
        }
        if (gridPosition.z - 1 >= 0)
        {
            // Up
            neighbourList.Add(GetNode(gridPosition.x + 0, gridPosition.z - 1, gridPosition.floor));
        }
        if (gridPosition.z + 1 < height)
        {
            // Down
            neighbourList.Add(GetNode(gridPosition.x + 0, gridPosition.z + 1, gridPosition.floor));
        }

        bool oddRow = gridPosition.z % 2 == 1;

        if (oddRow)
        {
            if (gridPosition.x + 1 < width)
            {
                if (gridPosition.z - 1 >= 0)
                {
                    neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z - 1, gridPosition.floor));
                }
                if (gridPosition.z + 1 < height)
                {
                    neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z + 1, gridPosition.floor));
                }
            }

        } else
        {
            if (gridPosition.x - 1 >= 0)
            {
                if (gridPosition.z - 1 >= 0)
                {
                    neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z - 1, gridPosition.floor));
                }
                if (gridPosition.z + 1 < height)
                {
                    neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z + 1, gridPosition.floor));
                }
            }
        }

        List<PathNode> totalNeighbourList = new List<PathNode>();
        totalNeighbourList.AddRange(neighbourList);

        List<GridPosition> pathfindingLinkGridPositionList = GetPathFindingLinkConnectedGridPositionList(gridPosition);

        foreach (GridPosition pathfindingLinkGridPosition in pathfindingLinkGridPositionList)
        {
            totalNeighbourList.Add(
                GetNode(
                    pathfindingLinkGridPosition.x,
                    pathfindingLinkGridPosition.z,
                    pathfindingLinkGridPosition.floor
                )
            );
        }
        return totalNeighbourList;
    }

    private List<GridPosition> GetPathFindingLinkConnectedGridPositionList(GridPosition gridPosition)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();

        foreach (PathfindingLink pathfindingLink in pathfindingLinkList)
        {
            if (pathfindingLink.gridPositionA == gridPosition)
            {
                gridPositionList.Add(pathfindingLink.gridPositionB);
            }
            if (pathfindingLink.gridPositionB == gridPosition)
            {
                gridPositionList.Add(pathfindingLink.gridPositionA);
            }
        }

        return gridPositionList;
    }

    private List<GridPosition> CalculatePath(PathNode endNode)
    {
        List<PathNode> pathNodeList = new List<PathNode>();

        pathNodeList.Add(endNode);
        PathNode currentNode = endNode;

        while (currentNode.GetCameFromPathNode() != null)
        {
            pathNodeList.Add(currentNode.GetCameFromPathNode());
            currentNode = currentNode.GetCameFromPathNode();
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
        return GetGridSystemAtAltitude(gridPosition.floor).GetGridObject(gridPosition).IsWalkable();
    }

    public bool HasPath(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        return FindPath(startGridPosition, endGridPosition, out int pathLength) != null;
    }

    public int GetPathLength(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        FindPath(startGridPosition, endGridPosition, out int pathLength);
        return pathLength;
    }

    public int GetPathLengthIgnoreTerrain(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        FindPath(startGridPosition, endGridPosition, out int pathLength, true);
        return pathLength;
    }
}
