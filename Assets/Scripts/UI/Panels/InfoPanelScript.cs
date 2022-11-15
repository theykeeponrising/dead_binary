using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoPanelScript : MonoBehaviour
{
    private AudioSource _audioSource;
    private Button _button;
    private InfoData _infoData;

    private Transform _targetContainer;
    private List<TargetButton> _targetButtons = new ();
    [SerializeField] private TargetButton _targetButtonPrefab;

    public class InfoData
    {
        RectTransform container;
        TextMeshProUGUI[] infoText;// = new TextMeshProUGUI[] { };

        public string actionName { set { infoText[0].text = string.Format(">> {0} <<", value); ; } }
        public string description { set { infoText[1].text = value; ; } }
        public string hitValue { set { infoText[2].text = value; ; } }
        public string hitLabel { set { infoText[3].text = value; ; } }
        public string damageValue { set { infoText[4].text = value; ; } }
        public string damageLabel { set { infoText[5].text = value; ; } }

        public InfoData(RectTransform container)
        {
            this.container = container;
            infoText = container.GetComponentsInChildren<TextMeshProUGUI>();
        }
    }

    void Awake()
    {
        _audioSource = GetComponentInParent<AudioSource>();
        _button = GetComponentInChildren<Button>();
        _button.onClick.AddListener(ButtonPress);

        _infoData = new InfoData(GetComponent<RectTransform>());
        _targetContainer = transform.Find("TargetContainer");
    }

    private void OnDisable()
    {
        DestroyTargetButtons();
    }

    public void UpdateAction(UnitAction unitAction)
    {
        if (_infoData == null) _infoData = new InfoData(GetComponentInChildren<RectTransform>());
        _infoData.actionName = unitAction.actionName;
        _infoData.description = unitAction.actionDescription;

        //infoText[0].text = string.Format(">> {0} <<", actionName);
        //infoText[1].text = actionDesc;
    }

    public void UpdateDamage(int hpAmount)
    {
        // Updates damage value displayed

        // If we are not doing damage, set the damage value and label to blank
        if (hpAmount == 0)
        {
            //infoText[4].text = "";
            //infoText[5].text = "";
            return;
        }
        
        // If damage value is negative, we are healing
        if (hpAmount < 0)
            _infoData.damageLabel = "Damage";
        else if (hpAmount > 0)
            _infoData.damageLabel = "Healed";


        // Show damage or heal value without negative
        _infoData.damageValue = Mathf.Abs(hpAmount).ToString();
    }

    public void UpdateHit(float hitChance)
    {
        // Updates hit chance value displayed

        // If hit chance is less than zero, set the hit value and label to blank
        if (hitChance < 0)
        {
            _infoData.hitValue = "";
            _infoData.hitLabel = "";
            return;
        }

        string displayText = string.Format("{0}%", (hitChance * 100).ToString("0"));
        _infoData.hitValue = "to Hit";
        _infoData.hitLabel = displayText;
    }

    public void ButtonPress()
    {
        // On button press, play sound and execute indexed action from the state

        PlayerTurnState playerTurnState = (PlayerTurnState)StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        InCombatPlayerAction playerAction = playerTurnState.GetPlayerAction();
        FiniteState<InCombatPlayerAction> state = playerAction.stateMachine.GetCurrentState();
        state.InputSpacebar(playerAction);
    }

    public void CreateTargetButtons(List<Unit> units)
    {
        DestroyTargetButtons();
        foreach (Unit unit in units)
        {
            TargetButton newButton = Instantiate(_targetButtonPrefab, _targetContainer);
            newButton.BindUnit(unit);
            _targetButtons.Add(newButton);
            int index = _targetButtons.IndexOf(newButton);

            if (index > 0)
            {
                Vector3 offset = new Vector3(50 * index, 0, 0);
                _targetButtons[index].transform.position += offset;
            }
        }

        // Resize action panel to fit number of buttons
        float height = 45;
        float width = 50 * _targetButtons.Count;
        _targetContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);

        if (units.Count <= 0) return;

        UpdateTargetButtons();
        PlayTargetingSFX();
    }

    public void UpdateTargetButtons()
    {
        PlayerTurnState playerTurnState = (PlayerTurnState)StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        InCombatPlayerAction playerAction = playerTurnState.GetPlayerAction();

        foreach (TargetButton button in _targetButtons)
        {
            button.ShowBracket(playerAction.selectedCharacter.GetActor().targetCharacter == button.BoundUnit);
        }

        PlaySwitchTargetSFX();
    }

    void PlayTargetingSFX()
    {
        // Targeting sound played when targets are initially revealed

        AudioClip audioClip = AudioManager.GetSound(InterfaceType.TARGETING);
        _audioSource.Stop();
        _audioSource.PlayOneShot(audioClip);
    }

    void PlaySwitchTargetSFX()
    {
        // Switch target sound played when target is changing

        AudioClip audioClip = AudioManager.GetSound(InterfaceType.SWITCH_TARGET);
        _audioSource.Stop();
        _audioSource.PlayOneShot(audioClip);
    }

    void DestroyTargetButtons()
    {
        // Destroy existing buttons
        if (_targetButtons.Count > 0)
            foreach (TargetButton button in _targetButtons)
            {
                Destroy(button.gameObject);
            }
        _targetButtons = new List<TargetButton>();
    }
}
