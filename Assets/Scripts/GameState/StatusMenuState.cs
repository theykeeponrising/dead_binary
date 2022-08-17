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
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("UI");
        foreach (GameObject go in gameObjects)
        {
            StatusMenuUI ui = go.GetComponent<StatusMenuUI>();
            if (ui) {
                statusMenuUI = ui;
                statusMenuUI.SetStatusMenuState(this);
            }
        }
    }

    public override void Update()
    {
        base.Update();
        HandleKeyPress();
    }

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
        //statusMenuUI.playerInput.Controls.InputMenu.performed += _ => this.ChangeState(StateHandler.State.CombatState);
    }

    public override void SetStateInactive()
    {
        statusMenuUI.DisablePlayerInput();
        statusMenuUI.DisplayMenu(false);
        //statusMenuUI.playerInput.Controls.InputMenu.performed += _ => this.ChangeState(StateHandler.State.CombatState);
    }
}
