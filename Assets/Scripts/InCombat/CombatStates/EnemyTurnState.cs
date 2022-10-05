using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//State is active during enemy turn, and inactive during plaayer turn
public class EnemyTurnState : GameState
{
    private EnemyTurnProcess enemyTurnProcess;
    public override void Init(GameState parentState, StateHandler stateHandler)
    {
        base.Init(parentState, stateHandler);
        this.stateEnum = StateHandler.State.EnemyTurnState;
        this.enemyTurnProcess = new EnemyTurnProcess(this);
    }

    public override void SetStateActive()
    {
        enemyTurnProcess.ProcessTurn();   
    }

    public override void SetStateInactive()
    {
    }

    public void EndTurn()
    {
        Debug.Log("Player turn!");
        this.ChangeState(StateHandler.State.PlayerTurnState);
    }
}
