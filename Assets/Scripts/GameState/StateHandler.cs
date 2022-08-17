using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Class that manages all the (highest-level) states. Handles transitioning between states, passes inputs to the states, etc. 
public class StateHandler: MonoBehaviour
{
    private GameRunningState gameRunningState;
    private CombatState combatState;
    private StatusMenuState statusMenuState;
    private PlayerTurnState playerTurnState;
    private EnemyTurnState enemyTurnState;
    public enum State { 
        GameRunningState,
        StatusMenuState,
        CombatState,
        PlayerTurnState,
        EnemyTurnState
    };

    public State activeState;
    public List<GameState> gameStates;
    GlobalManager globalManager;
    public bool keypressPaused = false;
    
	public void Init(GlobalManager globalManager)
	{
        gameRunningState = new GameRunningState();
        gameRunningState.Init(null, this);

        // gameStates = new List<GameState> {
        //     new CombatState(),
        //     new StatusMenuState(),
        // };

        // if (gameStates.Count == 0) {
        //     throw new System.Exception("No gamestates were added to the game state manager.");
        // }
        

        // foreach (GameState gameState in gameStates) {
           
        //     if (gameState is CombatState)
        //     {
        //         combatState = (CombatState) gameState;
        //     } else if (gameState is StatusMenuState) {
        //         statusMenuState = (StatusMenuState) gameState;
        //     } else if (gameState is PlayerTurnState) {
        //         playerTurnState = (PlayerTurnState) gameState;
        //     } else if (gameState is EnemyTurnState) {
        //         enemyTurnState = (EnemyTurnState) gameState;
        //     }
        // }
        this.globalManager = globalManager;
	}

    private void Start() {
        this.gameRunningState.Start();
        gameRunningState.SetStateActive();
    }
    
	public void Update()
	{
        this.gameRunningState.Update();
	}

    public void FixedUpdate()
    {
        this.gameRunningState.FixedUpdate();
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
        this.keypressPaused = paused;
    }

}
