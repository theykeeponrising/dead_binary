using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClickHandler : MonoBehaviour
{
    public Character selectedCharacter;
    public List<Tile> previewPath = new List<Tile>();
    public Tile targetTile;
    public enum ClickAction { select, target }
    public ClickAction clickAction;
    public string clickContext;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
            KeyPress();
        if (Input.GetMouseButtonDown(1))
            RightClick();
        if (Input.GetMouseButtonDown(0))
            LeftClick();

        PathPreview();
    }


    void LeftClick()
    {
        // Changes selection to clicked unit
        // No target will clear selection

        RaycastHit hit;
        Ray ray;
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Character targetCharacter = null;

        if (Physics.Raycast(ray, out hit))
            if (hit.collider.GetComponent<Character>())
                targetCharacter = hit.collider.GetComponent<Character>();

        if (clickAction == ClickAction.select)
            SelectAction(targetCharacter);
        else if (clickAction == ClickAction.target)
        {
            if (targetCharacter)
            {
                selectedCharacter.SetTarget(targetCharacter, clickContext);
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

    void RightClick()
    {
        // Orders target to move on right-click
        if (selectedCharacter)
        {
            RaycastHit hit;
            Ray ray;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.GetComponent<Tile>())
                {
                    if (previewPath != null)
                        foreach (Tile tile in previewPath)
                            tile.Highlighted(false);
                    selectedCharacter.SetTile(hit.collider.GetComponent<Tile>());
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

    void KeyPress()
    {
        // Sends inputs to selected character.

        foreach (KeyCode keycode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKey(keycode))
            {
                if (selectedCharacter)
                    selectedCharacter.KeyPress(keycode, this);
                return;
            }
        }
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
        if (previewPath != null)
            foreach (Tile tile in previewPath)
                tile.Highlighted(false);
    }
}
