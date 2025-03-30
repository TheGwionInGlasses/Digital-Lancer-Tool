using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is an abstract class that is used as an interface for actions to ensure consistent behaviour for different cases.
/// </summary>
public abstract class BaseAction : MonoBehaviour
{
    public static event EventHandler OnAnyActionStarted;
    public static event EventHandler OnAnyActionCompleted;
    
    protected bool isActive;
    protected Unit unit;
    protected Action onActionComplete;
    protected EventHandler actionLogUpdater;

    protected virtual void Awake()
    {
        unit = GetComponent<Unit>();
    }

    /// <summary>
    /// Returns a name for the action
    /// </summary>
    /// <returns>The name of the action as a string</returns>
    public abstract string GetActionName();

    /// <summary>
    /// This method contains the execution logic for the action
    /// </summary>
    /// <param name="gridPosition">Target grid position</param>
    /// <param name="onActionComplete">A function signature to run on exection of this function</param>
    public abstract void TakeAction(GridPosition gridPosition, Action onActionComplete);

    /// <summary>
    /// A method to check if the action can be performed on a specific gridPosition
    /// </summary>
    /// <param name="gridPosition">The target gridposition</param>
    /// <returns>Returns true if the action can be executed with the gridposition as the target</returns>
    public bool IsValidActionGridPosition(GridPosition gridPosition)
    {
        List<GridPosition> gridPositions = GetValidActionGridPositionList();
        return gridPositions.Contains(gridPosition);
    }

    /// <summary>
    /// A method that contains for all actions the logic for returning a list of valid grid positions
    /// </summary>
    /// <returns></returns>
    public abstract List<GridPosition> GetValidActionGridPositionList();

    /// <summary>
    /// A method for getting the cost of an aciton. Not all actions cost points.
    /// </summary>
    /// <returns></returns>
    public virtual int GetActionPointsCost()
    {
        return 1;
    }

    /// <summary>
    /// A method to call at the start of the action. It is a subscribable method which is usefull for notifying other components when an action has started.
    /// </summary>
    /// <param name="onActionComplete">A function signature to run when the action has been completed</param>
    protected void ActionStart(Action onActionComplete)
    {
        isActive = true;
        this.onActionComplete = onActionComplete;

        OnAnyActionStarted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// This method is called at the end of an action. It executes the onActionComplete() function and fires an event to notify subscribers of the actions completion.
    /// </summary>
    protected void ActionComplete()
    {
        isActive = false;
        onActionComplete();

        OnAnyActionCompleted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// This method is called to find the best action an AI can take. The logic for scoring the EnemyAIAction is specific to the child class.
    /// </summary>
    /// <returns>Returns the best EnemyAIAction containing the highest score and a target position.</returns>
    public EnemyAIAction GetBestEnemyAIAction()
    {
        List<EnemyAIAction> enemyAIActionList = new List<EnemyAIAction>();
        List<GridPosition> validActionGridPositionList = GetValidActionGridPositionList();

        foreach (GridPosition gridPosition in validActionGridPositionList)
        {
            EnemyAIAction enemyAIAction = GetEnemyAIAction(gridPosition);
            enemyAIActionList.Add(enemyAIAction);
        }

        if (enemyAIActionList.Count > 0)
        {
            enemyAIActionList.Sort((EnemyAIAction a, EnemyAIAction b) => b.actionValue - a.actionValue);
            return enemyAIActionList[0];
        } else
        {
            return null;
        }
    }

    /// <summary>
    /// This method is used to score the value of an action on a given position for the AI manager.
    /// </summary>
    /// <param name="gridPosition">The target position</param>
    /// <returns>Returns an EnemyAIAction containing a score for executing this action on the target position.</returns>
    public abstract EnemyAIAction GetEnemyAIAction(GridPosition gridPosition);

    /// <summary>
    /// A method for getting the agent executing the action.
    /// </summary>
    /// <returns>The origin agent</returns>
    public Unit GetUnit()
    {
        return unit;
    }
}
