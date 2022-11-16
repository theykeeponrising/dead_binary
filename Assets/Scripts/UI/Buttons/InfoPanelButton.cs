public class InfoPanelButton : ActionButton
{
    protected override void CheckRequirements()
    {
        _requirementsMet = StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState).isActive();
        if (!_requirementsMet) _buttonState = ButtonState.DISABLED;
        else if (_requirementsMet && _buttonState == ButtonState.DISABLED) _buttonState = ButtonState.PASSIVE;

        _buttonLabel.color = BackgroundColors[_buttonState];
        _buttonFrame.color = BackgroundColors[_buttonState];
        _buttonBackground.color = IconColors[_buttonState];
    }

    protected override void ButtonPress()
    {
        if (_requirementsMet) _buttonState = ButtonState.PASSIVE;
    }
}
