using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//Combat State is the basic gameplay, i.e. player vs robot battles. This class will handle inputs at a high level
public class CombatState : GameState
{
    // Used to manage user inputs
    PlayerInput playerInput;
    public InCombatPlayerAction inCombatPlayerAction;

    void Awake()
    {
        playerInput = new PlayerInput();
    }

    void EnablePlayerInput()
    {
        playerInput.Controls.InputPrimary.performed += _ => inCombatPlayerAction.SelectUnit();
        playerInput.Controls.InputSecondary.performed += _ => inCombatPlayerAction.MoveCharacter();
        //playerInput.Controls.AnyKey.performed += _ => KeyPress(playerInput.Controls.AnyKey); // For if we want any "PRESS ANY KEY" moments
        playerInput.Controls.ActionButton_1.performed += _ => KeyPress(playerInput.Controls, playerInput.Controls.ActionButton_1);
        playerInput.Controls.ActionButton_2.performed += _ => KeyPress(playerInput.Controls, playerInput.Controls.ActionButton_2);
        playerInput.Controls.ActionButton_3.performed += _ => KeyPress(playerInput.Controls, playerInput.Controls.ActionButton_3);
        playerInput.Controls.ActionButton_4.performed += _ => KeyPress(playerInput.Controls, playerInput.Controls.ActionButton_4);
        playerInput.Controls.InputMenu.performed += _ => stateHandler.TransitionState(StateHandler.State.StatusMenuState);
        playerInput.Enable();
    }

    void DisablePlayerInput()
    {
        playerInput.Controls.InputPrimary.performed -= _ => inCombatPlayerAction.SelectUnit();
        playerInput.Controls.InputSecondary.performed -= _ => inCombatPlayerAction.MoveCharacter();
        //playerInput.Controls.AnyKey.performed -= _ => KeyPress(playerInput.Controls.AnyKey); // For if we want any "PRESS ANY KEY" moments
        playerInput.Controls.ActionButton_1.performed -= _ => KeyPress(playerInput.Controls, playerInput.Controls.ActionButton_1);
        playerInput.Controls.ActionButton_2.performed -= _ => KeyPress(playerInput.Controls, playerInput.Controls.ActionButton_2);
        playerInput.Controls.ActionButton_3.performed -= _ => KeyPress(playerInput.Controls, playerInput.Controls.ActionButton_3);
        playerInput.Controls.InputMenu.performed += _ => stateHandler.TransitionState(StateHandler.State.StatusMenuState);
        playerInput.Disable();
    }

    void SetClickHandler(InCombatPlayerAction inCombatPlayerAction)
    {
        this.inCombatPlayerAction = inCombatPlayerAction;
    }

    // Update is called once per frame
    public override bool HandleKeyPress()
    {
        // True/False if any key is pressed while in state
        return (playerInput.Controls.AnyKey.ReadValue<float>() > 0.5f);
    }

    bool KeyPress(PlayerInput.ControlsActions controls, InputAction action)
    {
        // Sends inputs to selected character.
        return inCombatPlayerAction.Keypress(controls, action);
    }

    public override void SetStateActive()
    {
        EnablePlayerInput();
    }

    public override void SetStateInactive()
    {
        DisablePlayerInput();
    }
}
