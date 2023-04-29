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

    public override void SetStateInactive()
    {
        base.SetStateActive();
    }

    public void StartGame()
    {
        StateHandler.Instance.LoadScene(1);
        //ChangeState(StateHandler.State.CombatState);
    }
}
