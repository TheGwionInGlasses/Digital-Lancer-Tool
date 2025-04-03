using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the sword action. A generic close quaters combat action. Any agent with this attached as a component in unity can perform this action.
/// </summary>
public class SwordAction : BaseAction
{
    public static event EventHandler OnAnySwordHit;

    public event EventHandler OnSwordActionStarted;
    public event EventHandler OnSwordActionCompleted;

    [SerializeField] private int attackRange = 1;
    [SerializeField] private int attackDamage = 100;

    private enum State
    {
        SwingingSwordBeforeHit,
        SwingingSwordAfterHit
    }

    private State state;
    private float stateTimer;
    private Unit targetUnit;

    /// <summary>
    /// Continually check if the sword action is active. If not, do nothing. Once it is active, commit to a two stage sword action.
    /// First the agent will orient itself towards the target. And then the agent will slash.
    /// </summary>
    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        // Begin counting down the state timer
        stateTimer -= Time.deltaTime;
        switch (state)
        {
            case State.SwingingSwordBeforeHit:
                // Rotate towards the target before slashing.
                Vector3 targetDirection = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;

                float rotateSpeed = 10f;
                transform.forward = Vector3.Lerp(transform.forward, targetDirection, Time.deltaTime * rotateSpeed);
                break;
            case State.SwingingSwordAfterHit:
                break;
        }

        // Once the state timer has been rundown, move to the next stage
        if (stateTimer <= 0f)
        {
            NextState();
        }
    }

    /// <summary>
    /// This function is called to handle the logic of moving between stages of the sword action.
    /// </summary>
    private void NextState()
    {
        switch (state)
        {
            case State.SwingingSwordBeforeHit:
                // If we're moving to the next stage from the stage before slashing, damage the target and move to the next stage.
                string logText = targetUnit + " slashed " + unit + "\n";
                UpdateLog(logText);
                state = State.SwingingSwordAfterHit;
                float afterHitStateTime = 0.5f;
                stateTimer = afterHitStateTime;
                targetUnit.Damage(100);
                // Event handler is fired so the agent in the frontend executes a sword slash animation.
                OnAnySwordHit?.Invoke(this, EventArgs.Empty);
                break;
            case State.SwingingSwordAfterHit:
                // There is no state after this state, therefore the action has been completed.
                OnSwordActionCompleted?.Invoke(this, EventArgs.Empty);
                ActionComplete();
                break;
        }
    }

    public override string GetActionName()
    {
        return "Sword";
    }

    /// <summary>
    /// This function scores taking a sword action against an agent for the AI.
    /// This is weighted heavily in favour of taking the sword action as the action deals
    /// significant damage. The logic is simple and doesn't check if the action would
    /// kill the target like the shoot action.
    /// </summary>
    /// <param name="gridPosition">The grid position of a valid target</param>
    /// <returns>An EnemyAIAction containing the target position and a flat score for the value of taking this action.</returns>
    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction {
            gridPosition = gridPosition,
            actionValue = 200
        };
    }

    /// <summary>
    /// This function handles the logic for finding all the valid grid positions to execute the sword slash on.
    /// </summary>
    /// <returns>Returns a list of GridPositions containing enemies that can be sworded.</returns>
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

            // Check if the position is within the bounds of the Level.
            if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
            {
                continue;
            }

            // Check if the grid position contains an agent
            if (!LevelGrid.Instance.HasAnyAgentOnGridPosition(testGridPosition))
            {
                continue;
            }

            // Say no to friendly fire incidents in Lancer
            Unit targetUnit = LevelGrid.Instance.GetAgentAtGridPosition(testGridPosition);
            if (targetUnit.IsEnemy() == unit.IsEnemy())
            {
                continue;
            }

            ValidGridPositionList.Add(testGridPosition);
        }
        
        return ValidGridPositionList;
    }

    /// <summary>
    /// This function is called to start the sword action.
    /// </summary>
    /// <param name="gridPosition">The grid position containing the target of the attack</param>
    /// <param name="onActionComplete">The function signature to call when the action is completed</param>
    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetUnit = LevelGrid.Instance.GetAgentAtGridPosition(gridPosition);
        
        state = State.SwingingSwordBeforeHit;
        float beforeHitStateTime = 0.7f;
        stateTimer = beforeHitStateTime;
        OnSwordActionStarted?.Invoke(this, EventArgs.Empty);
        ActionStart(onActionComplete);
    }

    /// <summary>
    /// Returns a range around the unit. This is usefull for highlighting the range on the frontend but not necessarily the valid positions.
    /// </summary>
    /// <returns>The attack range as an int</returns>
    public int GetRange()
    {
        return attackRange;
    }
}
