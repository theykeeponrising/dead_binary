using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance = null;

    public StatusMenuUI statusMenuUI;
    public ActionPanelScript actionPanel;
    public EndTurnPanelScript endTurnPanel;
    public InCombatPlayerActionUI inCombatPlayerActionUI;
    public UseItemPanelScript useItemPanel;
    public InfoPanelScript infoPanel;

    private void Awake()
    {
        Instance = this;

        statusMenuUI = GetComponentInChildren<StatusMenuUI>();
        actionPanel = GetComponentInChildren<ActionPanelScript>();
        endTurnPanel = GetComponentInChildren<EndTurnPanelScript>();
        inCombatPlayerActionUI = GetComponentInChildren<InCombatPlayerActionUI>();
        useItemPanel = GetComponentInChildren<UseItemPanelScript>();
        infoPanel = GetComponentInChildren<InfoPanelScript>();
    }

}
