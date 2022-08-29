using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Class used to handle sprites for action buttons
    // Will be used to control mouse-over effects as well

    AudioSource audioSource;
    Button button;

    Sprite icon_active;
    Sprite icon_inactive;
    Sprite background_active;
    Sprite background_inactive;
    string spritePath;

    [SerializeField] private InCombatPlayerAction playerAction;
    PlayerTurnState playerTurnState;
    int buttonIndex;
    FiniteState<InCombatPlayerAction> state;

    void Start()
    {
        playerTurnState = (PlayerTurnState)StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        playerAction = playerTurnState.GetPlayerAction();
    }

    void OnEnable()
    {
        audioSource = GetComponentInParent<AudioSource>();
        button = GetComponent<Button>();
    }

    public void LoadResources(string newSpritePath)
    {
        spritePath = newSpritePath;
        icon_active = Resources.Load<Sprite>(spritePath);
        icon_inactive = Resources.Load<Sprite>(spritePath + "_1");
        background_inactive = Resources.Load<Sprite>(ActionButtons.btn_background);
        background_active = Resources.Load<Sprite>(ActionButtons.btn_background + "_1");
        GetComponentsInChildren<Image>()[1].sprite = icon_active;
    }

    public string GetSpritePath()
    {
        return spritePath;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        // Highlights icon on mouse over

        GetComponentsInChildren<Image>()[0].sprite = background_inactive;
        GetComponentsInChildren<Image>()[1].sprite = icon_inactive;

        AudioClip audioClip = AudioManager.Instance.GetInterfaceSound(AudioManager.InterfaceSFX.MOUSE_OVER, 0);
        audioSource.PlayOneShot(audioClip);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        // Clears unit highlight on mouse leave

        GetComponentsInChildren<Image>()[0].sprite = background_active;
        GetComponentsInChildren<Image>()[1].sprite = icon_active;
    }

    public void BindButton(int index)
    {
        // Binds state to action buttons

        buttonIndex = index;
        button.onClick.AddListener(ButtonPress);
    }

    public void UnbindButton()
    {
        // Removes button bindings

        button.onClick.RemoveAllListeners();
    }

    public void ButtonPress()
    {
        // On button press, play sound and execute indexed action from the state
        
        playerTurnState = (PlayerTurnState)StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        playerAction = playerTurnState.GetPlayerAction();
        state = playerAction.stateMachine.GetCurrentState();

        AudioClip audioClip = AudioManager.Instance.GetInterfaceSound(AudioManager.InterfaceSFX.MOUSE_CLICK, 0);
        audioSource.PlayOneShot(audioClip);
        state.InputActionBtn(playerAction, buttonIndex + 1);
    }
}

public static class ActionButtons
{
    // Loads action button sprites from resources

    public static string btn_background = "Buttons/btn_background";
    public static string btn_action_move = "Buttons/btn_move";
    public static string btn_action_shoot = "Buttons/btn_shoot";
    public static string btn_action_reload = "Buttons/btn_reload";
    public static string btn_action_swap = "Buttons/btn_swap";
    public static string btn_action_chooseItem = "Buttons/btn_chooseItem";
    public static string btn_action_useItem = "Buttons/btn_useItem";
}
