using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class EndTurnPanelScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    InCombatPlayerAction playerAction;
    Button button;
    AudioSource audioSource;

    TextMeshProUGUI label;
    List<Image> buttonFrames = new List<Image>();
    List<Image> buttonBackgrounds = new List<Image>();

    enum ButtonState { ACTIVE, PASSIVE, DISABLED };
    ButtonState currentButtonState = ButtonState.PASSIVE;

    bool _requirementsMet => CheckRequirementsMet();
    bool _playerTurn => StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState).isActive();

    // Colors for the icon and frame
    Dictionary<ButtonState, Color32> IconColors = new Dictionary<ButtonState, Color32>() {
        { ButtonState.ACTIVE, new Color32(0, 0, 0, 255) },
        { ButtonState.PASSIVE, new Color32(37, 232, 232, 255) },
        { ButtonState.DISABLED, new Color32(100, 100, 100, 255) },
    };

    // Colors for the background
    Dictionary<ButtonState, Color32> BackgroundColors = new Dictionary<ButtonState, Color32>() {
        { ButtonState.ACTIVE, new Color32(37, 232, 232, 255) },
        { ButtonState.PASSIVE, new Color32(0, 0, 0, 255) },
        { ButtonState.DISABLED, new Color32(0, 0, 0, 255) },
    };

    private void Awake()
    {
        audioSource = GetComponentInParent<AudioSource>();
        label = GetComponentInChildren<TextMeshProUGUI>();

        buttonFrames.Add(GetComponentsInChildren<Image>()[1]);
        buttonFrames.Add(GetComponentsInChildren<Image>()[3]);
        buttonFrames.Add(GetComponentsInChildren<Image>()[5]);

        buttonBackgrounds.Add(GetComponentsInChildren<Image>()[0]);
        buttonBackgrounds.Add(GetComponentsInChildren<Image>()[2]);
        buttonBackgrounds.Add(GetComponentsInChildren<Image>()[4]);
    }

    // Start is called before the first frame update
    private void Start()
    {
        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(ButtonPress);

        playerAction = ((PlayerTurnState)StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState)).GetPlayerAction();
    }

    // Update is called once per frame
    private void Update()
    {
        SetButtonState();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        // Highlights icon on mouse over

        if (!_requirementsMet) return;
        currentButtonState = ButtonState.ACTIVE;
        AudioClip audioClip = AudioManager.GetSound(InterfaceType.MOUSE_OVER, 0);
        audioSource.PlayOneShot(audioClip);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        // Clears unit highlight on mouse leave

        if (!_requirementsMet) return; 
        currentButtonState = ButtonState.PASSIVE;
    }

    private void SetButtonState()
    {
        // Changes button state based on if requirements are met

        if (!_requirementsMet) currentButtonState = ButtonState.DISABLED;
        else if (_requirementsMet && currentButtonState == ButtonState.DISABLED) currentButtonState = ButtonState.PASSIVE;

        label.color = IconColors[currentButtonState];
        foreach (Image image in buttonFrames)
            image.color = IconColors[currentButtonState];
        foreach (Image image in buttonBackgrounds)
            image.color = BackgroundColors[currentButtonState];
    }

    private bool CheckRequirementsMet()
    {
        if (!_playerTurn)
            return false;

        if (!playerAction.CheckTurnEnd() && !Keyboard.current.shiftKey.isPressed)
            return false;

        return true;
    }

    private void ButtonPress()
    {
        if (!_playerTurn)
            return;

        if (!_requirementsMet)
        {
            playerAction.SelectRemainingUnit();
            return;
        }

        AudioClip audioClip = AudioManager.GetSound(InterfaceType.MOUSE_CLICK, 0);
        audioSource.PlayOneShot(audioClip);

        playerAction.EndTurn();
    }
}
