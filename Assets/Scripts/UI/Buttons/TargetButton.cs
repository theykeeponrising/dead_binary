using UnityEngine;
using UnityEngine.UI;

public class TargetButton : ActionButton
{
    private InfoPanelScript _infoPanel;
    private string _spritePath;
    private Image _iconBracket;
    private Image _iconUnit;

    protected override void Awake()
    {
        _audioSource = UIManager.AudioSource;
        _button = GetComponentInChildren<Button>();

        _infoPanel = GetComponentInParent<InfoPanelScript>();
        _iconBracket = GetComponent<Image>();
        _iconUnit = GetComponentsInChildren<Image>()[1];
    }

    public override void LoadResources(string newSpritePath)
    {
        _spritePath = newSpritePath;
        _iconUnit.sprite = Resources.Load<Sprite>(_spritePath);
    }

    protected override void CheckRequirements()
    {
        // TO DO -- Add line of sight requirements here
    }

    public override void BindUnit(Unit unit)
    {
        base.BindUnit(unit);
        _button.onClick.AddListener(ButtonPress);
        LoadResources(UnitIcons.GetIcon(BoundUnit.Attributes.UnitIcon));
        SetIconColor();
    }

    private void SetIconColor()
    {
        Faction unitFaction = BoundUnit.Attributes.Faction;
        _iconBracket.color = unitFaction.FactionColor;
        _iconUnit.color = unitFaction.FactionColor;
    }

    public void ShowBracket(bool show)
    {
        _iconBracket.enabled = show;
    }

    protected override void ButtonPress()
    {
        // On button press, play sound and switch selected unit's target to the bound unit

        ButtonTrigger();
        StateTarget currentState = (StateTarget)GetCurrentState();
        InCombatPlayerAction playerAction = GetPlayerAction();
        currentState.ChangeTarget(playerAction, BoundUnit);
        _infoPanel.UpdateTargetButtons();
    }
}
