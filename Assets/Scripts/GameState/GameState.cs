using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Abstract class that all future game states (menus, combat, world map, etc.) will inherit from
//This is used to separate inputs for each state (since e.g. menus will have different inputs compared to combat)
public abstract class GameState
{
    protected StateHandler stateHandler;
    protected List<GameState> substates;
    protected GameState activeSubState;
    protected GameState parentState;
    protected 
    public bool stateActive;
    protected bool keypressPaused;

    public abstract GameState GetStateObject<T>(T stateName);

    public virtual void Init(GameState parentState)
    {
        this.parentState = parentState;
    }

    public virtual void ChangeState<T>(T stateName) {
        this.activeSubState.SetStateInactive();
        GameState subState = this.GetStateObject(stateName);
        subState.SetStateActive();
        this.activeSubState = subState;
    }

    public virtual void HandleInput() {
        
        if (!keypressPaused) {
            bool keyPressed = HandleKeyPress();
            if (keyPressed && this.isActiveAndEnabled)
            {
                StartCoroutine(WaitKeyPress());
            }
        }
    }

    public virtual void HandleContinuousInput()
    {
        HandleMovement();
    }

    //Handle key press for a particular game state
    //Returns true if a key was pressed
    public virtual bool HandleKeyPress() {
        return false;
    }
    public virtual void HandleMovement() {
        return;
    }

    public virtual bool isActive() 
    {
        return stateActive;
    }

    //Method to run when state becomes active (e.g. show UI, set variables, cleanup, etc.)
    public virtual void SetStateActive()
    {
        this.stateActive = true;
    }
    //Method to run when state become inactive (e.g. hide UI, show message, etc.)
    public virtual void SetStateInactive()
    {
        this.stateActive = false;
        foreach (GameState substate in substates)
        {
            if (substate.isActive()) substate.SetStateInactive();
        }
    }

    public virtual void SetStateManager(StateHandler stateHandler) {
        this.stateHandler = stateHandler;
    }

    public virtual IEnumerator WaitKeyPress()
    {
        keypressPaused = true;
        yield return new WaitForSeconds(0.25f);
        keypressPaused = false;
    }
}
