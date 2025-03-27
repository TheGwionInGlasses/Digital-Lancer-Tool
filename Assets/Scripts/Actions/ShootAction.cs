using System;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private LayerMask obstaclesLayerMask;
    private float stateTimer;
    private Unit targetUnit;
    private bool canShootBullet;

    public void Update()
    {
        if (!isActive)
        {
            return;
        }
        
        stateTimer -= Time.deltaTime;
        switch (state)
        {
            case State.Aiming:
                Vector3 targetDirection = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;

                float rotateSpeed = 10f;
                transform.forward = Vector3.Slerp(transform.forward, targetDirection, Time.deltaTime * rotateSpeed);
                break;
            case State.Shooting:
                if (canShootBullet)
                {
                    Shoot();
                    canShootBullet = false;
                }
                break;
            case State.Cooloff:
                break;
        }

        if (stateTimer <= 0f)
        {
            NextState();
        }

        
    }

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

    private void Shoot()
    {
        OnShoot?.Invoke(this, new OnShootEventArgs
        {
            targetUnit = targetUnit,
            shootingUnit = unit
        });

        OnAnyShoot?.Invoke(this, EventArgs.Empty);
        targetUnit.Damage(40);
    } 
    
    public override string GetActionName()
    {
        return "Shoot";
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = unit.GetGridPosition();
        return GetValidActionGridPositionList(unitGridPosition);
    }

    public List<GridPosition> GetValidActionGridPositionList(GridPosition gridPosition)
    {
        List<GridPosition> ValidGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = unit.GetGridPosition();
        for (int x = -attackRange; x <= attackRange; x++)
        {
            for (int z = -attackRange; z <= attackRange; z++)
            {
                for (int altitude = -attackRange; altitude <- attackRange; altitude++ )
                {
                    GridPosition offsetGridPosition = new GridPosition(x, z, 0);
                    GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                    if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                    {
                        continue;
                    }

                    int pathfindingDistanceMultiplayer = 10;
                    if (Pathfinding.Instance.GetPathLengthIgnoreTerrain(unitGridPosition, testGridPosition) > attackRange * pathfindingDistanceMultiplayer)
                    {
                        continue;
                    }

                    if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                    {
                        continue;
                    }

                    Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);
                    if (targetUnit.IsEnemy() == unit.IsEnemy())
                    {
                        continue;
                    }


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
            }
        }
        
        return ValidGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);

        state = State.Aiming;
        float aimingStateTime = 1f;
        stateTimer = aimingStateTime;

        canShootBullet = true;

        ActionStart(onActionComplete);
    }

    public override int GetActionPointsCost()
    {
        return 1;
    }

    public int GetRange()
    {
        return attackRange;
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        targetUnit.GetHealthNormalized();
        return new EnemyAIAction{
            gridPosition = gridPosition,
            actionValue = 100 + Mathf.RoundToInt((1 - targetUnit.GetHealthNormalized()) * 100f),
        };
    }

    public int GetTargetCountAtPosition(GridPosition gridPosition)
    {
        return GetValidActionGridPositionList(gridPosition).Count;
    }

    public Unit GetTargetUnit()
    {
        return targetUnit;
    }
}
