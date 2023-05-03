using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//Combat State is the basic gameplay, i.e. player vs robot battles. This class will handle inputs at a high level
public class CombatState : GameState
{
    // Used to manage user inputs
    public InCombatPlayerAction inCombatPlayerAction;

    // Creates an event for when the scene/map is loaded
    public delegate void MapLoaded();
    public static event MapLoaded OnMapLoaded;
    private bool _mapLoaded = false;

    public override void Init(GameState parentState, StateHandler stateHandler) 
    {
        base.Init(parentState, stateHandler);
        this.stateEnum = StateHandler.State.CombatState;
        this.substates = new List<GameState> {
            new TurnState(),
            new StatusMenuState(),
            new GameOverState(),
            new GameWinState(),
        };

        foreach (GameState gameState in substates) {
            gameState.Init(this, stateHandler);
        }

        this.activeSubState = FindSubState(StateHandler.State.TurnState);
    }

    public override void Update() 
    {
        base.Update();
    }

    public override void SetStateActive()
    {
        if (!_mapLoaded)
        {
            OnMapLoaded();
            _mapLoaded = true;
        }
        
        base.SetStateActive();
    }

    public void CheckGameConditions()
    {
        bool gameEnded = CheckGameWin();
        if (!gameEnded) CheckGameOver();
    }

    public bool CheckGameOver()
    {
        GameOverState gameOverState = (GameOverState) StateHandler.Instance.GetStateObject(StateHandler.State.GameOverState);
        bool gameOver = gameOverState.CheckGameOver();

        if (gameOver)
        {
            this.activeSubState.ChangeState(StateHandler.State.GameOverState);
            Debug.Log("Game Over.");
        }
        return gameOver;
    }

    public bool CheckGameWin()
    {
        GameWinState gameWinState = (GameWinState) StateHandler.Instance.GetStateObject(StateHandler.State.GameWinState);
        bool gameWin = gameWinState.CheckGameWin();
        if (gameWin) 
        {
            this.activeSubState.ChangeState(StateHandler.State.GameWinState);
            Debug.Log("Map clear! Well done.");
        }
        return gameWin;
    }
}
