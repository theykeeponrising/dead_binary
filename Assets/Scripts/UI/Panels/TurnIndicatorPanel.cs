using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class TurnIndicatorPanel : MonoBehaviour
{
    private Transform _objectsContainer;
    private RectTransform _textObject;
    private TextMeshProUGUI _textMeshComponent;

    private MessageData _nextMessage = Messages.GetMessage(MessageType.DEFAULT);
    private float _length = 765f;
    private float _bufferSize = 0.80f;

    [Tooltip("The speed at which the text scrolls. Lower is faster.")]
    [SerializeField] private float _scrollSpeed = 1f;

    private Vector2 _currentPosition { get { return _textObject.transform.localPosition; } set { _textObject.transform.localPosition = value; } }
    private bool _isVisible => FactionManager.ACS.TurnInProgress;

    private void Start()
    {
        _objectsContainer = transform.GetChild(0);
        _textObject = _objectsContainer.GetComponentInChildren<RectMask2D>().GetComponentsInChildren<RectTransform>()[1];
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
        _objectsContainer.gameObject.SetActive(_isVisible);
    }

    private void MoveTextObject()
    {
        // Move the text object left at the speed value provided
        _currentPosition = Vector2.Lerp(_currentPosition + Vector2.left, _currentPosition, _scrollSpeed);
    }

    private void MoveToStartPosition()
    {
        // When disabled, send the text back to the right
        if (!_isVisible)
        {
            _currentPosition = new Vector2(-_length * _bufferSize, _currentPosition.y);
            return;
        }

        // Reset text back to the right side
        else if (_currentPosition.x <= -_length * _bufferSize)
        {
            SetDisplayedMessage(_nextMessage);
            ClearNextMessage();

            // Get the new length of the object so we can position it correctly
            if (_textObject.sizeDelta.x != 0)
                _length = _textObject.sizeDelta.x;

            // Change text object position to the right side using current length
            _currentPosition = new Vector2(_length * _bufferSize, _currentPosition.y);
        }
    }

    public void SetTurnIndicatorMessage(MessageType messageType = MessageType.DEFAULT)
    {
        MessageData messageData = Messages.GetMessage(messageType);
        SetNextMessage(messageData);
    }

    private void SetNextMessage(MessageData messageData)
    {
        if (_nextMessage.Message == null)
        {
            _nextMessage = messageData;
        }
        else if (messageData.Priority > _nextMessage.Priority)
        {
            _nextMessage = messageData;
        }
    }

    public void ClearNextMessage()
    {
        _nextMessage = Messages.GetMessage(MessageType.DEFAULT);
    }

    private void SetDisplayedMessage(MessageData messageData)
    {
        _textMeshComponent.text = string.Format(">> {0} <<", messageData.Message);
    }
}
