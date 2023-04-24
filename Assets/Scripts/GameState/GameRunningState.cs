using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//State is active during player turn, and inactive during enemy turn
public class GameRunningState : GameState
{
    public StateHandler.State initialState = StateHandler.State.StartMenuState;

    public override void Init(GameState parentState, StateHandler stateHandler)
    {
        base.Init(parentState, stateHandler);
        this.stateEnum = StateHandler.State.GameRunningState;
        this.substates = new List<GameState> {
            new StartMenuState(),
            new CombatState(),
        };

        foreach (GameState gameState in substates) {
            gameState.Init(this, stateHandler);
        }   
    }

    public void SetInitialState(StateHandler.State state)
    {
        initialState = state;
    }

    public override void SetStateActive()
    {
        CombatState combatState = (CombatState) this.GetSubStateObject(StateHandler.State.CombatState);
        StartMenuState startMenuState = (StartMenuState) this.GetSubStateObject(StateHandler.State.StartMenuState);

        GameState initialStateObject = StateHandler.Instance.GetStateObject(initialState);
        initialStateObject.SetStateActive();
        this.activeSubState = initialStateObject;
    }

    public override void SetStateInactive()
    {
        Application.Quit();
    }
}
