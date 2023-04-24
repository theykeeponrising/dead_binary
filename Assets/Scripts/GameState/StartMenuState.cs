using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using TMPro;

//Start Menu
public class StartMenuState : GameState
{
    public StartMenuUI startMenuUI;

    public override void Init(GameState parentState, StateHandler stateHandler)
    {
        base.Init(parentState, stateHandler);
        this.stateEnum = StateHandler.State.StartMenuState;
        startMenuUI = UIManager.GetStartMenu();
        startMenuUI.SetStartMenuState(this);
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
        return startMenuUI.HandleKeyPress();
    }

    public override void SetStateActive()
    {
        startMenuUI.EnablePlayerInput();
        startMenuUI.DisplayMenu(true);
        //statusMenuUI.playerInput.Controls.InputMenu.performed += _ => this.ChangeState(StateHandler.State.CombatState);
    }

    public override void SetStateInactive()
    {
        startMenuUI.DisablePlayerInput();
        startMenuUI.DisplayMenu(false);
        //statusMenuUI.playerInput.Controls.InputMenu.performed += _ => this.ChangeState(StateHandler.State.CombatState);
    }
}
