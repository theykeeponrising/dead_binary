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
    public enum CombatSubState = {
        PlayerTurnState,
        EnemyTurnState,
    }

    public override void Init(GameState parentState) 
    {
        base.Init(parentState);
        this.substates = new List<GameState> {
            new PlayerTurnState(),
            new EnemyTurnState(),
        };

        foreach (GameState gameState in substates) {
            gameState.Init(this);
        }
    }

    //Get the substate object from the CombatSubState enum type
    public override GameState GetStateObject<CombatSubState>(CombatSubState stateName)
    {
        foreach (GameState state in substates)
        {
            switch(stateName)
            {
                case CombatSubState.PlayerTurnState:
                    if (state is typeof(PlayerTurnState)) return state;
                    break;
                case CombatSubState.EnemyTurnState:
                    if (state is typeof(EnemyTurnState)) return state; 
                default:
                    break;
            }
        }
    }

    void SetClickHandler(InCombatPlayerAction inCombatPlayerAction)
    {
        this.inCombatPlayerAction = inCombatPlayerAction;
    }

    // Update is called once per frame
    public override bool HandleKeyPress()
    {
        // True/False if any key is pressed while in state
        return (inCombatPlayerAction.playerInput.Controls.AnyKey.ReadValue<float>() > 0.5f);
    }

    public override void SetStateActive()
    {
        inCombatPlayerAction.EnablePlayerInput();
        inCombatPlayerAction.playerInput.Controls.InputMenu.performed += _ => stateHandler.ChangeState(StateHandler.State.StatusMenuState);
    }

    public override void SetStateInactive()
    {
        inCombatPlayerAction.DisablePlayerInput();
        inCombatPlayerAction.playerInput.Controls.InputMenu.performed += _ => stateHandler.ChangeState(StateHandler.State.StatusMenuState);
    }
}
