using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

//Class that manages all the (highest-level) states. Handles transitioning between states, passes inputs to the states, etc. 
public class StateHandler: MonoBehaviour
{
    public static StateHandler Instance = null;
    private GameRunningState gameRunningState;

    public enum State { 
        GameRunningState,
        StatusMenuState,
        StartMenuState,
        CombatState,
        TurnState,
        PlayerTurnState,
        EnemyTurnState,
        GameWinState,
        GameOverState
    };

    public State initialState = State.StartMenuState;
    public List<GameState> gameStates;
    public bool keypressPaused = false;


    private void Awake() {
        Instance = this;
        gameRunningState = new GameRunningState();
        gameRunningState.Init(null, this);
        gameRunningState.SetInitialState(initialState);
        gameRunningState.Start();
        gameRunningState.SetStateActive();
    }
    
	public void Update()
	{
        gameRunningState.Update();
	}

    public void FixedUpdate()
    {
        gameRunningState.FixedUpdate();
    }

    public GameState GetStateObject(State state) {
        if (state == State.GameRunningState) return gameRunningState;

        return gameRunningState.FindSubState(state);
    }

    // Ensures that the same key doesn't get registered multiple times when only pressing a single time
    public IEnumerator WaitKeyPress()
    {
        keypressPaused = true;
        yield return new WaitForSeconds(0.15f);
        keypressPaused = false;
    }

    public void WaitAfterKeyPress()
    {
        StartCoroutine(WaitKeyPress());
    }

    public void PauseKeypress(bool paused) {
        keypressPaused = paused;
    }

    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

}
