using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    private enum State
    {
        WaitingTurn,
        WaitingAction,
        Busy
    }

    private State state;

    private float timer;

    private void Awake() {
        state = State.WaitingTurn;
    }

    private void Start() 
    {
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
    }

    private void Update() 
    {
        if (TurnSystem.Instance.IsPlayerTurn()) return;

        switch (state)
        {
            case State.WaitingTurn:
                break;
            case State.WaitingAction:
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    if (TryCommitEnemyAIAction(SetStateToWaitingAction))
                    {
                        state = State.Busy;
                    }
                    else
                    {
                        TurnSystem.Instance.NextTurn();
                    }
                }
                break;
            case State.Busy:
                break;
        }
    }

    private void SetStateToWaitingAction()
    {
        timer = 0.5f;
        state = State.WaitingAction;
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if (!TurnSystem.Instance.IsPlayerTurn())
        {
            state = State.WaitingAction;
            timer = 2f;
        }
    }

    private bool TryCommitEnemyAIAction(Action onEnemyAIActionComplete)
    {
        foreach (Unit enemyUnit in UnitManager.Instance.GetEnemyUnitList())
        {
            if (TryCommitEnemyAIAction(enemyUnit, onEnemyAIActionComplete)) return true;
        }
        return false;
    }

    private bool TryCommitEnemyAIAction(Unit enemyUnit, Action onEnemyAIActionComplete)
    {
        EnemyAIAction bestEnemyAIAction = null;
        BaseAction bestBaseAction = null;
        foreach (BaseAction baseAction in enemyUnit.GetBaseActionArray())
        {
            if (!enemyUnit.IsAbleToSpendActionPoints(baseAction)) continue;
            if (bestEnemyAIAction == null)
            {
                bestEnemyAIAction = baseAction.GetBestEnemyAIAction();
                bestBaseAction = baseAction;
            }
            else
            {
                EnemyAIAction testEnemyAIAction = baseAction.GetBestEnemyAIAction();
                if (testEnemyAIAction != null && testEnemyAIAction.actionValue > bestEnemyAIAction.actionValue)
                {
                    bestEnemyAIAction = baseAction.GetBestEnemyAIAction();
                    bestBaseAction = baseAction;
                }
            }
        }
        if (bestEnemyAIAction == null || !enemyUnit.TrySpendActionPoints(bestBaseAction)) return false;
        bestBaseAction.TakeAction(bestEnemyAIAction.gridPosition, onEnemyAIActionComplete);
        return true;
    }

}
