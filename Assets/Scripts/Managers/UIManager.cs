using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    static StatusMenuUI statusMenuUI;
    static ActionPanelScript actionPanel;
    static InCombatPlayerActionUI inCombatPlayerActionUI;
    static InventoryPanelScript inventoryPanel;
    static InfoPanelScript infoPanel;

    public static InfoPanelScript GetInfoPanel()
    {
        if (!infoPanel) infoPanel = GameObject.Find("UI").GetComponentInChildren<InfoPanelScript>();
        return infoPanel;
    }

    public static StatusMenuUI GetStatusMenu()
    {
        if (!statusMenuUI) statusMenuUI = GameObject.Find("UI").GetComponentInChildren<StatusMenuUI>();
        return statusMenuUI;
    }

    public static ActionPanelScript GetActionPanel()
    {
        if (!actionPanel) actionPanel = GameObject.Find("UI").GetComponentInChildren<ActionPanelScript>();
        return actionPanel;
    }

    public static InCombatPlayerActionUI GetPlayerAction()
    {
        if (!inCombatPlayerActionUI) inCombatPlayerActionUI = GameObject.Find("UI").GetComponentInChildren<InCombatPlayerActionUI>();
        return inCombatPlayerActionUI;
    }

    public static InventoryPanelScript GetInventoryPanel()
    {
        if (!inventoryPanel) inventoryPanel = GameObject.Find("UI").GetComponentInChildren<InventoryPanelScript>();
        return inventoryPanel;
    }
}
