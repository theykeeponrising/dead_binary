using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogPanel : MonoBehaviour
{
    private Canvas _canvas;
    private AudioSource _audioSource;
    private Unit _unit;

    [SerializeField] private Image[] _dialogFrames;
    private TextMeshProUGUI _dialogNameUI;
    private TextMeshProUGUI _dialogTextUI;
    private Image _dialogPortraitSprite;
    private Image _dialogPortraitOutline;

    private string _unitDialogStored;
    private bool _typing;
    private readonly Timer _dialogSpeed = new(0.04f);
    private readonly Timer _activityTimer = new(3f);
    private readonly float _idleWaitTime = 3f;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        _audioSource = UIManager.AudioSource;

        _dialogNameUI = transform.Find("LabelName").GetComponent<TextMeshProUGUI>();
        _dialogTextUI = transform.Find("LabelDialog").GetComponent<TextMeshProUGUI>();
        _dialogPortraitSprite = transform.Find("Portrait").Find("Sprite").GetComponent<Image>();
        _dialogPortraitOutline = transform.Find("Portrait").Find("Outline").GetComponent<Image>();
    }

    private void OnDisable()
    {
        _dialogTextUI.text = "";

    }

    private void Update()
    {
        PerformUnitSpeaking();
        HideIdleDialog();
    }

    public void UnitTalk(Unit unit, string unitDialog)
    {
        _unit = unit;
        _unitDialogStored = unitDialog;
        _typing = true;
        _canvas.enabled = true;

        _dialogTextUI.text = "";
        _dialogSpeed.SetTimer(0.04f);

        SetUnitSpeaking();
        SetColor();
    }

    public void SetUnitSpeaking()
    {
        _dialogNameUI.text = _unit.Attributes.Name;
        _dialogPortraitSprite.sprite = _unit.Attributes.UnitPortrait.GetSprites()[0].sprite;
        _dialogPortraitOutline.sprite = _unit.Attributes.UnitPortrait.GetSprites()[1].sprite;
    }

    private void SetColor()
    {
        Color factionColor = _unit.Attributes.Faction.FactionColor;

        foreach (Image frame in _dialogFrames)
            frame.color = factionColor;

        _dialogNameUI.color = factionColor;
        _dialogTextUI.color = factionColor;
        _dialogPortraitOutline.color = factionColor;
    }

    private void HideIdleDialog()
    {
        if (_typing)
            return;

        if (!_activityTimer.CheckTimer())
            return;

        _canvas.enabled = false;
    }

    private void PerformUnitSpeaking()
    {
        if (!_typing)
            return;

        // Ensures dialog is printed as desired speed
        if (!_dialogSpeed.CheckContinuousTimer())
            return;

        PlayDialogSound();
        PrintDialogText();
        CheckDialogFinished();
    }

    private void PlayDialogSound()
    {
        AudioClip audioClip = AudioManager.GetSound(_unit.Attributes.UnitVoice);
        _audioSource.pitch = _unit.Attributes.UnitVoicePitch;
        _audioSource.PlayOneShot(audioClip);
    }

    private void PrintDialogText()
    {
        _dialogTextUI.text += _unitDialogStored[_dialogTextUI.text.Length];
    }

    private void CheckDialogFinished()
    {
        if (_dialogTextUI.text.Length == _unitDialogStored.Length)
        {
            _typing = false;
            _activityTimer.SetTimer(_idleWaitTime);
        }
    }
}
