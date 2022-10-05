using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    static StatusMenuUI statusMenuUI;
    static ActionPanelScript actionPanel;
    static InCombatPlayerActionUI inCombatPlayerActionUI;
    static InventoryPanelScript inventoryPanel;
    static InfoPanelScript infoPanel;

    private void Awake()
    {
        Instance = this;
    }

    public static InfoPanelScript GetInfoPanel()
    {
        if (!infoPanel) infoPanel = Instance.GetComponentInChildren<InfoPanelScript>();
        return infoPanel;
    }

    public static StatusMenuUI GetStatusMenu()
    {
        if (!statusMenuUI) statusMenuUI = Instance.GetComponentInChildren<StatusMenuUI>();
        return statusMenuUI;
    }

    public static ActionPanelScript GetActionPanel()
    {
        if (!actionPanel) actionPanel = Instance.GetComponentInChildren<ActionPanelScript>();
        return actionPanel;
    }

    public static InCombatPlayerActionUI GetPlayerAction()
    {
        if (!inCombatPlayerActionUI) inCombatPlayerActionUI = Instance.GetComponentInChildren<InCombatPlayerActionUI>();
        return inCombatPlayerActionUI;
    }

    public static InventoryPanelScript GetInventoryPanel()
    {
        if (!inventoryPanel) inventoryPanel = Instance.GetComponentInChildren<InventoryPanelScript>();
        return inventoryPanel;
    }
}
