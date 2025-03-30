using System;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// This class <c>PathNode</c> models a node in used for the pathfinding algorithm. It keeps track of its own costs, its location, and
/// the from which node this node was explored from in the algorithm.
/// </summary>
public class PathNode
{
    private GridPosition gridPosition;
    private CubeGridPosition axialGridPosition;
    private int gCost;
    private int hCost;
    private int fCost;
    private PathNode cameFromPathNode;
    private bool isWalkable = true;

    public PathNode(GridPosition gridPosition, CubeGridPosition axialGridPosition)
    {
        this.gridPosition = gridPosition;
        this.axialGridPosition = axialGridPosition;
    }

    public override string ToString()
    {
        return gridPosition.ToString();
    }

    public int GetGCost()
    {
        return gCost;
    }

    public int GetHCost()
    {
        return hCost;
    }

    public int GetFCost()
    {
        return fCost;
    }

    public void SetGCost(int gCost)
    {
        this.gCost = gCost;
    }

    public void SetHCost(int hCost)
    {
        this.hCost = hCost;
    }

    public void CalculateFCost()
    {
        this.fCost = this.gCost + this.hCost;
    }

    public void ResetCameFromPathNode()
    {
        cameFromPathNode = null;
    }

    public void SetCameFromPathNode(PathNode cameFromPathNode)
    {
        this.cameFromPathNode = cameFromPathNode;
    }

    public PathNode GetCameFromPathNode()
    {
        return this.cameFromPathNode;
    }


    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    public bool IsWalkable()
    {
        return isWalkable;
    }

    public void SetIsWalkable(bool isWalkable)
    {
        this.isWalkable = isWalkable;
    }
}
