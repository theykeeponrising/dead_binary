using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class InCombatPlayerAction : MonoBehaviour 
{
    // Used to manage user inputs
    public PlayerInput playerInput;
    public Character selectedCharacter;
    public List<Tile> previewPath = new List<Tile>();
    public Tile targetTile;
    public enum ClickAction { select, target }
    public ClickAction clickAction;
    public string clickContext;

    private void Awake()
    {
        playerInput = new PlayerInput();
        playerInput.Controls.InputPrimary.performed += _ => SelectUnit();
        playerInput.Controls.InputSecondary.performed += _ => MoveCharacter();
        //playerInput.Controls.AnyKey.performed += _ => KeyPress(playerInput.Controls.AnyKey); // For if we want any "PRESS ANY KEY" moments
        playerInput.Controls.ActionButton_1.performed += _ => KeyPress(playerInput.Controls, playerInput.Controls.ActionButton_1);
        playerInput.Controls.ActionButton_2.performed += _ => KeyPress(playerInput.Controls, playerInput.Controls.ActionButton_2);
        playerInput.Controls.ActionButton_3.performed += _ => KeyPress(playerInput.Controls, playerInput.Controls.ActionButton_3);
        playerInput.Controls.ActionButton_4.performed += _ => KeyPress(playerInput.Controls, playerInput.Controls.ActionButton_4);
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

        if (clickAction == ClickAction.select)
            SelectAction(targetCharacter);
        else if (clickAction == ClickAction.target)
        {
            if (targetCharacter)
            {
                selectedCharacter.ProcessAction(Actions.action_shoot, contextCharacter: targetCharacter, contextString: clickContext);
                clickAction = ClickAction.select;
                clickContext = "";
            }
            else
            {
                selectedCharacter.CancelTarget();
                clickAction = ClickAction.select;
                clickContext = "";
            }
        }
    }

    public void MoveCharacter()
    {
        // Orders target to move on right-click
        if (selectedCharacter)
        {
            RaycastHit hit;
            Ray ray;
            ray = Camera.main.ScreenPointToRay(playerInput.Controls.InputPosition.ReadValue<Vector2>());

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.GetComponent<Tile>())
                {
                    selectedCharacter.ProcessAction(Actions.action_move, contextTile: hit.collider.GetComponent<Tile>(), contextPath: previewPath);
                }
            }
        }
    }

    void SelectAction(Character targetCharacter)
    {

        // Select action, character selected, previous selection
        // Change character selection
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
    }

    public bool KeyPress(PlayerInput.ControlsActions controls, InputAction action) 
    {
        bool keyPress = false;
        if (selectedCharacter) {
            keyPress = selectedCharacter.KeyPress(controls, action);
        }
        return keyPress;
    }

    void PathPreview()
    {
        // Previews move path on mouse over

        if (selectedCharacter && targetTile)
        {
            if (selectedCharacter.currentTile)
            {
                PathPreviewClear();
                previewPath = selectedCharacter.currentTile.FindCost(targetTile);
                if (previewPath != null)
                    previewPath.Add(selectedCharacter.currentTile);
                if (previewPath != null)
                    foreach (Tile tile in previewPath)
                        tile.Highlighted(true, "preview");
            }
        }
        else if (previewPath != null)
            foreach (Tile tile in previewPath)
                tile.Highlighted(false);
    }

    void PathPreviewClear()
    {
        // Clears currently displayed path preview
        
        if (previewPath != null)
            foreach (Tile tile in previewPath)
                tile.Highlighted(false);
    }
}
