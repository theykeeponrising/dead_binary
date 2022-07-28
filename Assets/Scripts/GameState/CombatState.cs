using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Combat State is the basic gameplay, i.e. player vs robot battles. This class will handle inputs at a high level
public class CombatState : GameState
{
    // Used to manage user inputs

    public Character selectedCharacter;
    public InCombatPlayerAction inCombatPlayerAction;

    // Start is called before the first frame update
    void SetClickHandler(InCombatPlayerAction inCombatPlayerAction)
    {
        this.inCombatPlayerAction = inCombatPlayerAction;
    }

    // Update is called once per frame
    public override bool HandleKeyPress()
    {
        bool keyPressed = false;
        if (Input.anyKeyDown)
        {
            keyPressed = KeyPress();
        }
        if (Input.GetMouseButtonDown(1)) {
            inCombatPlayerAction.MoveCharacter();
            keyPressed = true;
        } 
        else if (Input.GetMouseButtonDown(0))
        {
            inCombatPlayerAction.SelectUnit();
            keyPressed = true;
        }
        return keyPressed;
    }

    bool KeyPress()
    {
        // Sends inputs to selected character.
        foreach (KeyCode keycode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKey(keycode))
            {
                if (keycode == KeyCode.Escape) stateHandler.TransitionState(StateHandler.State.StatusMenuState);
                if (selectedCharacter)
                    selectedCharacter.KeyPress(keycode);
                return true;
            }
        }
        return false;
    }

    public override void SetStateActive()
    {
        return;
    }

    public override void SetStateInactive()
    {
        return;
    }
}
