using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionBarButton : ActionButton
{
    // Class used to handle sprites for action bar buttons

    private int _buttonIndex;
    private string _spritePath;

    private Image _buttonIcon;
    private TextMeshProUGUI _buttonQuantity;

    protected override void Awake()
    {
        base.Awake();
        _buttonIcon = transform.Find("Icon").GetComponent<Image>();
        _buttonQuantity = transform.Find("Quantity").GetComponent<TextMeshProUGUI>();
    }

    public override void LoadResources(string newSpritePath)
    {
        _spritePath = newSpritePath;
        _buttonIcon.sprite = Resources.Load<Sprite>(_spritePath);
    }

    protected override void CheckRequirements()
    {
        _requirementsMet = BoundAction.CheckRequirements();
        if (!_requirementsMet) _buttonState = ButtonState.DISABLED;
        else if (_requirementsMet && _buttonState == ButtonState.DISABLED) _buttonState = ButtonState.PASSIVE;

        _buttonIcon.color = IconColors[_buttonState];
        _buttonFrame.color = IconColors[_buttonState];
        _buttonBackground.color = BackgroundColors[_buttonState];
        _buttonQuantity.color = IconColors[_buttonState];
    }

    protected override void CheckQuantity()
    {
        _buttonQuantity.text = BoundItem.itemUsesCurrent.ToString();
    }

    public void BindIndex(int index)
    {
        // Binds state to action buttons

        _buttonIndex = index;
    }

    public void SetLabel(string newLabel)
    {
        // Changes text label to new value

        _buttonLabel.text = newLabel;
    }

    protected override void ButtonPress()
    {
        // On button press, play sound and execute indexed action from the state

        if (!_requirementsMet)
            return;

        GetCurrentState().InputActionBtn(GetPlayerAction(), _buttonIndex + 1);
    }
}
