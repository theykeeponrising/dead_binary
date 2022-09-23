using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance = null;

    public StatusMenuUI statusMenuUI;
    public ActionPanelScript actionPanel;
    public EndTurnPanelScript endTurnPanel;
    public InCombatPlayerActionUI inCombatPlayerActionUI;
    public InventoryPanelScript inventoryPanel;
    public InfoPanelScript infoPanel;
    public ActionButton actionButtonPrefab;

    private void Awake()
    {
        Instance = this;

        statusMenuUI = GetComponentInChildren<StatusMenuUI>();
        actionPanel = GetComponentInChildren<ActionPanelScript>();
        endTurnPanel = GetComponentInChildren<EndTurnPanelScript>();
        inCombatPlayerActionUI = GetComponentInChildren<InCombatPlayerActionUI>();
        inventoryPanel = GetComponentInChildren<InventoryPanelScript>();
        infoPanel = GetComponentInChildren<InfoPanelScript>();
    }

}
