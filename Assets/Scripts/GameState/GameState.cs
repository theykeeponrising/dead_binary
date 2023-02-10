using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Abstract class that all future game states (menus, combat, world map, etc.) will inherit from
//This is used to separate inputs for each state (since e.g. menus will have different inputs compared to combat)
public abstract class GameState
{
    protected StateHandler stateHandler;
    protected List<GameState> substates;
    public GameState activeSubState;
    protected GameState parentState; 
    public StateHandler.State stateEnum;
    public bool stateActive;
    protected bool keypressPaused;

    public virtual void Init(GameState parentState, StateHandler stateHandler)
    {
        this.parentState = parentState;
        this.stateHandler = stateHandler;
        this.substates = new List<GameState>();
    }

    public virtual void Start() {
        foreach (GameState substate in substates) substate.Start();
    }

    //Note newState must be a substate of this GameState
    public virtual void ChangeSubState(StateHandler.State newStateEnum) {
        foreach (GameState state in substates)
        {
            if (state.stateEnum == newStateEnum)
            {
                this.activeSubState.SetStateInactive();
                this.activeSubState = state;
                state.SetStateActive();
            }
        }
    }

    public virtual void ChangeState(StateHandler.State newStateEnum) {
        this.parentState.ChangeSubState(newStateEnum);
    }

    public bool IsChildStateOf(StateHandler.State otherState)
    {
        List<GameState> childStates = this.parentState.substates;
        return childStates.Contains(this);
    }

    public List<GameState> GetSubStates()
    {
        return this.substates;
    }

    public virtual void HandleInput() {
        
        if (!keypressPaused) {
            bool keyPressed = HandleKeyPress();
            if (keyPressed && stateActive)
            {
                stateHandler.WaitAfterKeyPress();
            }
        }
    }

    public StateHandler.State GetStateEnum()
    {
        return this.stateEnum;
    }

    //Get the substate object from the CombatSubState enum type
    public GameState GetSubStateObject(StateHandler.State stateObjectEnum)
    {
        foreach (GameState state in substates)
        {
            if (state.GetStateEnum() == stateObjectEnum) return state;
        }
        return null;
    }

    //Called via StateHandler.cs
    public virtual void Update() 
    {
        if (this.activeSubState != null) this.activeSubState.Update();
        //HandleInput();
    }

    //Called via StateHandler.cs
    public virtual void FixedUpdate()
    {
        if (this.activeSubState != null) this.activeSubState.FixedUpdate();
        //HandleContinuousInput();
    }

    public virtual void HandleContinuousInput()
    {
        HandleMovement();
    }

    public GameState FindSubState(StateHandler.State state) 
    {
        foreach (GameState substate in substates)
        {
            if (substate.stateEnum == state) return substate;
            else {
                GameState substateFound = substate.FindSubState(state);
                if (substateFound != null) return substateFound;
            }
        }
        return null;
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
        if (this.activeSubState != null) this.activeSubState.SetStateActive();
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

    public virtual IEnumerator WaitKeyPress()
    {
        keypressPaused = true;
        yield return new WaitForSeconds(0.25f);
        keypressPaused = false;
    }
}
