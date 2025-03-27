using System.Collections.Generic;
using UnityEngine;
using System;

public class LevelGrid : MonoBehaviour
{
    public static LevelGrid Instance { get; private set; }
    public const float FLOOR_HEIGHT = .5f;


    public event EventHandler OnAnyUnitMovedGridPosition;
    [SerializeField] private Transform gridObjectDebugPrefab;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;
    [SerializeField] private int maxAltitude;

    private List<GridSystemHex<GridObject>> gridSystemList;
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

        for (int floor = 0; floor < maxAltitude; floor++)
        {
            GridSystemHex<GridObject> gridSystem = new GridSystemHex<GridObject>(width, height, cellSize, floor, FLOOR_HEIGHT,
                (GridSystemHex<GridObject> g, GridPosition gridPosition) => new GridObject(g, gridPosition));
            gridSystem.CreateDebugObject(gridObjectDebugPrefab);

            gridSystemList.Add(gridSystem);
        }
        
    }

    private void Start()
    {
        Pathfinding.Instance.Setup(width, height, cellSize, maxAltitude);
    }

    private GridSystemHex<GridObject> GetGridSystemHex(int floor)
    {
        return gridSystemList[floor];
    }

    public void AddUnitAtGridPosition(GridPosition gridPosition, Unit unit)
    {
        GetGridSystemHex(gridPosition.floor).GetGridObject(gridPosition).AddUnit(unit);
    }

    public List<Unit> GetUnitListAtGridPosition(GridPosition gridPosition)
    {
        return GetGridSystemHex(gridPosition.floor).GetGridObject(gridPosition).GetUnitList();
    }

    public void RemoveUnitAtGridPosition(GridPosition gridPosition, Unit unit)
    {
        GetGridSystemHex(gridPosition.floor).GetGridObject(gridPosition).RemoveUnit(unit);
    }

    public void UnitMovedGridPosition(Unit unit, GridPosition fromGridPosition, GridPosition toGridPosition)
    {
        RemoveUnitAtGridPosition(fromGridPosition, unit);
        AddUnitAtGridPosition(toGridPosition, unit);
        OnAnyUnitMovedGridPosition?.Invoke(this, EventArgs.Empty);
    }

    public bool HasAnyUnitOnGridPosition(GridPosition gridPosition) 
    {
        GridObject gridObject = GetGridSystemHex(gridPosition.floor).GetGridObject(gridPosition);
        return gridObject.HasAnyUnit();
    }

    public Unit GetUnitAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridSystemHex(gridPosition.floor).GetGridObject(gridPosition);
        return gridObject.GetUnit();
    }

    public int GetFloor(Vector3 worldPosition)
    {
        return Mathf.RoundToInt(worldPosition.y /FLOOR_HEIGHT);
    }

    public int GetAltitudeAmount()
    {
        return maxAltitude;
    }

    public GridPosition GetGridPosition(Vector3 worldPosition) => GetGridSystemHex(GetFloor(worldPosition)).GetGridPosition(worldPosition);

    public bool IsValidGridPosition(GridPosition gridPosition)
    {
        if (gridPosition.floor < 0 || gridPosition.floor >= maxAltitude)
        {
            return false;
        } else
        {
            return GetGridSystemHex(gridPosition.floor).IsValidGridPosition(gridPosition);
        }
    }

    public Vector3 GetWorldPosition(GridPosition gridPosition) => GetGridSystemHex(gridPosition.floor).GetWorldPosition(gridPosition);

    public int GetHeight() => GetGridSystemHex(0).GetHeight();

    public int GetWidth() => GetGridSystemHex(0).GetWidth();
}
