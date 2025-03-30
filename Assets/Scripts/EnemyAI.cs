using System;
using UnityEngine;

/// <summary>
/// This class <c>EnemyAI</c> provides some primitive logic for handling actions on the enemy's turn.
/// This is attached to a variant of the agent prefab.
/// </summary>
public class EnemyAI : MonoBehaviour
{

    private enum State
    {
        WaitingForEnemyTurn,
        TakingTurn,
        Busy
    }

    private State state;
    private float timer;

    /// <summary>
    /// On creation of the object, set the AI's status to waiting for its turn.
    /// </summary>
    private void Awake()
    {
        state = State.WaitingForEnemyTurn;
    }

    /// <summary>
    /// On scene start, subscribe this component to when the turn changes.
    /// </summary>
    private void Start()
    {
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
    }

    /// <summary>
    /// Whilst the game is running, continually check if it is not the players' turn. Do nothing whilst it is the player's turn,
    /// otherise execute its own actions.
    /// </summary>
    void Update()
    {
        if (TurnSystem.Instance.IsPlayerTurn())
        {
            return;
        }

        switch (state)
        {
            case State.WaitingForEnemyTurn:
                // Do nothing whilst it is waiting for its turn.
                break;
            case State.TakingTurn:
                // If it is the AI's turn, try to perform an action.
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {   
                    // If an AI agent is able to take an action. Set the state to busy otherwise end its turn.
                    if (TryTakeEnemyAIAction(SetStateTakingTurn))
                    {
                        state = State.Busy;
                    } else
                    {
                        TurnSystem.Instance.NextTurn();
                    }
                }
                break;
            case State.Busy:
                // Whilst the AI agent is performing an action, don't change anything..
                break;
        }
    }

    /// <summary>
    /// This methos is subscribed to when the player ends their turn. It resets the AI turn timer to two seconds and puts the AI agent in play.
    /// </summary>
    /// <param name="sender">The TurnSystem singleton</param>
    /// <param name="e">Any arguments sent alongside the event.</param>
    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if (!TurnSystem.Instance.IsPlayerTurn())
        {
            state = State.TakingTurn;
            timer = 2f;
        }
        
    }
    
    /// <summary>
    /// This resets the times to give the AI time to take additional actions upon completion of one of its actions.
    /// </summary>
    private void SetStateTakingTurn()
    {
        timer = 0.5f;
        state = State.TakingTurn;
    }

    /// <summary>
    /// This method runs through the list of all the agents managed by the AI to see if a move is possible.
    /// </summary>
    /// <param name="onEnemyAIActionComplete">A function signature passed as an argument to run on completion of an AI action</param>
    /// <returns>Returns true if an action was taken, returns false otherwise.</returns>
    private bool TryTakeEnemyAIAction(Action onEnemyAIActionComplete)
    {
        foreach (Unit enemyUnit in UnitManager.Instance.GetEnemyUnitList())
        {
            if (TryTakeEnemyAIAction(enemyUnit, onEnemyAIActionComplete))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// This method takes an AI managed agent and attempts to find the best action to take out of all possible actions.
    /// </summary>
    /// <param name="enemyUnit">A reference to the AI managed agent taking an action</param>
    /// <param name="onEnemyAIActionComplete">A function signature to call on completion of the action</param>
    /// <returns>Returns true if an action was taken, false otherwise.</returns>
    private bool TryTakeEnemyAIAction(Unit enemyUnit, Action onEnemyAIActionComplete)
    {
        EnemyAIAction bestEnemyAIAction = null;
        BaseAction bestBaseAction = null;

        // For every possible action the agent can take, check if it would be better than the currently cached best action, if so,
        // Set the cached best action to the new best.
        foreach (BaseAction baseAction in enemyUnit.GetBaseActionArray())
        {
            // Check if the AI managed agent has enough in its action economy to perform the aciton
            if (!enemyUnit.CanSpendActionPointsToTakeAction(baseAction))
            {
                continue;
            }

            // If there is currently no best action, set the first action it can take as the best.
            if (bestEnemyAIAction == null)
            {
                bestEnemyAIAction = baseAction.GetBestEnemyAIAction();
                bestBaseAction = baseAction;
            } else
            // Otherwise compare this current action against the best.
            {
                EnemyAIAction testEnemyAIAction = baseAction.GetBestEnemyAIAction();
                if (testEnemyAIAction != null && testEnemyAIAction.actionValue > bestEnemyAIAction.actionValue)
                {
                    bestEnemyAIAction = testEnemyAIAction;
                    bestBaseAction = baseAction;
                }
            }
        }

        // If an action is possible, take the best.
        if(bestEnemyAIAction != null && enemyUnit.TrySpendActionPointsToTakeAction(bestBaseAction))
        {
            bestBaseAction.TakeAction(bestEnemyAIAction.gridPosition, onEnemyAIActionComplete);
            return true;
        }
        else
        {
            return false;
        }
    }
}
