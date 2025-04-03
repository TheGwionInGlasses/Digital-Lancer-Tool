using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class for handling the logic of the shoot action. Any agent with this component attached can perform a shoot action.
/// </summary>
public class ShootAction : BaseAction
{
    public static event EventHandler OnAnyShoot;
    public event EventHandler<OnShootEventArgs> OnShoot;

    public class OnShootEventArgs : EventArgs
    {
        public Unit targetUnit;
        public Unit shootingUnit;
    }

    private enum State
    {
        Aiming,
        Shooting,
        Cooloff,
    }

    private State state;
    [SerializeField] private int attackRange = 8;
    [SerializeField] private int attackDamage = 40;
    [SerializeField] private LayerMask obstaclesLayerMask;
    
    private float stateTimer;
    private Unit targetUnit;
    private bool canShootBullet;

    /// <summary>
    /// Check firstly that the move is active. If the move isn't active, do nothing. Otherwise, begin executing this simple three stage shoot process.
    /// </summary>
    public void Update()
    {
        if (!isActive)
        {
            return;
        }
        
        // Until the state times has run down...
        stateTimer -= Time.deltaTime;
        switch (state)
        {
            case State.Aiming:
                // If the agent is aiming, rotate the agent to the target.
                Vector3 targetDirection = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;

                float rotateSpeed = 10f;
                transform.forward = Vector3.Slerp(transform.forward, targetDirection, Time.deltaTime * rotateSpeed);
                break;
            case State.Shooting:
                // If the agent is shooting, call the shoot function and set the canShootBullet to false so it doesn't make a hose a gajillion projectiles at the target.
                if (canShootBullet)
                {
                    Shoot();
                    canShootBullet = false;
                }
                break;
            case State.Cooloff:
                break;
        }

        // Once the state timer has run out, transition to the next state.
        if (stateTimer <= 0f)
        {
            NextState();
        }
    }

    /// <summary>
    /// This function is called to transition to the next state. The state timer can be changed for each phase in this function.
    /// </summary>
    private void NextState()
    {
        switch (state)
        {
            case State.Aiming:
                state = State.Shooting;
                float shootingStateTime = 0.1f;
                stateTimer = shootingStateTime;
                break;
            case State.Shooting:
                state = State.Cooloff;
                float coolOffStateTime = 0.5f;
                stateTimer = coolOffStateTime;
                break;
            case State.Cooloff:
                ActionComplete();
                break;
        }
    }

    /// <summary>
    /// This function contains the logic for shooting the target. Here the target agent is damaged. It's a very simple function
    /// however the unit animator and screen shake managed both receive an event from this so that the player view is updated.
    /// </summary>
    private void Shoot()
    {
        string logText = unit + " shot at " + targetUnit + ".\n";

        UpdateLog(logText);
        OnShoot?.Invoke(this, new OnShootEventArgs
        {
            targetUnit = targetUnit,
            shootingUnit = unit
        });

        OnAnyShoot?.Invoke(this, EventArgs.Empty);
        targetUnit.Damage(attackDamage);
    } 
    
    /// <summary>
    /// I do wonder.
    /// </summary>
    /// <returns>Not a clue</returns>
    public override string GetActionName()
    {
        return "Shoot";
    }

    /// <summary>
    /// Uses a helper function to return a list of all the valid grid positions for good shooting.
    /// </summary>
    /// <returns>A List of Grid Positions containing shoot worthy targets if any.</returns>
    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = unit.GetGridPosition();
        return GetValidActionGridPositionList(unitGridPosition);
    }

    /// <summary>
    /// Given the position of the agent, return a list of GridPositions containing target the agent can shoot.
    /// </summary>
    /// <param name="gridPosition">The position of the shooting agent</param>
    /// <returns>A List of Grid Positions containing shoot worthy targets if any.</returns>
    private List<GridPosition> GetValidActionGridPositionList(GridPosition gridPosition)
    {
        List<GridPosition> ValidGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = unit.GetGridPosition();
        List<CubeGridPosition> offsetCubeGridPositions = new List<CubeGridPosition>();
        
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
            GridPosition offsetGridPosition = LevelGrid.Instance.CubeToOffset(offsetCubeGridPosition);
            GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

            // If the test grid position is not in bounds, ignore it.
            if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
            {
                continue;
            }

            // If the position is empty, it's not a valid shooting position
            if (!LevelGrid.Instance.HasAnyAgentOnGridPosition(testGridPosition))
            {
                continue;
            }

            // No friendly fire!
            Unit targetUnit = LevelGrid.Instance.GetAgentAtGridPosition(testGridPosition);
            if (targetUnit.IsEnemy() == unit.IsEnemy())
            {
                continue;
            }

            // Check if there's a clear line of site from the agent to the target.
            Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitGridPosition);
            Vector3 shootDir = targetUnit.GetWorldPosition() - unitWorldPosition.normalized;
            float unitShoulderHeight = 1.7f;
            if (Physics.Raycast(
                    unitWorldPosition + Vector3.up * unitShoulderHeight,
                    shootDir,
                    Vector3.Distance(unitWorldPosition, targetUnit.GetWorldPosition()),
                    obstaclesLayerMask
            ))
            {   
                //Blocked by obstacle
                continue;
            }

            ValidGridPositionList.Add(testGridPosition);
        }
        
        return ValidGridPositionList;
    }

    /// <summary>
    /// This function is called at the start of the shoot action. It starts the aiming state timer at 1 second and begins the three phase
    /// shooting action.
    /// </summary>
    /// <param name="gridPosition">The position of the agent being shot</param>
    /// <param name="onActionComplete">The function signature to call upon the actions completion</param>
    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetUnit = LevelGrid.Instance.GetAgentAtGridPosition(gridPosition);

        state = State.Aiming;
        float aimingStateTime = 1f;
        stateTimer = aimingStateTime;

        canShootBullet = true;

        ActionStart(onActionComplete);
    }

    /// <summary>
    /// This function can be modified to change the cost of this aciton.
    /// </summary>
    /// <returns>The cost of the action of the action point economy as an int</returns>
    public override int GetActionPointsCost()
    {
        return 1;
    }

    /// <summary>
    /// Returns the attack range of the shoot action. Useful for the frontend to know the radius of tiles to highlight
    /// </summary>
    /// <returns></returns>
    public int GetRange()
    {
        return attackRange;
    }

    /// <summary>
    /// A simple scoring method. It's a function used by the AI to score the value of shooting at a given target.
    /// It value shooting an already injured target than shooting at an uninjured one. Cruel, I know. But there's
    /// no Geneva Convention in Lancer.
    /// </summary>
    /// <param name="gridPosition">The target grid position containing a possible enemy agent</param>
    /// <returns>An EnemyAIAction containging the grid position of prey and a score on how useful it would be to shoot at said prey</returns>
    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        Unit targetUnit = LevelGrid.Instance.GetAgentAtGridPosition(gridPosition);
        targetUnit.GetHealthNormalized();
        return new EnemyAIAction{
            gridPosition = gridPosition,
            actionValue = 100 + Mathf.RoundToInt((1 - targetUnit.GetHealthNormalized()) * 100f),
        };
    }

    /// <summary>
    /// This method returns a count of all the targets that can be shot from a position. This is used
    /// by the AI to determine if a target position would be helpful to relocate to. This generally
    /// creates AI that lack self preservation. It also makes the Shoot Action and the Move Action more
    /// coupled than I'd prefer.
    /// </summary>
    /// <param name="gridPosition"></param>
    /// <returns></returns>
    public int GetTargetCountAtPosition(GridPosition gridPosition)
    {
        return GetValidActionGridPositionList(gridPosition).Count;
    }

    /// <summary>
    /// This returns the agent being lined up for a gool ol' shooting.
    /// </summary>
    /// <returns>Returns the target agent for this agent's shoot action if there is one.</returns>
    public Unit GetTargetUnit()
    {
        return targetUnit;
    }
}
