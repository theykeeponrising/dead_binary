using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//State is active during player turn, and inactive during enemy turn
public class EnemyTurnState : GameState
{
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
