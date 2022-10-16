using UnityEngine;
using TMPro;

public sealed class TurnIndicatorPanel : MonoBehaviour
{
    private Transform _objectsContainer;
    private RectTransform _textObject;
    private TextMeshProUGUI _textMeshComponent;

    private string _defaultMessage = "--- PEACEKEEPING OPERATION UNDERWAY::PLEASE REMAIN CALM ---";
    private string _tempMessage;
    private int _messagePriority = 0;
    private float _length = 765f;

    [Tooltip("The speed at which the text scrolls. Lower is faster.")]
    [SerializeField] private float _scrollSpeed = 1f;

    private Vector2 _currentPosition { get { return _textObject.transform.localPosition; } set { _textObject.transform.localPosition = value; } }
    private bool _displayPanel => FactionManager.ACS.TurnInProgress;

    private void Start()
    {
        _objectsContainer = transform.Find("ObjectsContainer");
        _textObject = _objectsContainer.Find("ScrollingTextMask").GetComponentsInChildren<RectTransform>()[1];
        _textMeshComponent = _textObject.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void LateUpdate()
    {
        SetPanelActive();
        MoveToStartPosition();
        MoveTextObject();
    }

    private void SetPanelActive()
    {
        _objectsContainer.gameObject.SetActive(_displayPanel);
    }

    private void MoveTextObject()
    {
        // Move the text object left at the speed value provided
        _currentPosition = Vector2.Lerp(_currentPosition + Vector2.left, _currentPosition, _scrollSpeed);
    }

    private void MoveToStartPosition()
    {
        // When disabled, send the text back to the right
        if (!_displayPanel)
        {
            _currentPosition = new Vector2(-_length * 0.75f, _currentPosition.y);
            return;
        }

        // Reset text back to the right side
        else if (_currentPosition.x <= -_length * 0.75f)
        {
            SetDisplayMessage(_defaultMessage);
            if (CheckTempMessage())
            {
                SetDisplayMessage(_tempMessage);
                ResetTempMessage();
            }

            // Get the new length of the object so we can position it correctly
            if (_textObject.sizeDelta.x != 0)
                _length = _textObject.sizeDelta.x;

            // Change text object position to the right side using current length
            _currentPosition = new Vector2(_length * 0.75f, _currentPosition.y);
        }
    }

    public void SetTurnIndicatorMessage(MessageType damageType = MessageType.NONE)
    {
        switch (damageType)
        {
            case (MessageType.DMG_CONVENTIONAL):
                UIManager.GetTurnIndicator().SetTempMessage("--- WARNING::UNAUTHORIZED USE OF FORCE DETECTED ---", 1);
                break;

            case (MessageType.DMG_EXPLOSIVE):
                UIManager.GetTurnIndicator().SetTempMessage("--- DANGER::HIGH EXPLOSIVE MUNITIONS DETECTED ---", 2);
                break;

            case (MessageType.PV_DEATH):
                UIManager.GetTurnIndicator().SetTempMessage("--- HOSTILE TARGET EXPIRED::THANK YOU FOR YOUR COMPLIANCE ---", 3);
                break;

            case (MessageType.ACS_DEATH):
                int randomInt = Random.Range(0, 9999);
                string randomName = string.Format("ACSPU_{0}@ACS.LIMURA.GOV", randomInt);
                UIManager.GetTurnIndicator().SetTempMessage(string.Format("--- UNABLE TO RESOLVE HOST {0}::PLEASE CONTACT AN ADMINISTRATOR ---", randomName), 4);
                break;
        }
    }

    private void SetTempMessage(string message, int priorty)
    {
        if (_tempMessage == null)
        {
            _tempMessage = message;
        }
        else if (priorty > _messagePriority)
        {
            _messagePriority = priorty;
            _tempMessage = message;
        }
    }

    public bool CheckTempMessage()
    { return _tempMessage != null; }

    public void ResetTempMessage()
    {
        _messagePriority = 0;
        _tempMessage = null; 
    }

    private void SetDisplayMessage(string value)
    { _textMeshComponent.text = value; }
}

public enum MessageType { NONE, DMG_CONVENTIONAL, DMG_EXPLOSIVE, PV_DEATH, ACS_DEATH };