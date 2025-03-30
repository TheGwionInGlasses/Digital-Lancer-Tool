using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This singleton handles the highlighting of valid grid positions in play. It contains a three dimensional array of GridSystemVisual objects
/// that are disabled by default but can be enabled to highlight tiles in different colours.
/// The bulk of the logic is in the UpdateGridVisual method. Rather than call this method repeatedly in the Update function, this method
/// is called when needed through a serious of event listeners to events in the backend.
/// </summary>
public class GridSystemVisual : MonoBehaviour
{
    [Serializable] public struct GridVisualTypeMaterial 
    {
        public GridVisualType gridVisualType;
        public Material material;
    }

    // This is a list of colours for grid system visuals. They are type sensitive and correspond to materials in the materials folder.
    public enum GridVisualType
    {
        White,
        Blue,
        Red,
        RedSoft,
        Yellow
    }

    [SerializeField] private Transform gridSystemVisualSinglePrefab;
    [SerializeField] private List<GridVisualTypeMaterial> gridVisualTypeMaterialList;
    private GridSystemVisualSingle[,,] gridSystemVisualSingleArray;
    public static GridSystemVisual Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one GridSystemVisual! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// On start, instantiate the visual grid system. The shape and size of the visual grid is determined by the fields set for
    /// the height, width, and the floor amount in the Level Grid.
    /// This method also subscribes other methods containted in this class to events in the logic so that the visual grid system
    /// is kept up to date.
    /// </summary>
    private void Start()
    {
        gridSystemVisualSingleArray = new GridSystemVisualSingle[LevelGrid.Instance.GetWidth(), LevelGrid.Instance.GetWidth(), LevelGrid.Instance.GetFloorAmount()];
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetWidth(); z++)
            {
                for (int altitude = 0; altitude < LevelGrid.Instance.GetFloorAmount(); altitude++)
                {
                    GridPosition gridPosition = new GridPosition(x, z, altitude);
                    Transform gridPositionVisualTransform = 
                        GameObject.Instantiate(gridSystemVisualSinglePrefab, LevelGrid.Instance.GetWorldPosition(gridPosition), Quaternion.identity);

                    gridSystemVisualSingleArray[x, z, altitude] = gridPositionVisualTransform.GetComponent<GridSystemVisualSingle>();
                }
            }
        }

        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged; 
        LevelGrid.Instance.OnAnyUnitMovedGridPosition += LevelGrid_OnAnyUnitMovedGridPosition;
        Unit.OnAnyUnitDead += Unit_OnAnyUnitDied;
        Unit.OnAnyUnitSpawned += Unit_OnAnyUnitSpawned;        
    }

    /// <summary>
    /// Itterate through the grid system and hide all the grid system visuals.
    /// </summary>
    public void HideAllGridPosition()
    {
        foreach (GridSystemVisualSingle gridPositionVisual in gridSystemVisualSingleArray)
        {
            gridPositionVisual.Hide();
        }
    }

    /// <summary>
    /// Given a grid position and a range around the grid position, this method is called to show a radius of grid system visuals
    /// around a grid position in a colour passed in the gridVisualType parameter.
    /// This is often used to show a range around the unit for an action but not necessarily the valid tiles.
    /// </summary>
    /// <param name="gridPosition">The epicenter</param>
    /// <param name="range">The size of the radius around the epicenter</param>
    /// <param name="gridVisualType">A gridVisualType containing a colour for the visuals</param>
    public void ShowGridPositionRange(GridPosition gridPosition, int range, GridVisualType gridVisualType)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();
        for (int x = -range; x <= range; x++ )
        {
            for (int z = -range; z <= range; z++)
            {
                GridPosition testGridPosition = gridPosition + new GridPosition(x, z, 0);

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                int pathfindingDistanceMultiplayer = 10;
                if (Pathfinding.Instance.GetPathLengthIgnoreTerrain(gridPosition, testGridPosition) > range * pathfindingDistanceMultiplayer)
                {
                    continue;
                }

                gridPositionList.Add(testGridPosition);
            }
        }
        ShowGridPositionList(gridPositionList, gridVisualType);
    }

    /// <summary>
    /// Given a list of positions, itterate over each grid position, find the grid system visual single on the grid position, and show
    /// the grid system visual single in the colour contained in the gridVisualType.
    /// </summary>
    /// <param name="gridPositionList">A list of GridPositions of GridSystemVisualSingle's to highlight</param>
    /// <param name="gridVisualType">An object containing the colour to highlight singles in.</param>
    public void ShowGridPositionList(List<GridPosition> gridPositionList, GridVisualType gridVisualType)
    {
        foreach (GridPosition gridPosition in gridPositionList)
        {
            gridSystemVisualSingleArray[gridPosition.x, gridPosition.z, gridPosition.floor].Show(GetGridVisualTypeMaterial(gridVisualType));
        }
    }

    /// <summary>
    /// This method uses the selected unit and selected action in the UnitActionSystem to update the
    /// player view. It uses a switch statement on the selected action to determine how to update the
    /// visuals of the grid system.
    /// </summary>
    private void UpdateGridVisual()
    {
        HideAllGridPosition();
    
        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();

        GridVisualType gridVisualType;

        switch (selectedAction)
        { 
            default:
            case MoveAction moveAction:
                gridVisualType = GridVisualType.White;
                break;
            case SpinAction spinAction:
                gridVisualType = GridVisualType.Blue;
                break;
            case ShootAction shootAction:
                gridVisualType = GridVisualType.Red;

                ShowGridPositionRange(selectedUnit.GetGridPosition(), shootAction.GetRange(), GridVisualType.Yellow);
                break;
            case SwordAction swordAction:
                gridVisualType = GridVisualType.Red;

                ShowGridPositionRange(selectedUnit.GetGridPosition(), swordAction.GetRange(), GridVisualType.Yellow);
                break;
        }
        ShowGridPositionList(selectedAction.GetValidActionGridPositionList(), gridVisualType);
    }

    /// <summary>
    /// This method is fired when the selected unit is changed in the UnitActionSystem in order to keep
    /// the view updated.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UnitActionSystem_OnSelectedActionChanged(object sender, EventArgs e)
    {
        UpdateGridVisual();
    }

    /// <summary>
    /// This method is called when a unit has changed position on the Levelgrid in order to keep
    /// the view updated.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LevelGrid_OnAnyUnitMovedGridPosition(object sender, EventArgs e)
    {
        UpdateGridVisual();
    }

    /// <summary>
    /// This method is called when any unit on the board has died in order to keep
    /// the view updated and consistent.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Unit_OnAnyUnitDied(object sender, EventArgs e)
    {
        UpdateGridVisual();
    }

    /// <summary>
    /// This method is called when any unit is spawned in order to keep the view updated and consistent.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Unit_OnAnyUnitSpawned(object sender, EventArgs e)
    {
        UpdateGridVisual();
    }


    private Material GetGridVisualTypeMaterial(GridVisualType gridVisualType)
    {
        foreach (GridVisualTypeMaterial gridVisualTypeMaterial in gridVisualTypeMaterialList)
        {
            if (gridVisualTypeMaterial.gridVisualType == gridVisualType)
            {
                return gridVisualTypeMaterial.material;
            }
        }
        Debug.LogError("Missing Material for GridVisualType" + gridVisualType);
        return null;
    }
}
