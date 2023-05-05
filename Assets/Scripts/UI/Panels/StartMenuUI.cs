using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using TMPro;

//Menu for exiting the game, may obtain additional functionality later
public class StartMenuUI : MonoBehaviour
{
    public GameObject startButton;
    public GameObject quitButton;
    public GameObject creditsButton;
    public GameObject creditsPanel;
    public StartMenuState startMenuState;
    public int currentPanelSelection;
    public bool bCreditsActive = false;
    private PlayerInput playerInput;

    void Awake()
    {
        playerInput = new PlayerInput();
    }

    public void Start() 
    {
        startButton.GetComponent<Button>().onClick.AddListener(OnStartClick);
        quitButton.GetComponent<Button>().onClick.AddListener(OnQuitClick);
        creditsButton.GetComponent<Button>().onClick.AddListener(OnCreditsClick);
        SetCreditsActive(false);
        EnablePlayerInput();
    }

    public bool HandleKeyPress()
    {
        if (IsCreditsActive() && playerInput.Controls.AnyKey.IsPressed())
        {
            OnCloseCreditsPanel();
            return true;
        }
        return false;
    }

    public void SetStartMenuState(StartMenuState startMenuState)
    {
        this.startMenuState = startMenuState;
    }

    public void OnStartClick()
    {
        startMenuState.StartGame();
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }

    public void OnCreditsClick()
    {
        SetCreditsActive(true);
    }

    public bool IsCreditsActive()
    {
        return bCreditsActive;
    }

    public void OnCloseCreditsPanel()
    {
        SetCreditsActive(false);
    }

    public void EnablePlayerInput()
    {
        playerInput.Enable();
    }

    public void DisablePlayerInput()
    {
        playerInput.Disable();
    }

    public void SetCreditsActive(bool active)
    {
        creditsPanel.GetComponent<CanvasGroup>().alpha = active ? 1 : 0;
        creditsPanel.GetComponent<CanvasGroup>().blocksRaycasts = active;
        bCreditsActive = active;
    }
}
