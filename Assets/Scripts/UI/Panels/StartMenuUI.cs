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
    public StartMenuState startMenuState;
    public int currentPanelSelection;

    public void Start() 
    {
        startButton.GetComponent<Button>().onClick.AddListener(OnStartClick);
        quitButton.GetComponent<Button>().onClick.AddListener(OnQuitClick);
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
}
