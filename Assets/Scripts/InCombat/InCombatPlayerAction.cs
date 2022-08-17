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
    public Character selectedCharacter;
    public Character[] allCharacters; // TO DO -- Characters should be dynamically added to this list
    public List<Tile> previewPath = new List<Tile>();
    public Tile targetTile;
    public enum ClickAction { select, target }
    public ClickAction clickAction;
    public string clickContext;
    private ActionPanelScript actionPanelScript;
    public StateMachine<InCombatPlayerAction> stateMachine;
    public LayerMask uiLayermask;
    InCombatPlayerActionUI playerActionUI;

    [Tooltip("The object to float above the Target's head.")]
    public GameObject selectorBall;

    public void Init() 
    {
        playerInput = new PlayerInput();
    }

    public void Start()
    {
        stateMachine = new StateMachine<InCombatPlayerAction>();
        stateMachine.Configure(this, new SelectedStates.NoTargetSelected(stateMachine));

        actionPanelScript = GameObject.FindGameObjectWithTag("ActionPanel").GetComponent<ActionPanelScript>();
        actionPanelScript.gameObject.SetActive(false);

        playerActionUI = GameObject.FindGameObjectWithTag("InCombatPlayerActionUI").GetComponent<InCombatPlayerActionUI>();
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
    void Update()
    {
        PathPreview();
    }

    public InCombatPlayerActionUI GetPlayerActionUI()
    {
        return playerActionUI;
    }

    public void SelectUnit()
    {
        // Default context - select a unit, or deselect if none targeted
        // If unit is selected, send action to the unit along with context (such as attack target)
        RaycastHit hit;
        Ray ray;
        ray = Camera.main.ScreenPointToRay(playerInput.Controls.InputPosition.ReadValue<Vector2>());
        Character targetCharacter = null;
        int layerMask = (1 << LayerMask.NameToLayer("TileMap"));

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~layerMask))
        {
            if (hit.collider.GetComponent<Character>())
                targetCharacter = hit.collider.GetComponent<Character>();
        }

        SelectAction(targetCharacter);        
    }

    public Actions.ActionsList GetBindings(int index)
    {
        // Returns which action should be bound to which action button index
        if (!selectedCharacter)
            return 0;

        List<Actions.ActionsList> actionsList = new List<Actions.ActionsList>();
        foreach (Actions.ActionsList characterAction in selectedCharacter.availableActions)
            if (Actions.ActionsDict[characterAction].buttonPath != null)
                actionsList.Add(characterAction);

        if (index > actionsList.Count)
            return 0;
        return actionsList[index-1];
    }


    public void MoveCharacter()
    {
        // Orders target to move on right-click
        if (selectedCharacter)
        {
            //if (stateMachine.GetCurrentState().GetType() 
            //    == typeof(SelectedStates.ChoosingMoveDestination))
            {
                RaycastHit hit;
                Ray ray;
                ray = Camera.main.ScreenPointToRay(playerInput.Controls.InputPosition.ReadValue<Vector2>());
                int layerMask = (1 << LayerMask.NameToLayer("TileMap"));

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {
                    if (hit.collider.GetComponent<Tile>())
                    {
                        selectedCharacter.ProcessAction(Actions.action_move, contextTile: hit.collider.GetComponent<Tile>(), contextPath: previewPath);
                    }
                }
            }
        }
    }

    void SelectAction(Character targetCharacter)
    {
        // Select action, character selected, previous selection
        // Change character selection

        // Clears current action bar
        actionPanelScript.gameObject.SetActive(false);

        // Deselects existing character if any
        if (targetCharacter)
        {
            if (selectedCharacter)
                selectedCharacter.SelectUnit(false);

            selectedCharacter = targetCharacter;
            selectedCharacter.SelectUnit(true);
        }

        // Select action, character selected, no previous selection
        // Select current character
        else
        {
            if (selectedCharacter)
            {
                selectedCharacter.SelectUnit(false);
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

        // Previews move path on mouse over
        if (selectedCharacter && targetTile)
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

        foreach (Character character in allCharacters)
        {
            character.RefreshActionPoints();
        }
    }

    public void EndTurn()
    {
        // Ends player's current turn

        // TO DO -- Any end of turn effects, and transfer to AI

        StartTurn(); // TEMP
    }
}
