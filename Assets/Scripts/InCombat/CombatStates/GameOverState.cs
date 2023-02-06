using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//Combat State is the basic gameplay, i.e. player vs robot battles. This class will handle inputs at a high level
public class GameOverState : GameState
{
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
        base.SetStateActive();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public bool CheckGameOver()
    {
        List<PlayerUnit> units = PlayerUnit.FindPlayerUnits();
        if (units.Count == 0) return true;
        return false;
    }
}
