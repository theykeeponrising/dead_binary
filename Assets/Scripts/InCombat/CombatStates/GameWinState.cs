using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

//Combat State is the basic gameplay, i.e. player vs robot battles. This class will handle inputs at a high level
public class GameWinState : GameState
{
    GameWinUI gameWinUI;
    public override void Init(GameState parentState, StateHandler stateHandler) 
    {
        base.Init(parentState, stateHandler);
        this.stateEnum = StateHandler.State.GameWinState;
    }

    public override void Update() 
    {
        base.Update();
    }

    public override void SetStateActive()
    {
        if (gameWinUI == null)
        {
            gameWinUI = UIManager.GetGameWinMenu();
            gameWinUI.SetGameWinState(this);
            gameWinUI.SetActive(true);
        }
        base.SetStateActive();
    }

    public bool CheckGameWin()
    {
        List<EnemyUnit> units = Map.FindEnemyUnits();
        if (units.Count == 0) return true;
        return false;
    }

    public void RestartGame()
    {
        StateHandler.Instance.ReloadScene();
    }
}
