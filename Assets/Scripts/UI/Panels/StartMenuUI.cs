using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

//Menu for exiting the game, may obtain additional functionality later
public class StartMenuUI : MonoBehaviour
{
    public GameObject startButton;
    public GameObject quitButton;
    public GameObject creditsButton;
    public GameObject creditsPanel;
    public List<GameObject> creditsPages;
    public GameObject loadingPanel;
    public StartMenuState startMenuState;
    public int currentPanelSelection;
    public bool bCreditsActive = false;
    public bool bLoadingPanelActive = false;
    private PlayerInput playerInput;
    private int _creditsIndex = 0;
    private bool _waitBuffer = false;

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
        SetLoadingPanelActive(false);
        EnablePlayerInput();
    }

    public bool HandleKeyPress()
    {
        if (IsCreditsActive() && playerInput.Controls.AnyKey.IsPressed())
        {
            if (_waitBuffer)
                return false;

            if (_creditsIndex < creditsPages.Count)
            {
                NextCreditsPage();
                _creditsIndex++;
                _waitBuffer = true;
                return true;
            }
            else
            {
                OnCloseCreditsPanel();
                _waitBuffer = true;
                return true;
            }
        }
        _waitBuffer = false;
        return false;
    }

    public void SetStartMenuState(StartMenuState startMenuState)
    {
        this.startMenuState = startMenuState;
    }

    public void OnStartClick()
    {
        StartCoroutine(StartGame());
    }

    public IEnumerator StartGame()
    {
        SetLoadingPanelActive(true);
        yield return new WaitForSeconds(0.1f);
        startMenuState.StartGame();
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }

    public void OnCreditsClick()
    {
        SetCreditsActive(true);
        NextCreditsPage();
        _creditsIndex++;
    }

    public bool IsCreditsActive()
    {
        return bCreditsActive;
    }

    public void OnCloseCreditsPanel()
    {
        SetCreditsActive(false);
        foreach (GameObject creditPage in creditsPages)
            creditPage.SetActive(false);
        _creditsIndex = 0;
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

    public void NextCreditsPage()
    {
        if (_creditsIndex > 0)
            creditsPages[_creditsIndex - 1].SetActive(false);

        creditsPages[_creditsIndex].SetActive(true);
    }

    public void SetLoadingPanelActive(bool active)
    {
        loadingPanel.GetComponent<CanvasGroup>().alpha = active ? 1 : 0;
        loadingPanel.GetComponent<CanvasGroup>().blocksRaycasts = active;
        bLoadingPanelActive = active;
    }
}
