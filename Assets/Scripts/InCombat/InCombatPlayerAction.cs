using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;


public class InCombatPlayerAction
{
    // Used to manage user inputs
    public PlayerInput playerInput;
    public Unit selectedCharacter;
    public List<Unit> allCharacters; // TO DO -- Characters should be dynamically added to this list
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

    public Faction playerFaction = Faction.Good;

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

        // Add characters to allCharacters list
        allCharacters = new List<Unit>();
        GameObject[] characterGOs = GameObject.FindGameObjectsWithTag("Character");
        foreach (GameObject go in characterGOs) allCharacters.Add(go.GetComponent<Unit>());

        stateDebugText = GameObject.Find("StateDebugText").GetComponent<TextMeshProUGUI>();
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
                selectedCharacter.GetActor().SelectUnit(false);

            selectedCharacter = targetCharacter;
            selectedCharacter.GetActor().SelectUnit(true);
        }

        // Select action, character selected, no previous selection
        // Select current character
        else
        {
            if (selectedCharacter)
            {
                selectedCharacter.GetActor().SelectUnit(false);
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

        // Don't show if we are not currently using the main camera
        if (Camera.current != Camera.main)
            return;

        // Previews move path on mouse over
        if (selectedCharacter && !selectedCharacter.GetActor().IsActing() && targetTile)
        {
           //if(stateMachine.GetCurrentState().GetType() 
            //    == typeof(SelectedStates.ChoosingMoveDestination))
            {
                PathPreviewClear();
                previewPath = selectedCharacter.currentTile.FindCost(targetTile, selectedCharacter.stats.movement);

                // If target tile has an object on it, can't move there
                if (targetTile.occupant) previewPath = null;

                if (previewPath != null)
                {
                    previewPath.Add(selectedCharacter.currentTile);
                    if (previewPath.Count > 1)
                        foreach (Tile tile in previewPath)
                            tile.Highlighted(true, "preview");
                }       
            }
        }
    }

    public void PathPreviewClear()
    {
        // Clears currently displayed path preview
        
        if (previewPath != null)
            foreach (Tile tile in previewPath)
                tile.Highlighted(false);
    }

    public void StartTurn()
    {
        // Start player's next turn

        Debug.Log("Starting player turn");

        foreach (Unit character in allCharacters)
        {
            character.RefreshActionPoints();
        }
    }

    public void EndTurn()
    {
        // Ends player's current turn

        // TO DO -- Any end of turn effects, and transfer to AI
        SelectAction(null);
        this.playerTurnState.EndTurn();
    }

    public bool CheckTurnOver()
    {
        // Player turn is over if all action points were used

        bool turnOver = true;

        foreach (Unit character in allCharacters)
        {
            if (character.attributes.faction == playerFaction && character.stats.actionPointsCurrent != 0) turnOver = false;
        }

        return turnOver;
    }
}
