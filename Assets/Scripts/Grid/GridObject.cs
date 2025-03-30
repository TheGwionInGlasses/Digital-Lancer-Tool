using System.Collections.Generic;

/// <summary>
/// This class represents a single grid position on the hexagonal grid. It tracks its own coordinates as well as any
/// agents positioned over this grid object.
/// </summary>
public class GridObject
{
    private GridSystemHex<GridObject> gridSystem;
    private GridPosition gridPosition;
    private CubeGridPosition axialGridPosition;
    private List<Unit> unitList;


    public GridObject(GridSystemHex<GridObject> gridSystem, GridPosition gridPosition, CubeGridPosition axialGridPosition)
    {
        this.gridPosition = gridPosition;
        this.axialGridPosition = axialGridPosition;
        this.gridSystem = gridSystem;
        unitList = new List<Unit>();
    }

    public void AddUnit(Unit unit)
    {
        unitList.Add(unit);
    }

    public List<Unit> GetUnitList()
    {
        return unitList;
    }

    public void RemoveUnit(Unit unit)
    {
        unitList.Remove(unit);
    }

    public override string ToString()
    {
        string unitString = "";
        foreach (Unit unit in unitList)
        {
            unitString += unit + "\n";
        }
        return gridPosition.ToString() + "\n" + axialGridPosition.ToString() + "\n" + unitString;
    }

    public bool HasAnyUnit()
    {
        return unitList.Count > 0;
    }

    public Unit GetUnit()
    {
        if (HasAnyUnit())
        {
            return unitList[0];
        }
        return null;
    }
}
