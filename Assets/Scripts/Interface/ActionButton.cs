using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Class used to handle sprites for action buttons
    // Will be used to control mouse-over effects as well

    AudioSource audioSource;
    Button button;
    string spritePath;

    [SerializeField] private InCombatPlayerAction playerAction;
    PlayerTurnState playerTurnState;
    int buttonIndex;
    FiniteState<InCombatPlayerAction> state;

    ActionList boundAction;
    bool requirementsMet;

    Sprite icon;
    Image btnBackground;
    Image btnFrame;
    Image btnIcon;

    enum ButtonState { ACTIVE, PASSIVE, DISABLED };
    ButtonState currentButtonState = ButtonState.PASSIVE;

    // Colors for the icon and frame
    Dictionary<ButtonState, Color32> IconColors = new Dictionary<ButtonState, Color32>() {
        { ButtonState.ACTIVE, new Color32(37, 232, 232, 255) },
        { ButtonState.PASSIVE, new Color32(0, 0, 0, 255) },
        { ButtonState.DISABLED, new Color32(100, 100, 100, 255) },
    };

    // Colors for the background
    Dictionary<ButtonState, Color32> BackgroundColors = new Dictionary<ButtonState, Color32>() { 
        { ButtonState.ACTIVE, new Color32(0, 0, 0, 255) },
        { ButtonState.PASSIVE, new Color32(37, 232, 232, 255) },
        { ButtonState.DISABLED, new Color32(0, 0, 0, 255) },
    };

    void Start()
    {
        playerTurnState = (PlayerTurnState)StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        playerAction = playerTurnState.GetPlayerAction();

        btnBackground = GetComponentsInChildren<Image>()[0];
        btnFrame = GetComponentsInChildren<Image>()[1];
        btnIcon = GetComponentsInChildren<Image>()[2];

        btnIcon.sprite = icon;
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
        icon = Resources.Load<Sprite>(spritePath);
    }

    public string GetSpritePath()
    {
        return spritePath;
    }

    void CheckRequirements()
    {
        requirementsMet = Action.ActionsDict[boundAction].CheckRequirements(playerAction.selectedCharacter);
        if (!requirementsMet) currentButtonState = ButtonState.DISABLED;
        else if (requirementsMet && currentButtonState == ButtonState.DISABLED) currentButtonState = ButtonState.PASSIVE;

        btnIcon.color = IconColors[currentButtonState];
        btnFrame.color = IconColors[currentButtonState];
        btnBackground.color = BackgroundColors[currentButtonState];
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        // Highlights icon on mouse over

        if (requirementsMet) currentButtonState = ButtonState.ACTIVE;

        AudioClip audioClip = AudioManager.Instance.GetInterfaceSound(AudioManager.InterfaceSFX.MOUSE_OVER, 0);
        audioSource.PlayOneShot(audioClip);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        // Clears unit highlight on mouse leave

        if (requirementsMet) currentButtonState = ButtonState.PASSIVE;
    }

    public void BindAction(ActionList action)
    {
        // Stores action for checking requirements

        boundAction = action;
    }

    public ActionList GetAction()
    {
        // Returns bound action

        return boundAction;
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

    public void ButtonTrigger()
    {
        // Handle to kick off button trigger effect

        StartCoroutine(ButtonTriggerEffect());
    }

    IEnumerator ButtonTriggerEffect()
    {
        // Simulates the visual look of the action button being clicked
        // For use when action button is triggered via another input

        AudioClip audioClip = AudioManager.Instance.GetInterfaceSound(AudioManager.InterfaceSFX.MOUSE_CLICK, 0);
        audioSource.PlayOneShot(audioClip);

        currentButtonState = ButtonState.ACTIVE;
        yield return new WaitForSeconds(0.2f);
        currentButtonState = ButtonState.PASSIVE;
    }

    public void SetLabel(string newLabel)
    {
        // Changes text label to new value

        GetComponentInChildren<TextMeshProUGUI>().text = newLabel;
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
    public static string btn_action_medkit = "Buttons/btn_medkit";
}
