using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Class that manages all the (highest-level) states. Handles transitioning between states, passes inputs to the states, etc. 
public class StateHandler: MonoBehaviour
{
    public static StateHandler Instance = null;
    private GameRunningState gameRunningState;

    public enum State { 
        GameRunningState,
        StatusMenuState,
        CombatState,
        PlayerTurnState,
        EnemyTurnState
    };

    public State activeState;
    public List<GameState> gameStates;
    public bool keypressPaused = false;


    private void Awake() {
        Instance = this;
        gameRunningState = new GameRunningState();
        gameRunningState.Init(null, this);
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

    public State SetStateActive(State state) {
        State old_state = activeState;
        GetStateObject(old_state).SetStateInactive();
        GetStateObject(state).SetStateActive();
        StartCoroutine(GetStateObject(state).WaitKeyPress());
        activeState = state;
        return old_state;
    }

    //Obtain the currently active menu
    public State GetActiveState() {
        return activeState;
    }

    public GameState GetStateObject(State state) {
        if (state == State.GameRunningState) return gameRunningState;

        return gameRunningState.FindSubState(state);
    }

    // Ensures that the same key doesn't get registered multiple times when only pressing a single time
    public IEnumerator WaitKeyPress()
    {
        keypressPaused = true;
        yield return new WaitForSeconds(0.01f);
        keypressPaused = false;
    }

    public void WaitAfterKeyPress()
    {
        StartCoroutine(WaitKeyPress());
    }

    public void PauseKeypress(bool paused) {
        keypressPaused = paused;
    }

}
