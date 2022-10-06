using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public abstract class ActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Class used to handle sprites for action buttons
    // Will be used to control mouse-over effects as well

    public AudioSource audioSource;
    public Button button;

    public Item boundItem;
    public UnitAction boundAction;
    public Unit boundUnit;
    public bool requirementsMet;

    public enum ButtonState { ACTIVE, PASSIVE, DISABLED };
    public ButtonState currentButtonState = ButtonState.PASSIVE;

    // Colors for the icon and frame
    public Dictionary<ButtonState, Color32> IconColors = new Dictionary<ButtonState, Color32>() {
        { ButtonState.ACTIVE, new Color32(37, 232, 232, 255) },
        { ButtonState.PASSIVE, new Color32(0, 0, 0, 255) },
        { ButtonState.DISABLED, new Color32(100, 100, 100, 255) },
    };

    // Colors for the background
    public Dictionary<ButtonState, Color32> BackgroundColors = new Dictionary<ButtonState, Color32>() {
        { ButtonState.ACTIVE, new Color32(0, 0, 0, 255) },
        { ButtonState.PASSIVE, new Color32(37, 232, 232, 255) },
        { ButtonState.DISABLED, new Color32(0, 0, 0, 255) },
    };

    // Colors for the factions
    public Dictionary<Faction, Color32> FactionColors = new Dictionary<Faction, Color32>() {
        { Faction.Good, new Color32(37, 232, 232, 255) },
        { Faction.Bad, new Color32(232, 37, 37, 255) },
    };

    void OnEnable()
    {
        audioSource = GetComponentInParent<AudioSource>();
        button = GetComponent<Button>();
    }

    void Update()
    {
        if (boundAction) CheckRequirements();
        if (boundItem) CheckQuantity();
    }

    public virtual void LoadResources(string newSpritePath)
    {
        Debug.Log("Load Resources override missing for button!");
    }

    public virtual void LoadResources(string[] newSpritePath)
    {
        Debug.Log("Load Resources override missing for button!");
    }

    public virtual void CheckRequirements()
    {
        Debug.Log("Check Requirements override missing for button!");
    }

    public virtual void CheckQuantity()
    {
        Debug.Log("Check Quantity override missing for button!");
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

    public virtual void BindAction(UnitAction action)
    {
        // Stores action for checking requirements

        boundAction = action;
    }

    public virtual void BindItem(Item item)
    {
        // Binds item for requirement checking

        boundItem = item;
    }

    public virtual void BindUnit(Unit unit)
    {
        boundUnit = unit;
    }

    public UnitAction GetAction()
    {
        // Returns bound action

        return boundAction;
    }

    public Item GetItem()
    {
        // Returns bound action

        return boundItem;
    }

    public InCombatPlayerAction GetPlayerAction()
    {
        PlayerTurnState playerTurnState = (PlayerTurnState)StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        InCombatPlayerAction playerAction = playerTurnState.GetPlayerAction();
        return playerAction;
    }

    public FiniteState<InCombatPlayerAction> GetCurrentState()
    {
        FiniteState<InCombatPlayerAction> state = GetPlayerAction().stateMachine.GetCurrentState();
        return state;
    }

    public void UnbindButton()
    {
        // Removes button bindings

        button.onClick.RemoveAllListeners();
    }

    public virtual void ButtonPress()
    {
        Debug.Log("Button Press override missing for button!");
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
    public static string btn_action_grenade = "Buttons/btn_grenade";
}

public enum ActionButtonSprite { MOVE, SHOOT, RELOAD, SWAP, CHOOSE_ITEM, MEDKIT, GRENADE }
