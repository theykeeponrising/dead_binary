using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ActionBarButton : ActionButton
{
    // Class used to handle sprites for action buttons
    // Will be used to control mouse-over effects as well

    int buttonIndex;
    string spritePath;

    Image btnBackground;
    Image btnFrame;
    Image btnIcon;
    TextMeshProUGUI btnLabel;
    TextMeshProUGUI btnQuantity;

    void Awake()
    {
        btnBackground = GetComponentsInChildren<Image>()[0];
        btnFrame = GetComponentsInChildren<Image>()[1];
        btnIcon = GetComponentsInChildren<Image>()[2];
        btnLabel = GetComponentsInChildren<TextMeshProUGUI>()[0];
        btnQuantity = GetComponentsInChildren<TextMeshProUGUI>()[1];
    }

    void OnEnable()
    {
        audioSource = GetComponentInParent<AudioSource>();
        button = GetComponent<Button>();
    }

    public override void LoadResources(string newSpritePath)
    {
        spritePath = newSpritePath;
        btnIcon.sprite = Resources.Load<Sprite>(spritePath);
    }

    public override void CheckRequirements()
    {
        requirementsMet = boundAction.CheckRequirements();
        if (!requirementsMet) currentButtonState = ButtonState.DISABLED;
        else if (requirementsMet && currentButtonState == ButtonState.DISABLED) currentButtonState = ButtonState.PASSIVE;

        btnIcon.color = IconColors[currentButtonState];
        btnFrame.color = IconColors[currentButtonState];
        btnBackground.color = BackgroundColors[currentButtonState];
        btnQuantity.color = IconColors[currentButtonState];
    }

    public override void CheckQuantity()
    {
        btnQuantity.text = boundItem.itemUsesCurrent.ToString();
    }

    public void BindIndex(int index)
    {
        // Binds state to action buttons

        buttonIndex = index;
        button.onClick.AddListener(ButtonPress);
    }

    public void SetLabel(string newLabel)
    {
        // Changes text label to new value

        btnLabel.text = newLabel;
    }

    public override void ButtonPress()
    {
        // On button press, play sound and execute indexed action from the state

        if (!requirementsMet)
            return;

        GetCurrentState().InputActionBtn(GetPlayerAction(), buttonIndex + 1);
    }
}
