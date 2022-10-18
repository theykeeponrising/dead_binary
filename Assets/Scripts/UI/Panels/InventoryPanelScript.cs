using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryPanelScript : ActionPanelScript
{
    ActionPanelScript actionPanel;
    int offset => actionPanel.GetButtons().Count;

    private void Start()
    {
        playerTurnState = (PlayerTurnState)StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        playerAction = playerTurnState.GetPlayerAction();
        actionPanel = UIManager.GetActionPanel();
        buttonPrefab = actionPanel.buttonPrefab;
        panel = transform.Find("Background").gameObject;
    }

    void BuildActions()
    {
        // Dynamically creates buttons based on what items the Character has
        // Will skip creating buttons for items that do not use buttons (such as moving)

        Start();

        if (playerTurnState == null || playerAction == null) return;

        if (playerAction.selectedCharacter)
        {
            panel.SetActive(true);

            List<UnitAction> actionsList = new List<UnitAction>();
            foreach (Item characterItem in playerAction.selectedCharacter.GetItems())
            {
                if (characterItem.itemAction.HasSprite())
                {
                    actionsList.Add(characterItem.itemAction);
                    buttons.Add(Instantiate(buttonPrefab, panel.transform));
                    int index = buttons.Count - 1;
                    string spritePath = UnitActionSprites.GetSprite(characterItem.itemAction.actionSprite);

                    buttons[index].LoadResources(spritePath);
                    buttons[index].BindAction(characterItem.itemAction);
                    buttons[index].BindItem(characterItem);
                    buttons[index].SetLabel((index + 1 + actionPanel.GetButtons().Count).ToString());
                    buttons[index].gameObject.SetActive(true);

                    if (index > 0)
                    {
                        Vector3 offset = new Vector3(75 * index, 0, 0);
                        buttons[index].transform.position += offset;
                    }
                }
            }

            // Resize action panel to fit number of buttons
            float height = 100f;
            float width = 75 * buttons.Count + 15;
            GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        }
    }

    public override void BindButtons()
    {
        // Binds actions to buttons

        // Build actions list before creating bindings
        BuildActions();

        // Remove existing bindings if any
        foreach (ActionBarButton button in buttons) button.UnbindButton();

        // Bind buttons to inputs
        foreach (ActionBarButton button in buttons) button.BindIndex(buttons.IndexOf(button) + offset);
    }
}
