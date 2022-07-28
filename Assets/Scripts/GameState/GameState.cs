using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Abstract class that all future game states (menus, combat, world map, etc.) will inherit from
//This is used to separate inputs for each state (since e.g. menus will have different inputs compared to combat)
public abstract class GameState : MonoBehaviour
{
    protected StateHandler stateHandler;
    protected bool keypressPaused;
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

    //Method to run when state becomes active (e.g. show UI, set variables, cleanup, etc.)
    public abstract void SetStateActive();
    //Method to run when state become inactive (e.g. hide UI, show message, etc.)
    public abstract void SetStateInactive();

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
