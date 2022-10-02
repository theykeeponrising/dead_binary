using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using TMPro;

//Menu for exiting the game, may obtain additional functionality later
public class StatusMenuUI : MonoBehaviour
{
    public GameObject yesPanel;
    public GameObject noPanel;
    public StatusMenuState statusMenuState;
    public PlayerInput playerInput;
    public bool shouldQuit;

    void Awake()
    {
        playerInput = new PlayerInput();
    }

    public void Start() 
    {
        gameObject.GetComponent<CanvasGroup>().alpha = 0;
    }

    public void SetStatusMenuState(StatusMenuState statusMenuState)
    {
        this.statusMenuState = statusMenuState;
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

        if (playerInput.Controls.InputCancel.triggered || playerInput.Controls.InputMenu.triggered)
        {
            statusMenuState.ChangeState(StateHandler.State.CombatState);
            keyPressed = true;
        }

        float joystickX = playerInput.Controls.InputJoystick.ReadValue<Vector2>().x;

        if (joystickX != 0)
        {
            int yesPanelAlpha = Mathf.CeilToInt(-joystickX);
            int noPanelAlpha = 1 - yesPanelAlpha;

            shouldQuit = yesPanelAlpha == 1;

            yesPanel.GetComponent<CanvasGroup>().alpha = yesPanelAlpha;
            noPanel.GetComponent<CanvasGroup>().alpha = noPanelAlpha;
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
            else statusMenuState.ChangeState(StateHandler.State.CombatState);
        }
        return keyPressed;
    }

    public void DisplayMenu(bool visible)
    {
        int alpha = 1;
        if (!visible) alpha = 0;
        GetComponentInParent<GraphicRaycaster>().enabled = visible;
        gameObject.GetComponent<CanvasGroup>().alpha = alpha;
        yesPanel.GetComponent<CanvasGroup>().alpha = alpha;
        noPanel.GetComponent<CanvasGroup>().alpha = 0;
        shouldQuit = true;
    }
}
