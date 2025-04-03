using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// This class represents a flat hexagonal grid. It is used to map the space on one floor. Multiple of these objects may be initiliased on the LevelGrid and
/// Pathfinding script depending on the number of floors contained in a scene.
/// The level grid is made up of a two dimensional array of TGridObject generics so that this code can be reused for Pathfinding singleton that uses Pathnodes
/// or the LevelGrid singleton that uses GridObjects.
/// </summary>
/// <typeparam name="TGridObject"> A generic Grid Object. Either a Grid Object or a PathNode. </typeparam>
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

    public GridSystemHex(int width, int height, float cellSize, int floor, float floor_height, Func<GridSystemHex<TGridObject>, GridPosition, CubeGridPosition, TGridObject> createGridObject)
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
                CubeGridPosition cubeGridPosition = OffsetToCube(gridPosition);
                gridObjectArray[x,z] = createGridObject(this, gridPosition, cubeGridPosition);
            }
        }
        
    }

    /// <summary>
    /// This function translates a gridPosition into a Vector3 representing a world position. This is used primarily
    /// to for functions such as agent movement where a Vector3 is used to set a target position to move to.
    /// Both the Veritcal offset and the horizontal offset used here are floats for used to create an accurate Vector3
    /// position from a hex in the hex grid given the not square shape of the grid.
    /// </summary>
    /// <param name="gridPosition"></param>
    /// <returns>A vector3 compliment to the grid position</returns>
    public Vector3 GetWorldPosition(GridPosition gridPosition)
    {
        return 
            new Vector3(gridPosition.x, 0, 0) * cellSize + 
            new Vector3(0,0, gridPosition.z) * cellSize * VERITCAL_OFFSET_MULTIPLIER +
            (((gridPosition.z %2) == 1) ? new Vector3(1,0,0) * cellSize * HORIZONTAL_OFFSET_MULTIPLIER : Vector3.zero) +
            new Vector3(0, floor, 0) * floor_height;
    }

    /// <summary>
    /// This method is called to return the a Vector3 world position as an object. This is used primarily to translate mouse interactions
    /// into grid positions for the logic.
    /// Due to the shape of the hex, translating the world position into a grid position can give the wrong answer. To answer to this problem
    /// is to find all of the rough grid positions neighbours and then turn them into world positions to find the grid position closest to
    /// the worldPosition passed initially.
    /// </summary>
    /// <param name="worldPosition">A Vector3 representing a coordinate in space in the Unity scene</param>
    /// <returns>The closest Grid position to the interaction.</returns>
    public GridPosition GetGridPosition(Vector3 worldPosition)
    {
        GridPosition roughXZ = new GridPosition(
            Mathf.RoundToInt(worldPosition.x / cellSize),
            Mathf.RoundToInt(worldPosition.z / cellSize/ VERITCAL_OFFSET_MULTIPLIER),
            floor
        );

        // Offset to compensate for the right row shift on odd rows in an offset oddrow coordinate hex grid.
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


    /// <summary>
    /// This method instantiates a prefab of a debug object on every position in the grid. It looks a little funky for multi level scenes.
    /// </summary>
    /// <param name="debugPrefab">The prefab to instantiate on every grid position.</param>
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
    
    /// <summary>
    /// This function is used to return the object in the gridposition. Treats the grid position as an index
    /// in the of objects.
    /// </summary>
    /// <param name="gridPosition"></param>
    /// <returns></returns>
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

    public bool IsValidGridPosition(CubeGridPosition cubeGridPosition)
    {
        return IsValidGridPosition(CubeToOffset(cubeGridPosition));
    }

    public int GetHeight()
    {
        return height;
    }

    public int GetWidth()
    {
        return width;
    }

    public GridPosition CubeToOffset(CubeGridPosition cubeGridPosition)
    {
        return new GridPosition(
            cubeGridPosition.q + (cubeGridPosition.r - (cubeGridPosition.r%2)) /2,
            cubeGridPosition.r,
            cubeGridPosition.floor
        );
    }

    public CubeGridPosition OffsetToCube(GridPosition gridPosition)
    {
        return new CubeGridPosition(
            gridPosition.x - (gridPosition.z - (gridPosition.z%2)) / 2, 
            gridPosition.z,
            gridPosition.floor
            );
    }
}
