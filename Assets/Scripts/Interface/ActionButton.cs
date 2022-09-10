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
    Sprite background_active;
    Sprite background_inactive;
    string spritePath;

    [SerializeField] private InCombatPlayerAction playerAction;
    PlayerTurnState playerTurnState;
    int buttonIndex;
    FiniteState<InCombatPlayerAction> state;
    public bool requirementsMet;
    public int apRequirement = 0;

    enum ButtonState { ACTIVE, PASSIVE, DISABLED };
    ButtonState currentButtonState = ButtonState.PASSIVE;

    Dictionary<ButtonState, Color32> ButtonColors = new Dictionary<ButtonState, Color32>() { 
        { ButtonState.ACTIVE, new Color32(37, 232, 232, 255) },
        { ButtonState.PASSIVE, new Color32(0, 0, 0, 255) },
        { ButtonState.DISABLED, new Color32(100, 100, 100, 255) },
    };

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

    void Update()
    {
        CheckRequirements();
    }

    public void LoadResources(string newSpritePath)
    {
        spritePath = newSpritePath;
        icon_active = Resources.Load<Sprite>(spritePath);
        //icon_inactive = Resources.Load<Sprite>(spritePath + "_1");
        background_inactive = Resources.Load<Sprite>(ActionButtons.btn_background);
        background_active = Resources.Load<Sprite>(ActionButtons.btn_background + "_1");
        GetComponentsInChildren<Image>()[1].sprite = icon_active;
    }

    public string GetSpritePath()
    {
        return spritePath;
    }

    void CheckRequirements()
    {
        requirementsMet = playerAction.selectedCharacter.stats.actionPointsCurrent >= apRequirement;
        if (!requirementsMet) currentButtonState = ButtonState.DISABLED;
        else if (requirementsMet && currentButtonState == ButtonState.DISABLED) currentButtonState = ButtonState.PASSIVE;
        GetComponentsInChildren<Image>()[1].color = ButtonColors[currentButtonState];
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        // Highlights icon on mouse over

        if (requirementsMet) currentButtonState = ButtonState.ACTIVE;
        GetComponentsInChildren<Image>()[0].sprite = background_inactive;

        AudioClip audioClip = AudioManager.Instance.GetInterfaceSound(AudioManager.InterfaceSFX.MOUSE_OVER, 0);
        audioSource.PlayOneShot(audioClip);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        // Clears unit highlight on mouse leave

        if (requirementsMet) currentButtonState = ButtonState.PASSIVE;
        GetComponentsInChildren<Image>()[0].sprite = background_active;
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

        if (!requirementsMet)
            return;

        playerTurnState = (PlayerTurnState)StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        playerAction = playerTurnState.GetPlayerAction();
        state = playerAction.stateMachine.GetCurrentState();

        //AudioClip audioClip = AudioManager.Instance.GetInterfaceSound(AudioManager.InterfaceSFX.MOUSE_CLICK, 0);
        //audioSource.PlayOneShot(audioClip);
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
