using System.Collections.Generic;
using UnityEngine;
using System;


/// <summary>
/// This class <c>LevelGrid</c> models the coordinate space of the level. It is a public singleton through which all classes interact with to access
/// the grid system logic.
/// This stores the three dimensional coordinate space, but not the pathfinding for agents. For that, check the <c>Pathfinding</c> class.
/// </summary>
public class LevelGrid : MonoBehaviour
{
    // Making this a public singleton makes it easier when working with code in prefabs.
    public static LevelGrid Instance { get; private set; }
    


    public event EventHandler OnAnyUnitMovedGridPosition;
    [SerializeField] private Transform gridObjectDebugPrefab;

    // These properties modify the shape, size, and number of gridsystems in the level.
    public const float FLOOR_HEIGHT = .5f;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;
    [SerializeField] private int maxFloors;

    private List<GridSystemHex<GridObject>> gridSystemList;

    /// <summary>
    /// When the LevelGrid is initialised, first check there isn't another another Levelgrid.
    /// Initialise as many grid systems as specified per distance on the scene's height axis.
    /// </summary>
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one LevelGrid! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        gridSystemList = new List<GridSystemHex<GridObject>>();

        // Create a GridSystem at set intervals of height for as many floors were specified.
        for (int floor = 0; floor < maxFloors; floor++)
        {
            GridSystemHex<GridObject> gridSystem = new GridSystemHex<GridObject>(width, height, cellSize, floor, FLOOR_HEIGHT,
                (GridSystemHex<GridObject> g, GridPosition gridPosition, CubeGridPosition axialGridPosition) => new GridObject(g, gridPosition, axialGridPosition));
            
            // Debug option that creates an overlay on every tile displaying its coordinates and the object occupying that space.
            // gridSystem.CreateDebugObject(gridObjectDebugPrefab);

            gridSystemList.Add(gridSystem);
        }
        gridSystemList[0].CreateDebugObject(gridObjectDebugPrefab);
    }

    /// <summary>
    /// After the coordinate space has been setup, create the pathfinding space.
    /// </summary>
    private void Start()
    {
        Pathfinding.Instance.Setup(width, height, cellSize, maxFloors);
    }

    /// <summary>
    /// Returns the grid system on the specified layer/floor.
    /// </summary>
    /// <param name="floor">The specific layer</param>
    /// <returns>The GridSystem representing the coordinate space on that layer</returns>
    private GridSystemHex<GridObject> GetGridSystemHex(int floor)
    {
        return gridSystemList[floor];
    }

    /// <summary>
    /// Add an agent to the coordiante space in the logic. This doesn't create an agent, it helps track where agents are in the coordinate space.
    /// </summary>
    /// <param name="gridPosition">The grid position containging the coordinates of the agent</param>
    /// <param name="agent">The agent itself.</param>
    public void AddAgentAtGridPosition(GridPosition gridPosition, Unit agent)
    {
        GetGridSystemHex(gridPosition.floor).GetGridObject(gridPosition).AddUnit(agent);
    }

    /// <summary>
    /// This method returns all the agents present at a position in the coordinate space.
    /// This method is currently not in use, however could be usefull in the future.
    /// </summary>
    /// <param name="gridPosition">The gridpostion in the coordinate space</param>
    /// <returns>A list of agents contained at the gridposition</returns>
    public List<Unit> GetAgentListAtGridPosition(GridPosition gridPosition)
    {
        return GetGridSystemHex(gridPosition.floor).GetGridObject(gridPosition).GetUnitList();
    }

    /// <summary>
    /// Removes a specific agent from the gridposition in the coordinate space.
    /// This might be called when an agent is no longer active or has moved from this position.
    /// </summary>
    /// <param name="gridPosition">The coordinates in the coordinate space</param>
    /// <param name="agent">The agent.</param>
    public void RemoveAgentAtGridPosition(GridPosition gridPosition, Unit agent)
    {
        GetGridSystemHex(gridPosition.floor).GetGridObject(gridPosition).RemoveUnit(agent);
    }

    /// <summary>
    /// This method is used for handling the logic of agents moving across the coordiante space.
    /// Classes that need to do something specific when an agent has changed coordinate can subscribe to this event.
    /// </summary>
    /// <param name="agent">The roaming agent</param>
    /// <param name="fromGridPosition">The origin of the agent as coordinates</param>
    /// <param name="toGridPosition">The destination of the agent as coordinates</param>
    public void AgentMovedGridPosition(Unit agent, GridPosition fromGridPosition, GridPosition toGridPosition)
    {
        RemoveAgentAtGridPosition(fromGridPosition, agent);
        AddAgentAtGridPosition(toGridPosition, agent);
        OnAnyUnitMovedGridPosition?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// This method is used to check if a position in the coordiante space contains an agent.
    /// </summary>
    /// <param name="gridPosition">The coordinates being checked</param>
    /// <returns>Returns a boolean true if there's an agent present</returns>
    public bool HasAnyAgentOnGridPosition(GridPosition gridPosition) 
    {
        GridObject gridObject = GetGridSystemHex(gridPosition.floor).GetGridObject(gridPosition);
        return gridObject.HasAnyUnit();
    }

    /// <summary>
    /// Returns a reference to the agent gameobject contained at a certain grid position.
    /// </summary>
    /// <param name="gridPosition">The coordinates</param>
    /// <returns>A reference to the agent at said coordinates</returns>
    public Unit GetAgentAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridSystemHex(gridPosition.floor).GetGridObject(gridPosition);
        return gridObject.GetUnit();
    }

    /// <summary>
    /// Converts the y of a Vector3 into an int that can be used to index a floor
    /// </summary>
    /// <param name="worldPosition">A vector3, perhaps from a mouse or from another class.</param>
    /// <returns>An int that can represents the floor the Vector3 can be localised to</returns>
    private int GetFloor(Vector3 worldPosition)
    {
        return Mathf.RoundToInt(worldPosition.y /FLOOR_HEIGHT);
    }

    /// <summary>
    /// A function to get the set amount of floors.
    /// </summary>
    /// <returns>Returns the number of gridsystems in play</returns>
    public int GetFloorAmount()
    {
        return maxFloors;
    }

    /// <summary>
    /// A method to get the closest grid position object for a given Vector3.
    /// </summary>
    /// <param name="worldPosition">A vector3 representing a fine location in the unity scene</param>
    /// <returns>A grid position representing a coordinate in the coordinate space</returns>
    public GridPosition GetGridPosition(Vector3 worldPosition) => GetGridSystemHex(GetFloor(worldPosition)).GetGridPosition(worldPosition);

    /// <summary>
    /// A method used to check if a grid position is within the bounds of the coordinate space
    /// </summary>
    /// <param name="gridPosition">A grid position containing a coordinate</param>
    /// <returns>Returns true if the gridposition is in bounds and therefore valid</returns>
    public bool IsValidGridPosition(GridPosition gridPosition)
    {
        if (gridPosition.floor < 0 || gridPosition.floor >= maxFloors)
        {
            return false;
        } else
        {
            return GetGridSystemHex(gridPosition.floor).IsValidGridPosition(gridPosition);
        }
    }

    /// <summary>
    /// A method used to convert a coordinate in a gridposition into a Vector3 representing a point in the unity scene.
    /// </summary>
    /// <param name="gridPosition">A coordinate from the coordintate space contained in a gridPositon object</param>
    /// <returns>A vector3 representing a fine point in the Unity scene</returns>
    public Vector3 GetWorldPosition(GridPosition gridPosition) => GetGridSystemHex(gridPosition.floor).GetWorldPosition(gridPosition);

    /// <summary>
    /// I mean come on, really? Take a guess. I can almost guarantee you'll know what this does if you read the method signature.
    /// </summary>
    /// <returns>You got this</returns>
    public int GetHeight() => GetGridSystemHex(0).GetHeight();

    /// <summary>
    /// If you guessed what the GetHeight method does, you can figure this one out on your own as well.
    /// </summary>
    /// <returns>I'll give you a hint, add a space between Get and Height</returns>
    public int GetWidth() => GetGridSystemHex(0).GetWidth();
}
