using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using System.Threading;


public class GridSystemHex<TGridObject>
{
    private const float VERITCAL_OFFSET_MULTIPLIER = 0.75f;
    private const float HORIZONTAL_OFFSET_MULTIPLIER = 0.5f;
    private int width;
    private int height;
    private float cellSize;
    private int floor;
    private float floor_height;
    private TGridObject[,] gridObjectArray;

    public GridSystemHex(int width, int height, float cellSize, int floor, float floor_height, Func<GridSystemHex<TGridObject>, GridPosition, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.floor = floor;
        this.floor_height = floor_height;

        gridObjectArray = new TGridObject[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z, floor);
                gridObjectArray[x,z] = createGridObject(this, gridPosition);
            }
        }
        
    }

    public Vector3 GetWorldPosition(GridPosition gridPosition)
    {
        return 
            new Vector3(gridPosition.x, 0, 0) * cellSize + 
            new Vector3(0,0, gridPosition.z) * cellSize * VERITCAL_OFFSET_MULTIPLIER +
            (((gridPosition.z %2) == 1) ? new Vector3(1,0,0) * cellSize * HORIZONTAL_OFFSET_MULTIPLIER : Vector3.zero) +
            new Vector3(0, floor, 0) * floor_height;
    }

    public GridPosition GetGridPosition(Vector3 worldPosition)
    {
        GridPosition roughXZ = new GridPosition(
            Mathf.RoundToInt(worldPosition.x / cellSize),
            Mathf.RoundToInt(worldPosition.z / cellSize/ VERITCAL_OFFSET_MULTIPLIER),
            floor
        );


        bool oddRow = (roughXZ.z % 2) == 1;
        List<GridPosition> neighbourGridPositionList = new List<GridPosition>
        {
            roughXZ + new GridPosition(-1, 0, 0),
            roughXZ + new GridPosition(+1, 0, 0),

            roughXZ + new GridPosition(0, +1, 0),
            roughXZ + new GridPosition(0, -1, 0),

            roughXZ + new GridPosition(oddRow ? +1 : -1, 1, 0),
            roughXZ + new GridPosition(oddRow ? +1 : -1, 1, 0),
        };

        GridPosition closestGridPosition = roughXZ;
        foreach (GridPosition neighbourGridPosition in neighbourGridPositionList)
        {
            if (Vector3.Distance(worldPosition, GetWorldPosition(neighbourGridPosition)) <
                Vector3.Distance(worldPosition, GetWorldPosition(closestGridPosition)))
            {
                closestGridPosition = neighbourGridPosition;
            }
        }
        return closestGridPosition;
    }


    public void CreateDebugObject(Transform debugPrefab)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z, floor);

                Transform debugTransform = GameObject.Instantiate(debugPrefab, GetWorldPosition(gridPosition), Quaternion.identity);
                GridDebugObject gridDebugObject = debugTransform.GetComponent<GridDebugObject>();
                gridDebugObject.SetGridObject(GetGridObject(gridPosition));
            }
        }
    }
    

    public TGridObject GetGridObject(GridPosition gridPosition)
    {
        return gridObjectArray[gridPosition.x, gridPosition.z];
    }

    public bool IsValidGridPosition(GridPosition gridPosition)
    {
        return gridPosition.x >= 0 && 
            gridPosition.x < width && 
            gridPosition.z >= 0 && 
            gridPosition.z < height &&
            gridPosition.floor == floor;
    }

    public int GetHeight()
    {
        return height;
    }

    public int GetWidth()
    {
        return width;
    }
}
