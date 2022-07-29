using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;


public class InCombatPlayerAction : MonoBehaviour 
{
    // Used to manage user inputs

    public Character selectedCharacter;
    public List<Tile> previewPath = new List<Tile>();
    public Tile targetTile;
    public enum ClickAction { select, target }
    public ClickAction clickAction;
    public string clickContext;

    public LayerMask uiLayermask;
    [SerializeField]
    public ActionPanel actionPanel;
    [Serializable]
    public class ActionPanel
    {
        public GameObject panel;
        public TextMeshProUGUI actionPointsText;
        public Button moveButton;
        public Button shootButton;
    }

    // Update is called once per frame
    void Update()
    {
        PathPreview();

        if (selectedCharacter != null)
        {
            actionPanel.panel.SetActive(true);
            if (actionPanel != null)
            {
                actionPanel.actionPointsText.text =
                    "AP: " + selectedCharacter.stats.actionPointsCurrent.ToString();
            }
        }
        else
        {
            actionPanel.panel.SetActive(false);
        }
    }
    
    public void SelectUnit()
    {
        // Default context - select a unit, or deselect if none targeted
        // If unit is selected, send action to the unit along with context (such as attack target)
        RaycastHit hit;
        Ray ray;
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Character targetCharacter = null;

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Physics.Raycast(ray, out hit))
                if (hit.collider.GetComponent<Character>())
                    targetCharacter = hit.collider.GetComponent<Character>();

            SelectAction(targetCharacter);
        }


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

    public void MoveCharacter()
    {
        // Orders target to move on right-click
        if (selectedCharacter)
        {
            if (selectedCharacter.state.GetType() == typeof(SelectedStates.ChoosingMoveDestination))
            {
                RaycastHit hit;
                Ray ray;
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
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

    public bool Keypress(KeyCode keycode) 
    {
        bool keyPress = false;
        if (selectedCharacter) {
            keyPress = selectedCharacter.KeyPress(keycode);
        }
        return keyPress;
    }

    void PathPreview()
    {
        // Previews move path on mouse over

        if (selectedCharacter && targetTile)
        {
            if (selectedCharacter.state.GetType() == typeof(SelectedStates.ChoosingMoveDestination))
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

    void PathPreviewClear()
    {
        // Clears currently displayed path preview
        
        if (previewPath != null)
            foreach (Tile tile in previewPath)
                tile.Highlighted(false);
    }

    public void SetState_ChooseMoveDestination()
    {
        selectedCharacter.stateMachine.ChangeState(new SelectedStates.ChoosingMoveDestination(selectedCharacter.stateMachine));

        PathPreviewClear();
    }

    public void SetState_ChooseShootTarget()
    {
        selectedCharacter.stateMachine.ChangeState(new SelectedStates.ChoosingShootTarget(selectedCharacter.stateMachine));
    }
}
