using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using TMPro;

//Menu for exiting the game, may obtain additional functionality later
public class GameWinUI : MonoBehaviour
{
    public GameObject retryButton;
    public GameObject quitButton;
    public GameWinState gameWinState;
    public int currentPanelSelection;

    public void Init()
    {
        SetActive(false);
    }
    
    public void Start() 
    {
        retryButton.GetComponent<Button>().onClick.AddListener(OnStartClick);
        quitButton.GetComponent<Button>().onClick.AddListener(OnQuitClick);
        
    }

    public void SetActive(bool active)
    {
        gameObject.GetComponent<CanvasGroup>().alpha = active ? 1 : 0;
        gameObject.GetComponent<CanvasGroup>().blocksRaycasts = active;
    }

    public void SetGameWinState(GameWinState gameWinState)
    {
        this.gameWinState = gameWinState;
    }

    public void OnStartClick()
    {
        gameWinState.RestartGame();
    }

    public void OnQuitClick()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }
}
