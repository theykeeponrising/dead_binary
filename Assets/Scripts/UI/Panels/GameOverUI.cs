using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using TMPro;

//Menu for exiting the game, may obtain additional functionality later
public class GameOverUI : MonoBehaviour
{
    public GameObject retryButton;
    public GameObject quitButton;
    public GameOverState gameOverState;
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

    public void SetGameOverState(GameOverState gameOverState)
    {
        this.gameOverState = gameOverState;
    }

    public void OnStartClick()
    {
        gameOverState.RestartGame();
    }

    public void OnQuitClick()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }
}
