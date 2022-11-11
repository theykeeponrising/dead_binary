using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;


public class InCombatPlayerAction
{
    // Used to manage user inputs
    public PlayerInput playerInput;
    public Unit selectedCharacter;
    public List<Unit> playerUnits;
    public List<Tile> previewPath = new List<Tile>();
    public Tile targetTile;
    public enum ClickAction { select, target }
    public ClickAction clickAction;
    public string clickContext;
    
    private ActionPanelScript actionPanelScript;
    private InventoryPanelScript inventoryPanelScript;
    private InfoPanelScript infoPanelScript;
    public StateMachine<InCombatPlayerAction> stateMachine;
    public LayerMask uiLayermask;
    private PlayerTurnState playerTurnState; 
    InCombatPlayerActionUI playerActionUI;

    public Faction playerFaction = FactionManager.PV;

    private TextMeshProUGUI stateDebugText;

    public void Init(PlayerTurnState playerTurnState) 
    {
        playerInput = new PlayerInput();
        this.playerTurnState = playerTurnState;
    }

    public void Start()
    {
        actionPanelScript = UIManager.GetActionPanel();
        actionPanelScript.gameObject.SetActive(false);
        inventoryPanelScript = UIManager.GetInventoryPanel();
        inventoryPanelScript.gameObject.SetActive(false);
        infoPanelScript = UIManager.GetInfoPanel();
        infoPanelScript.gameObject.SetActive(false);
        playerActionUI = UIManager.GetPlayerAction();
        stateDebugText = UIManager.GetStateDebug();

        playerUnits = Map.FindUnits(FactionManager.PV);
    }

    public void EnablePlayerInput()
    {
        playerInput.Enable();
    }

    public void DisablePlayerInput()
    {
        playerInput.Disable();
    }

    // Update is called once per frame
    public void Update()
    {
        PathPreview();
        //Debug.Log(stateMachine.GetCurrentState().GetType().Name);
        stateDebugText.text = stateMachine.GetCurrentState().GetType().Name;
    }

    public InCombatPlayerActionUI GetPlayerActionUI()
    {
        return playerActionUI;
    }

    public void SetStateMachine(StateMachine<InCombatPlayerAction> stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public void SelectUnit()
    {
        // Default context - select a unit, or deselect if none targeted
        // If unit is selected, send action to the unit along with context (such as attack target)
        RaycastHit hit;
        Ray ray;
        ray = Camera.main.ScreenPointToRay(playerInput.Controls.InputPosition.ReadValue<Vector2>());
        Unit targetCharacter = null;
        int layerMask = (1 << LayerMask.NameToLayer("TileMap"));

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~layerMask))
        {
            if (hit.collider.GetComponent<Unit>())
                targetCharacter = hit.collider.GetComponent<Unit>();
        }

        SelectAction(targetCharacter);
    }

    public void SelectNextUnit(bool reverseOrder = false)
    {
        // Default index 0
        int index = 0;

        List<Unit> validUnits = new List<Unit>();
        foreach (Unit unit in playerUnits)
            if (!unit.GetFlag(FlagType.DEAD) && (!unit.HasTurnEnded()))
                validUnits.Add(unit);

        // Get the index of the currently selected unit (if any)
        if (selectedCharacter) index = validUnits.IndexOf(selectedCharacter);

        // If we have reached the end of the list, start at position 0 again
        if (reverseOrder)
            index = (index - 1 >= 0) ? index - 1 : validUnits.Count - 1;
        else
            index = (index + 1 < validUnits.Count) ? index + 1 : 0;

        // If we are back to the same unit, do nothing
        if (validUnits.Count < 1 || validUnits[index] == selectedCharacter)
            return;

        // Select the next unit
        SelectAction(validUnits[index]);
    }

    public void SelectRemainingUnit()
    {
        // Selects a remaining player unit (meaning unit that still has AP)

        if (CheckTurnEnd()) return;

        foreach (Unit unit in playerUnits)
        {
            if (unit.GetFlag(FlagType.DEAD) || unit.HasTurnEnded()) continue;
            if (selectedCharacter == unit) return;
            SelectAction(unit);
            return;
        }
    }

    public UnitAction GetBindings(int index)
    {
        // Returns which action should be bound to which action button index

        if (!selectedCharacter)
            return null;

        List<UnitAction> actionsList = new List<UnitAction>();
        foreach (UnitAction characterAction in selectedCharacter.GetUnitActions())
        {
            if (characterAction.HasSprite())
                actionsList.Add(characterAction);
        }

        if (UIManager.GetInventoryPanel().gameObject.activeSelf)
        {
            foreach (Item item in selectedCharacter.GetItems())
            {
                if (item.itemAction.HasSprite())
                    actionsList.Add(item.itemAction);
            }
        }

        if (index > actionsList.Count)
            return null;
        return actionsList[index-1];
    }

    public int GetIndex(UnitAction action)
    {
        // Returns which index an action should be bound at

        if (!selectedCharacter)
            return 0;

        List<UnitAction> actionsList = new List<UnitAction>();
        foreach (UnitAction characterAction in selectedCharacter.GetUnitActions())
        {
            if (characterAction.HasSprite())
                actionsList.Add(characterAction);
        }

        if (UIManager.GetInventoryPanel().gameObject.activeSelf)
        {
            foreach (Item item in selectedCharacter.GetItems())
            {
                if (item.itemAction.HasSprite())
                    actionsList.Add(item.itemAction);
            }
        }

        if (!actionsList.Contains(action))
            return 0;

        return actionsList.IndexOf(action) + 1;
    }

    public void SelectAction(Unit targetCharacter = null)
    {
        // Select action, character selected, previous selection
        // Change character selection

        //Can't select enemy units or dead units.
        if (targetCharacter && (targetCharacter.GetFlag(FlagType.DEAD) || targetCharacter.attributes.faction != playerFaction)) return;

        // Clears current action bar
        actionPanelScript.gameObject.SetActive(false);
        inventoryPanelScript.gameObject.SetActive(false);

        // Deselects existing character if any
        if (targetCharacter)
        {
            if (selectedCharacter)
                selectedCharacter.GetActor().SelectUnit(SelectionType.DESELECT);

            selectedCharacter = targetCharacter;
            selectedCharacter.GetActor().SelectUnit(SelectionType.SELECT);
            selectedCharacter.GetActor().SetWaiting(false);
            Camera.main.GetComponent<CameraHandler>().SetCameraSnap(targetCharacter);
        }

        // Select action, character selected, no previous selection
        // Select current character
        else
        {
            if (selectedCharacter)
            {
                selectedCharacter.GetActor().SelectUnit(SelectionType.DESELECT);
                selectedCharacter = null;
                PathPreviewClear();
            }
        }
        
        // Builds action bar if a character is selected
        actionPanelScript.gameObject.SetActive(selectedCharacter != null);
        if (actionPanelScript.gameObject.activeSelf)
            actionPanelScript.BindButtons();
    }

    void PathPreview()
    {
        // Don't show path preview if mouse is over UI element
        if (stateMachine.GetCurrentState().IsPointerOverUIElement(this))
        {
            PathPreviewClear();
            return;
        }

        // Don't show path preview if we are using an item
        if (stateMachine.GetCurrentState().GetType() == typeof(StateUseItem))
        { 
            PathPreviewClear();
            return;
        }

        // Don't show if we are not currently using the main camera
        if (CameraHandler.ActiveCamera != Camera.main)
            return;

        // Previews move path on mouse over
        if (selectedCharacter && !selectedCharacter.GetActor().IsActing() && targetTile)
        {
           //if(stateMachine.GetCurrentState().GetType() 
            //    == typeof(SelectedStates.ChoosingMoveDestination))
            {
                PathPreviewClear();
                previewPath = selectedCharacter.currentTile.GetMovementCost(targetTile, selectedCharacter.stats.movement);

                // If target tile has an object on it, can't move there
                if (targetTile.Occupant) previewPath = null;

                if (previewPath != null)
                {
                    previewPath.Add(selectedCharacter.currentTile);
                    if (previewPath.Count > 1)
                        foreach (Tile tile in previewPath)
                            if (tile != selectedCharacter.currentTile)
                                tile.HighlightTile(TileHighlightType.PREVIEW, true);
                }       
            }
        }
    }

    public void PathPreviewClear()
    {
        // Clears currently displayed path preview
        
        if (previewPath != null)
            foreach (Tile tile in previewPath)
                tile.HighlightTile(showHighlight: false);
    }

    public void StartTurn()
    {
        // Start player's next turn

        foreach (Unit unit in playerUnits)
        {
            unit.OnTurnStart();
        }
    }

    public void EndTurn()
    {
        // Ends player's current turn

        // TO DO -- Any end of turn effects, and transfer to AI
        SelectAction(null);
        this.playerTurnState.EndTurn();
    }

    public bool CheckTurnEnd()
    {
        // Checks if all player units have exhausted their turn
        // Returns False if any units can still perform actions

        foreach (Unit unit in playerUnits)
            if (!unit.HasTurnEnded() && !unit.GetFlag(FlagType.DEAD))
                return false;
        return true;
    }
}
