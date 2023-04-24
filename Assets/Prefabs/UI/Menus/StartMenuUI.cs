using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using TMPro;

//Menu for exiting the game, may obtain additional functionality later
public class StartMenuUI : MonoBehaviour
{
    public GameObject startGamePanel;
    public GameObject quitGamePanel;
    public StartMenuState startMenuState;
    public PlayerInput playerInput;
    public bool shouldQuit;

    //Used by PlayerTurnState to check if SetStateActive is coming from Status Menu
    public static bool ISSTATUS = false;

    void Awake()
    {
        playerInput = new PlayerInput();
    }

    public void Start() 
    {
        gameObject.GetComponent<CanvasGroup>().alpha = 0;
    }

    public void SetStartMenuState(StartMenuState startMenuState)
    {
        this.startMenuState = startMenuState;
    }

    public void EnablePlayerInput()
    {
        playerInput.Enable();
    }

    public void DisablePlayerInput()
    {
        playerInput.Disable();
    }

    // Better subscribe on user input events, instead of invoking unnecessarry checks in every frame.
    public bool HandleKeyPress()
    {
        bool keyPressed = false;
        float joystickX = playerInput.Controls.InputJoystick.ReadValue<Vector2>().x;

        if (joystickX != 0)
        {
            int startPanelAlpha = Mathf.CeilToInt(-joystickX);
            int quitPanelAlpha = 1 - startPanelAlpha;

            shouldQuit = startPanelAlpha == 1;

            startGamePanel.GetComponent<CanvasGroup>().alpha = startPanelAlpha;
            quitGamePanel.GetComponent<CanvasGroup>().alpha = quitPanelAlpha;
            keyPressed = true;
        }

        if (playerInput.Controls.InputSubmit.triggered)
        {
            Debug.Log(shouldQuit);
            if (shouldQuit)
            {
                // UnityEditor.EditorApplication.isPlaying = false;
                Application.Quit();
                return true;
            }
            else startMenuState.ChangeState(StateHandler.State.CombatState);
        }
        return keyPressed;
    }

    public void DisplayMenu(bool visible)
    {
        int alpha = 1;
        if (!visible) alpha = 0;
        GetComponentInParent<GraphicRaycaster>().enabled = visible;
        gameObject.GetComponent<CanvasGroup>().alpha = alpha;
        startGamePanel.GetComponent<CanvasGroup>().alpha = alpha;
        quitGamePanel.GetComponent<CanvasGroup>().alpha = 0;
        shouldQuit = false;
    }
}
