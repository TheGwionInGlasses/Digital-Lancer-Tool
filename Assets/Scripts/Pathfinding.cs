using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// This class singelton <c>Pathfinding</c> handles the logic for pathfinding behaviour.
/// The class uses a basic implentation of the A* algorithm for pathfinding
/// </summary>
public class Pathfinding : MonoBehaviour
{
    public static Pathfinding Instance { get; private set; }

    private const int MOVE_STRAIGHT_COST = 10;
    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private LayerMask obstaclesLayerMark;
    [SerializeField] private LayerMask floorLayerMask;
    [SerializeField] private Transform pathfindingLinksContainer;

    // These parmaters determine the size and shape of the coordinate space in the Pathfinding class. They are determined in the <c>LevelGrid</c> class
    private int width;
    private int height;
    private int maxAlitude;
    private float cellSize;
    private List<GridSystemHex<PathNode>> gridSystemList;
    private List<PathfindingLink> pathfindingLinkList;

    /// <summary>
    /// On the object's creation, check there are no other instances of this singleton
    /// </summary>
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

    /// <summary>
    /// This method sets up layers of empty gridsystems used to contain instances of Pathnodes.
    /// The gridsystems are then itterated over to check if a Pathnode is walkable. A pathnode would not be walkable
    /// if it contains an obstacle.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="cellSize"></param>
    /// <param name="max_alitude"></param>
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
                (GridSystemHex<PathNode> g, GridPosition gridPosition, CubeGridPosition cubeGridPosition) => new PathNode(gridPosition, cubeGridPosition));
            
            gridSystemList.Add(gridSystem);
            // Create a dubugging overlay for the pathfinding. Not recommeneded to be turned on if the LevelGrid deebug overlay is also active.
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

                    // By default every Pathnode is unwalkable
                    GetNode(x, z, altitude).SetIsWalkable(false);
                    
                    // If there is a floor under the Pathnode, set the pathnode to be walkable
                    if (Physics.Raycast(
                        worldPosition + Vector3.up * raycastOffsetDistance, 
                        Vector3.down, 
                        raycastOffsetDistance * 2, 
                        floorLayerMask))
                    {
                        GetNode(x, z, altitude).SetIsWalkable(true);
                    }

                    // If these is an obstacle on the Pathnode, set the pathnode to be unwalkable.
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

        // For every pathfinding link between floors that exists in the Unity scene, track it in a list in this singleton.
        pathfindingLinkList = new List<PathfindingLink>();
        foreach (Transform pathfindingLinkTransform in pathfindingLinksContainer)
        {
            if (pathfindingLinkTransform.TryGetComponent(out PathfindingLinkMonoBehaviour pathfindingLinkMonoBehaviour))
            {
                pathfindingLinkList.Add(pathfindingLinkMonoBehaviour.GetPathfindingLink());
            }
        }
    }

    /// <summary>
    /// This method uses an A* algorithm to find a path between two grid positions.
    /// </summary>
    /// <param name="startGridPosition">The origin</param>
    /// <param name="endGridPosition">The destination</param>
    /// <param name="pathLength">The length of the path from origin to destination</param>
    /// <param name="ignoreTerrain">Whether to count for terrain</param>
    /// <returns></returns>
    public List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition endGridPosition, out int pathLength, bool ignoreTerrain = false)
    {
        List<PathNode> openList = new List<PathNode>();
        List<PathNode> closedList = new List<PathNode>();

        PathNode startNode = GetGridSystemAtAltitude(startGridPosition.floor).GetGridObject(startGridPosition);
        PathNode endNode = GetGridSystemAtAltitude(endGridPosition.floor).GetGridObject(endGridPosition);
        openList.Add(startNode);

        // Reset the cost of all the pathnodes in in the Pathfinding coordinate space
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

        // Calculate cost based on the distance between the two positions in the Unity scene.
        startNode.SetGCost(0);
        startNode.SetHCost(CalculateHeuristicDistance(startGridPosition, endGridPosition));
        startNode.CalculateFCost();

        // Whilst there are still pathnodes left to explore
        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostPathNode(openList);

            // If the current node is the destination, return a path to the destination
            if (currentNode == endNode)
            {
                pathLength = endNode.GetFCost();
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                // If the neighbour has already been explored, ignore the neighbour
                if (closedList.Contains(neighbourNode))
                {
                    continue;
                }

                // If the neighbour is not walkable, ignore the neighbour
                if (!neighbourNode.IsWalkable() && !ignoreTerrain)
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.GetGCost() + MOVE_STRAIGHT_COST;

                // If exploring the neighbour node will bring us closer to our destination, set the origin of the neighbour node to the current node and then add it to the open list.
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

    /// <summary>
    /// Calculates the Heuristic distance for pathfinding between two points. Calculates the distance based on the distance
    /// of the two gridpositions as Vector3.
    /// </summary>
    /// <param name="gridPositionA">The origin</param>
    /// <param name="gridPositionB">The destination</param>
    /// <returns>The Heuristic distance as an int</returns>
    public int CalculateHeuristicDistance(GridPosition gridPositionA, GridPosition gridPositionB)
    {
        return Mathf.RoundToInt(MOVE_STRAIGHT_COST * Vector3.Distance(GetGridSystemAtAltitude(gridPositionA.floor).GetWorldPosition(gridPositionA), 
            GetGridSystemAtAltitude(gridPositionB.floor).GetWorldPosition(gridPositionB)));
    }

    /// <summary>
    /// Return the Pathnode with the lowest F cost in a given list of Pathnodes
    /// </summary>
    /// <param name="pathNodeList">A list containing Pathnodes</param>
    /// <returns>Returns a Pathnode from the pathNodeList with the lowest F cost</returns>
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

    /// <summary>
    /// Get a gridsystem at the requested in index in the gridsystem list
    /// </summary>
    /// <param name="layer">Index of the grid system</param>
    /// <returns>A grid system containing Pathnodes</returns>
    private GridSystemHex<PathNode> GetGridSystemAtAltitude(int layer)
    {
        return gridSystemList[layer];
    }

    /// <summary>
    /// Returns a Pathnode using the three paramaters as indexes.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="floor"></param>
    /// <returns>A Pathnode</returns>
    private PathNode GetNode(int x, int z, int floor)
    {
        return GetGridSystemAtAltitude(floor).GetGridObject(new GridPosition(x, z, floor));
    }

    /// <summary>
    /// This method is used to get all the neighbouring Pathnodes to a specific pathnode.
    /// </summary>
    /// <param name="currentNode">The origin Pathnode</param>
    /// <returns>A list of neighbouring Pathnodes</returns>
    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        GridPosition gridPosition = currentNode.GetGridPosition();

        // Get the neighbouring pathnodes from the same floor
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

        // Get the neighbouring Pathnodes using the on connected floors using the list of PathfindingLink
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

    /// <summary>
    /// Method used to get all the grid position linked to the paramater grid position in the list of pathfinding links.
    /// </summary>
    /// <param name="gridPosition">The origin</param>
    /// <returns>A list of grid positions on different floors theoretically reachable from the origin</returns>
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

    /// <summary>
    /// This returns a list of gridpositions by passing through each path node's origin until the origin is set to null.
    /// </summary>
    /// <param name="endNode">The destination</param>
    /// <returns>A list of gridpositions that can be read as a path to the end node</returns>
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

    /// <summary>
    /// Method used to check if a gridposition is walkable.
    /// </summary>
    /// <param name="gridPosition"></param>
    /// <returns>True if the gridposition is walkable.</returns>
    public bool IsWalkableGridPosition(GridPosition gridPosition)
    {
        return GetGridSystemAtAltitude(gridPosition.floor).GetGridObject(gridPosition).IsWalkable();
    }

    /// <summary>
    /// Method used to check if there is a path between two grid positions.
    /// </summary>
    /// <param name="startGridPosition">The origin</param>
    /// <param name="endGridPosition">The destination</param>
    /// <returns>true if there exists a path between the origin and the destination</returns>
    public bool HasPath(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        return FindPath(startGridPosition, endGridPosition, out int pathLength) != null;
    }

    /// <summary>
    /// This method is used to check the length of the a path between two gridpositions
    /// </summary>
    /// <param name="startGridPosition">The origin</param>
    /// <param name="endGridPosition">The destination</param>
    /// <returns>The length of the path counted in gridpositions traversed to reach the destination</returns>
    public int GetPathLength(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        FindPath(startGridPosition, endGridPosition, out int pathLength);
        return pathLength;
    }

    /// <summary>
    /// This method is used to check the length of a path between two gridpositions ignoring obstacles and terrain cost.
    /// </summary>
    /// <param name="startGridPosition">The origin</param>
    /// <param name="endGridPosition">The destination</param>
    /// <returns>The length of the path counted in gridpositions traversed to reach the destination</returns>
    public int GetPathLengthIgnoreTerrain(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        FindPath(startGridPosition, endGridPosition, out int pathLength, true);
        return pathLength;
    }
}
