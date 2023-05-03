using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using TMPro;

//Menu for exiting the game, may obtain additional functionality later
public class StatusMenuState : GameState
{
    public StatusMenuUI statusMenuUI;
    public enum StatusState {};

    public override void Init(GameState parentState, StateHandler stateHandler)
    {
        base.Init(parentState, stateHandler);
        this.stateEnum = StateHandler.State.StatusMenuState;
        CombatState.OnMapLoaded += OnMapLoaded;
    }

    public void OnMapLoaded()
    {
        statusMenuUI = UIManager.GetStatusMenu();
        statusMenuUI.SetStatusMenuState(this);
    }

    public override void Update()
    {
        base.Update();
        HandleInput();
    }

    // Update is called once per frame
    public override bool HandleKeyPress()
    {
        // True/False if any key is pressed while in state
        return statusMenuUI.HandleKeyPress();
    }

    public override void SetStateActive()
    {
        base.SetStateActive();
        statusMenuUI.EnablePlayerInput();
        statusMenuUI.DisplayMenu(true);
        //statusMenuUI.playerInput.Controls.InputMenu.performed += _ => this.ChangeState(StateHandler.State.CombatState);
    }

    public override void SetStateInactive()
    {
        base.SetStateActive();
        statusMenuUI.DisablePlayerInput();
        statusMenuUI.DisplayMenu(false);
        //statusMenuUI.playerInput.Controls.InputMenu.performed += _ => this.ChangeState(StateHandler.State.CombatState);
    }
}
