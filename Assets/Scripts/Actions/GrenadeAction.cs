using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This actions models a component that can be attached to an agent to allow for Area of effect attacks.
/// </summary>
public class GrenadeAction : BaseAction
{
    [SerializeField] private int attackRange = 6;
    [SerializeField] private Transform grenadeProjectilePrefab;
    [SerializeField] private int damage = 30;

    /// <summary>
    /// Returns a name for the action
    /// </summary>
    /// <returns>The name of the action as a string</returns>
    public override string GetActionName()
    {
        return "Grenade";
    }

    /// <summary>
    /// This method contains the execution logic for the action
    /// </summary>
    /// <param name="gridPosition">Target grid position</param>
    /// <param name="onActionComplete">A function signature to run on exection of this function</param>
   public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 0
        };
    }

    /// <summary>
    /// A method containing the logic that returns all of the valid grid positions the agent can throw this grenade.
    /// First checks in a radius around the agent using the attack range.
    /// </summary>
    /// <returns>A list of targetable GridPositions</returns>
    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> ValidGridPositionList = new List<GridPosition>();
        List<CubeGridPosition> offsetCubeGridPositions = new List<CubeGridPosition>();

        GridPosition unitGridPosition = unit.GetGridPosition();
        CubeGridPosition unitCubeGridPosition = LevelGrid.Instance.OffsetToCube(unitGridPosition);

        for (int altitude = -attackRange; altitude <= attackRange; altitude++)
        {
            for (int q = -attackRange; q <= attackRange; q++)
            {
                for (int r = Mathf.Max(-attackRange, -q-attackRange); r <= Mathf.Min(attackRange, -q+attackRange); r++)
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

            // Check if the Grid Position is in the bounds of the level
            if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
            {
                continue;
            }

            // Check the tile is not floating.
            if (!Pathfinding.Instance.IsWalkableGridPosition(testGridPosition))
            {
                continue;
            }

            ValidGridPositionList.Add(testGridPosition);
        }
        
        return ValidGridPositionList;
    }

    /// <summary>
    /// This function is used to instantiate a grenade projectile on the origin agent.
    /// </summary>
    /// <param name="gridPosition">The target position for the grenade action</param>
    /// <param name="onActionComplete">A signature function to call when the action haas been completed.</param>
    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        Transform grenadeProjectileTransform = 
            Instantiate(grenadeProjectilePrefab, unit.GetWorldPosition(), Quaternion.identity);
        GrenadeProjectile grenadeProjectile = grenadeProjectileTransform.GetComponent<GrenadeProjectile>();
        grenadeProjectile.Setup(gridPosition, damage, OnGrenadeBehaviourComplete);

        ActionStart(onActionComplete);
    }

    /// <summary>
    /// Calls the base action ActionComplete() function to clear active status and any further logic.
    /// </summary>
    private void OnGrenadeBehaviourComplete()
    {
        ActionComplete();
    }
}
