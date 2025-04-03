using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This action contains the logic for agent movement
/// </summary>
public class MoveAction : BaseAction
{
    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private int movementRange = 4;

    // Initially empty. This field stores the positions the agent should walk to in a path to its destination.
    private List<Vector3> positionList;
    private int currentPositionIndex;
    private bool isChangingFloors;


    private float differentFloorsTeleportTimer;
    private float differentFloorsTeleportTimerMax = .5f;

    /// <summary>
    /// First check if the agent is active. If the agent is not active, wait.
    /// If the agent is active, begin moving towards the target position.
    /// </summary>
    void Update()
    {
        if (!isActive)
        {
            return;
        }

        // Initially target self.
        Vector3 targetPosition = positionList[currentPositionIndex];

        if (isChangingFloors)
        {
            // If we're changing floors, stop on position and teleport to position on next floor and then rest is changing condition.
            differentFloorsTeleportTimer -= Time.deltaTime;
            if (differentFloorsTeleportTimer < 0f)
            {
                isChangingFloors = false;
                transform.position = targetPosition;
            }
        } else
        {
            // Otherwise walk to the next position. The target position will usually be a neighbouring tile from the current position on a path to the final destination.
            Vector3 moveDirection = (targetPosition - transform.position).normalized;

            float rotateSpeed = 10f;
            transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);

            transform.position += moveDirection * Time.deltaTime * moveSpeed;
        }

        // When we have arrived at the target position, check that the target position is the final destination. If so, stop moving.
        float stoppingDistance = .1f;
        if (Vector3.Distance(targetPosition, transform.position) < stoppingDistance)
        {
            currentPositionIndex++;

            if (currentPositionIndex >= positionList.Count)
            {
                OnStopMoving?.Invoke(this, EventArgs.Empty);

                ActionComplete();
            } else
            {
                // If the target position was not the destination, get the next target position and the current world position and resume travelling.
                targetPosition = positionList[currentPositionIndex];
                GridPosition targetGridPosition = LevelGrid.Instance.GetGridPosition(targetPosition);
                GridPosition unitGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);

                if (targetGridPosition.floor != unitGridPosition.floor)
                {
                    // If the target position is on a different floor, then the next move will be to a tile on a different floor.
                    isChangingFloors = true;
                }
            }
        }
    }

    /// <summary>
    /// This method is called on taking the movement action. It uses the Pathfinding script to obtain a path between the agent's current position
    /// and the target position. The path is given as a list of GridPositions which must be turned into world position and stored in
    /// the positionList field.
    /// </summary>
    /// <param name="gridPosition">The target grid position, where we are trying to move.</param>
    /// <param name="onActionComplete">The function signature of the function to call when the code is completed.</param>
    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        List<GridPosition> pathGridPositionList = Pathfinding.Instance.FindPath(unit.GetGridPosition(), gridPosition, out int pathLength);

        currentPositionIndex = 0;
        positionList = new List<Vector3>();
        foreach (GridPosition pathGridPosition in pathGridPositionList)
        {
            positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPosition));
        }
        ActionStart(onActionComplete);

        OnStartMoving?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// This method handles the logic for deciding the valid grid positions for the agent to move to.
    /// </summary>
    /// <returns>A list of grid positions the agent is allowed to move to</returns>
    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> ValidGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = unit.GetGridPosition();
        CubeGridPosition unitCubeGridPosition = LevelGrid.Instance.OffsetToCube(unitGridPosition);

        List<CubeGridPosition> offsetCubeGridPositions = new List<CubeGridPosition>();
        for (int altitude = -movementRange; altitude <= movementRange; altitude++)
        {
            for (int q = -movementRange; q <= movementRange; q++)
            {
                for (int r = Mathf.Max(-movementRange, -q-movementRange); r <= Mathf.Min(movementRange, -q+movementRange); r++)
                {
                    CubeGridPosition offsetCubeGridPosition = new CubeGridPosition(q, r, altitude);
                    offsetCubeGridPositions.Add(offsetCubeGridPosition);
                }
            }
        }
        foreach (CubeGridPosition offsetCubeGridPosition in offsetCubeGridPositions)
        {
            CubeGridPosition testCubePosition = offsetCubeGridPosition + unitCubeGridPosition;
            GridPosition testGridPosition = LevelGrid.Instance.CubeToOffset(testCubePosition);

            if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
            {
                continue;
            }

            if (unitGridPosition == testGridPosition)
            {
                continue;
            }

            if (LevelGrid.Instance.HasAnyAgentOnGridPosition(testGridPosition))
            {
                continue;
            }

            if (!Pathfinding.Instance.IsWalkableGridPosition(testGridPosition))
            {
                continue;
            }

            if (!Pathfinding.Instance.HasPath(unitGridPosition, testGridPosition))
            {
                continue;
            }

            int pathfindingDistanceMultiplayer = 10;
            if (Pathfinding.Instance.GetPathLength(unitGridPosition, testGridPosition) > movementRange * pathfindingDistanceMultiplayer)
            {
                continue;
            }

            ValidGridPositionList.Add(testGridPosition);
        }
        
        return ValidGridPositionList;
    }

    /// <summary>
    /// Returns the actions name for the UI and log.
    /// </summary>
    /// <returns>String</returns>
    public override string GetActionName()
    {
        return "Move";
    }

    /// <summary>
    /// This method is used to create an EnemyAIAction that represents the value of doing this action on the passed
    /// grid position. Currently it is very basic. This method will try to push the AI into positions in which
    /// it can make the most attacks with its basic shoot action.
    /// </summary>
    /// <param name="gridPosition">A target grid position</param>
    /// <returns>An EenemyAIAction used to evaluate the value of moving to the target position.</returns>
    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        int targetCountAtGridPosition = unit.GetAction<ShootAction>().GetTargetCountAtPosition(gridPosition);
        return new EnemyAIAction{
            gridPosition = gridPosition,
            actionValue = targetCountAtGridPosition * 10,
        };
    }
}
