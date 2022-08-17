using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//Combat State is the basic gameplay, i.e. player vs robot battles. This class will handle inputs at a high level
public class CombatState : GameState
{
    // Used to manage user inputs
    public InCombatPlayerAction inCombatPlayerAction;

    public override void Init(GameState parentState, StateHandler stateHandler) 
    {
        base.Init(parentState, stateHandler);
        this.stateEnum = StateHandler.State.CombatState;
        this.substates = new List<GameState> {
            new PlayerTurnState(),
            new EnemyTurnState(),
        };

        foreach (GameState gameState in substates) {
            gameState.Init(this, stateHandler);
        }
    }

    public override void SetStateActive()
    {
        this.activeSubState = FindSubState(StateHandler.State.PlayerTurnState);
        // inCombatPlayerAction.EnablePlayerInput();
        // inCombatPlayerAction.playerInput.Controls.InputMenu.performed += _ => stateHandler.ChangeState(StateHandler.State.StatusMenuState);
    }

    // public override void SetStateInactive()
    // {
    //     inCombatPlayerAction.DisablePlayerInput();
    //     inCombatPlayerAction.playerInput.Controls.InputMenu.performed += _ => stateHandler.ChangeState(StateHandler.State.StatusMenuState);
    // }
}
