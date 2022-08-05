using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
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


    public LayerMask uiLayermask;
    [SerializeField] public ActionPanel actionPanel;
    [Serializable] public class ActionPanel
    {
        public GameObject panel;
        public TextMeshProUGUI actionPointsText;
        public Button moveButton;
        public Button shootButton;
    }

    public StateMachine<InCombatPlayerAction> stateMachine;
    public TextMeshProUGUI stateText;

    [Tooltip("The object to float above the Target's head.")]
    public GameObject selectorBall;
    private void Awake()
    {
        playerInput = new PlayerInput();

       // playerInput.Controls.InputPrimary.performed += _ => SelectUnit();
       // playerInput.Controls.InputSecondary.performed += _ => MoveCharacter();
        //playerInput.Controls.AnyKey.performed += _ => KeyPress(playerInput.Controls.AnyKey); // For if we want any "PRESS ANY KEY" moments
        playerInput.Controls.ActionButton_1.performed += _ => KeyPress(playerInput.Controls, playerInput.Controls.ActionButton_1);
        playerInput.Controls.ActionButton_2.performed += _ => KeyPress(playerInput.Controls, playerInput.Controls.ActionButton_2);
        playerInput.Controls.ActionButton_3.performed += _ => KeyPress(playerInput.Controls, playerInput.Controls.ActionButton_3);
        playerInput.Controls.ActionButton_4.performed += _ => KeyPress(playerInput.Controls, playerInput.Controls.ActionButton_4);

        
    }
    private void Start()
    {
        stateMachine = new StateMachine<InCombatPlayerAction>();
        stateMachine.Configure(this, new SelectedStates.NoTargetSelected(stateMachine));
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

        if (selectedCharacter != null)
        {
            actionPanel.panel.SetActive(true);
            if (actionPanel != null)
                actionPanel.actionPointsText.text =
                    "AP: " + selectedCharacter.stats.actionPointsCurrent.ToString();
        }
        else
            actionPanel.panel.SetActive(false);

        if (stateMachine != null)
        {
            stateMachine.Update();
            stateText.text = stateMachine.GetCurrentState().StateName;
        }
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

        // ClickActions. - Obsolete?
        {
            /*
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
            */
        }
    }
    

    public void MoveCharacter()
    {
        // Orders target to move on right-click
        if (selectedCharacter)
        {
            if (stateMachine.GetCurrentState().GetType() 
                == typeof(SelectedStates.ChoosingMoveDestination))
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
        {
            if (action == controls.ActionButton_1)
            {
            }

            if (action == controls.ActionButton_2)
            {
            }
        }
        return keyPress;
    }

    public int TriggerButton(int num)
    {
        return num;
    }

    void PathPreview()
    {
        // Previews move path on mouse over
        if (selectedCharacter && targetTile)
        {

            if(stateMachine.GetCurrentState().GetType() 
                == typeof(SelectedStates.ChoosingMoveDestination))
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

                else if (previewPath != null)
                    foreach (Tile tile in previewPath)
                        tile.Highlighted(false);
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
}
