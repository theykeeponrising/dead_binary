using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetButton : ActionButton
{
    InfoPanelScript infoPanel;
    string spritePath;
    Image iconBracket;
    Image iconUnit;

    private void Awake()
    {
        infoPanel = GetComponentInParent<InfoPanelScript>();
        iconBracket = GetComponent<Image>();
        iconUnit = GetComponentsInChildren<Image>()[1];
    }

    public override void LoadResources(string newSpritePath)
    {
        spritePath = newSpritePath;

        iconUnit.sprite = Resources.Load<Sprite>(spritePath);
    }

    public override void BindUnit(Unit unit)
    {
        base.BindUnit(unit);
        button.onClick.AddListener(ButtonPress);
        LoadResources(UnitIcons.GetIcon(boundUnit.attributes.unitIcon));
        SetIconColor();
    }

    void SetIconColor()
    {
        Faction unitFaction = boundUnit.attributes.faction;
        iconBracket.color = FactionColors[unitFaction];
        iconUnit.color = FactionColors[unitFaction];
    }

    public void ShowBracket(bool show)
    {
        iconBracket.enabled = show;
    }

    public override void ButtonPress()
    {
        // On button press, play sound and switch selected unit's target to the bound unit

        ButtonTrigger();
        StateTarget currentState = (StateTarget)GetCurrentState();
        InCombatPlayerAction playerAction = GetPlayerAction();
        currentState.target = boundUnit;
        playerAction.selectedCharacter.GetActor().targetCharacter = boundUnit;
        infoPanel.UpdateTargetButtons();
    }
}
