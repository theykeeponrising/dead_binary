using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Class that manages all the (highest-level) states. Handles transitioning between states, passes inputs to the states, etc. 
public class StateHandler: MonoBehaviour
{
    private CombatState combatState;
    private StatusMenuState statusMenuState;
    public enum State { StatusMenuState, CombatState };
    public State activeState;
    public List<GameState> gameStates;
    GlobalManager globalManager;
    public bool keypressPaused = false;
    
	public void Init(GlobalManager globalManager)
	{
        gamestates = new List<GameState> {
            new CombatState(),
            new StatusMenuState(),
        };

        if (gameStates.Count == 0) {
            throw new System.Exception("No gamestates were added to the game state manager.");
        }
        
        foreach (GameState gameState in gameStates) {
           
            if (gameState is CombatState)
            {
                combatState = (CombatState) gameState;
            } else if (gameState is StatusMenuState) {
                statusMenuState = (StatusMenuState) gameState;
            }
            gameState.SetStateManager(this);
            gameState.Init();
        }
        this.globalManager = globalManager;
	}

    private void Start() {
        this.GetStateObject(this.activeState).SetStateActive();
    }
	public void Update()
	{
        this.GetStateObject(this.activeState).HandleInput();
	}

    public void FixedUpdate()
    {
        this.GetStateObject(this.activeState).HandleContinuousInput();
    }

    public State SetStateActive(State state) {
        State old_state = this.activeState;
        this.GetStateObject(old_state).SetStateInactive();
        this.GetStateObject(state).SetStateActive();
        StartCoroutine(this.GetStateObject(state).WaitKeyPress());
        this.activeState = state;
        return old_state;
    }

    //Obtain the currently active menu
    public State GetActiveState() {
        return this.activeState;
    }

    public GameState GetStateObject(State state) {
        if (state == State.CombatState)
        {
            return combatState;
        }
        else if (state == State.StatusMenuState)
        {
            return statusMenuState;
        }
        else
        {
            return null;
        }
    }

    public void ChangeState(State state) {
        this.GetStateObject(this.activeState).SetStateInactive();
        this.SetStateActive(state);
    }

    // Ensures that the same key doesn't get registered multiple times when only pressing a single time
    public IEnumerator WaitKeypress()
    {
        keypressPaused = true;
        yield return new WaitForSeconds(0.01f);
        keypressPaused = false;
    }

    public void PauseKeypress(bool paused) {
        this.keypressPaused = paused;
    }

}
