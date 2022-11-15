using UnityEngine;
using UnityEngine.InputSystem;

public class EndTurnPanelScript : ActionButton
{
    private InCombatPlayerAction _playerAction;
    private Canvas _canvas;

    private bool IsPlayerTurn { get { return StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState).isActive(); } }

    protected override void Awake()
    {
        base.Awake();
        _canvas = GetComponent<Canvas>();
    }

    protected override void Start()
    {
        base.Start();
        _playerAction = ((PlayerTurnState)StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState)).GetPlayerAction();
    }

    protected override void Update()
    {
        base.Update();
        SetColors();
        SetCanvasVisibility();
    }

    protected override void CheckRequirements()
    {
        // Changes button state based on if requirements are met

        _requirementsMet = CheckRequirementsMet();
        if (!_requirementsMet) _buttonState = ButtonState.DISABLED;
        else if (_requirementsMet && _buttonState == ButtonState.DISABLED) _buttonState = ButtonState.PASSIVE;
    }

    private bool CheckRequirementsMet()
    {
        if (!IsPlayerTurn)
            return false;
        
        var playerTurnState = (PlayerTurnState) StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        var currentState = playerTurnState.GetPlayerAction().stateMachine.GetCurrentState();

        return (_playerAction.CheckTurnEnd() || Keyboard.current.shiftKey.isPressed) && 
               (currentState.GetType() == typeof(StateNoSelection) || currentState.GetType() == typeof(StateIdle));
    }

    protected override void ButtonPress()
    {
        if (!IsPlayerTurn)
            return;

        if (!_requirementsMet)
        {
            _playerAction.SelectRemainingUnit();
            return;
        }

        AudioClip audioClip = AudioManager.GetSound(InterfaceType.MOUSE_CLICK, 0);
        _audioSource.PlayOneShot(audioClip);

        _playerAction.EndTurn();
    }

    private void SetColors()
    {
        _buttonLabel.color = BackgroundColors[_buttonState];
        _buttonFrame.color = BackgroundColors[_buttonState];
        _buttonBackground.color = IconColors[_buttonState];
    }

    private void SetCanvasVisibility()
    {
        _canvas.enabled = IsPlayerTurn;
    }
}
