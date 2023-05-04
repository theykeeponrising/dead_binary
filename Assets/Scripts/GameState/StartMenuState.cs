using UnityEngine;
using System.Collections;

//Start Menu
public class StartMenuState : GameState
{
    public StartMenuUI startMenuUI;

    public override void Init(GameState parentState, StateHandler stateHandler)
    {
        base.Init(parentState, stateHandler);
        this.stateEnum = StateHandler.State.StartMenuState;        
    }

    public override void SetStateActive()
    {
        if (startMenuUI == null)
        {
            startMenuUI = UIManager.GetStartMenu();
            startMenuUI.SetStartMenuState(this);
        }
        base.SetStateActive();
    }

    public override void Update()
    {
        base.Update();
        HandleInput();
    }

    public override bool HandleKeyPress()
    {
        return startMenuUI.HandleKeyPress();
    }

    public override void SetStateInactive()
    {
        base.SetStateInactive();
        startMenuUI.DisablePlayerInput();
    }

    public void StartGame()
    {
        SetStateInactive();
        StateHandler.Instance.LoadScene(1);
        //ChangeState(StateHandler.State.CombatState);
    }

    public void ShowCredits()
    {

    }
}
