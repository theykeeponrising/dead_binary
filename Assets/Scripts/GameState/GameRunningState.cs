using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//State is active during player turn, and inactive during enemy turn
public class GameRunningState : GameState
{
    public override void Init(GameState parentState, StateHandler stateHandler)
    {
        base.Init(parentState, stateHandler);
        this.stateEnum = StateHandler.State.GameRunningState;
        this.substates = new List<GameState> {
            new CombatState(),
        };

        foreach (GameState gameState in substates) {
            gameState.Init(this, stateHandler);
        }   
    }

    public override void SetStateActive()
    {
        CombatState combatState = (CombatState) this.GetSubStateObject(StateHandler.State.CombatState);
        combatState.SetStateActive();
        this.activeSubState = combatState;
    }

    public override void SetStateInactive()
    {
        Application.Quit();
    }
}
