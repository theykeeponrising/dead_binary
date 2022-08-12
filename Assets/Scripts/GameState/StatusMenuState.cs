using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using TMPro;

//Menu for exiting the game, may obtain additional functionality later
public class StatusMenuState : GameState
{
    public StatusMenuUI statusMenuUI;

    // Update is called once per frame
    public override bool HandleKeyPress()
    {
        // True/False if any key is pressed while in state
        return statusMenuUI.HandleKeyPress();
    }

    public override void SetStateActive()
    {
        statusMenuUI.EnablePlayerInput();
        statusMenuUI.DisplayMenu(true);
        statusMenuUI.playerInput.Controls.InputMenu.performed += _ => stateHandler.ChangeState(StateHandler.State.CombatState);
    }

    public override void SetStateInactive()
    {
        statusMenuUI.DisablePlayerInput();
        statusMenuUI.DisplayMenu(false);
        statusMenuUI.playerInput.Controls.InputMenu.performed += _ => stateHandler.ChangeState(StateHandler.State.CombatState);
    }
}
