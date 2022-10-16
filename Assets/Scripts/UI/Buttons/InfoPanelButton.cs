using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InfoPanelButton : ActionButton
{
    TextMeshProUGUI label;
    List<Image> buttonFrames = new List<Image>();
    List<Image> buttonBackgrounds = new List<Image>();


    new bool requirementsMet => StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState).isActive();

    void Awake()
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

    void Start()
    {
        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(ButtonPress);
    }

    public override void CheckRequirements()
    {
        if (!requirementsMet) currentButtonState = ButtonState.DISABLED;
        else if (requirementsMet && currentButtonState == ButtonState.DISABLED) currentButtonState = ButtonState.PASSIVE;

        label.color = IconColors[currentButtonState];
        foreach (Image image in buttonFrames)
            image.color = IconColors[currentButtonState];
        foreach (Image image in buttonBackgrounds)
            image.color = BackgroundColors[currentButtonState];
    }

    public override void ButtonPress()
    {
        if (requirementsMet) currentButtonState = ButtonState.PASSIVE;
    }
}
