using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
public class StatusMenuState : GameState
{
    bool shouldQuit = true;
    public GameObject yesPanel;
    public GameObject noPanel;

    public void Start() 
    {
        gameObject.GetComponent<CanvasGroup>().alpha = 0;
    }

    public override bool HandleKeyPress()
    {
        bool keyPressed = false;
        if (Input.GetAxis("Cancel") > 0)
        {
            stateHandler.TransitionState(StateHandler.State.CombatState);
            keyPressed = true;
        }

        if (Input.GetAxis("Horizontal") != 0)
        {
            shouldQuit = !shouldQuit;
            int alpha = 0;
            if (shouldQuit) alpha = 1;
            yesPanel.GetComponent<CanvasGroup>().alpha = alpha;
            noPanel.GetComponent<CanvasGroup>().alpha = 1 - alpha;
            keyPressed = true;
        }

        if (Input.GetAxis("Submit") > 0)
        {
            Debug.Log(shouldQuit);
            if (shouldQuit) {
                UnityEditor.EditorApplication.isPlaying = false;
                Application.Quit();
            }
            else stateHandler.TransitionState(StateHandler.State.CombatState);
        }
        return keyPressed;
    }

    public void DisplayMenu(bool visible)
    {
        int alpha = 1;
        if (!visible) alpha = 0;
        gameObject.GetComponent<CanvasGroup>().alpha = alpha;
        yesPanel.GetComponent<CanvasGroup>().alpha = alpha;
        noPanel.GetComponent<CanvasGroup>().alpha = 0;
        shouldQuit = true;
    }

    public override void SetStateActive()
    {
        DisplayMenu(true);
    }

    public override void SetStateInactive()
    {
        DisplayMenu(false);
    }

}
