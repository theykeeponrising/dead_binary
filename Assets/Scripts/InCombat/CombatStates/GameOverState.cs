using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//Combat State is the basic gameplay, i.e. player vs robot battles. This class will handle inputs at a high level
public class GameOverState : GameState
{
    GameOverUI gameOverUI;
    public override void Init(GameState parentState, StateHandler stateHandler) 
    {
        base.Init(parentState, stateHandler);
        this.stateEnum = StateHandler.State.GameOverState;
    }

    public override void Update() 
    {
        base.Update();
    }

    public override void SetStateActive()
    {
        if (gameOverUI == null)
        {
            gameOverUI = UIManager.GetGameOverMenu();
            gameOverUI.SetGameOverState(this);
            gameOverUI.SetActive(true);
        }
        base.SetStateActive();
    }

    public bool CheckGameOver()
    {
        List<PlayerUnit> units = PlayerUnit.FindPlayerUnits();
        if (units.Count == 0) return true;
        return false;
    }

    public void RestartGame()
    {
        StateHandler.Instance.ReloadScene();
    }
}
