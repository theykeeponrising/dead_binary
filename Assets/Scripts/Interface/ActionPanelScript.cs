using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ActionPanelScript : MonoBehaviour
{
    [HideInInspector] public InCombatPlayerAction playerAction;
    [HideInInspector] public GameObject panel;
    [HideInInspector] public List<ActionButton> buttons = new List<ActionButton>();
    [HideInInspector] public PlayerTurnState playerTurnState;
    [HideInInspector] public ActionButton buttonPrefab;
    [HideInInspector] public TextMeshProUGUI[] textObjects;

    // ELEMENT LIST
    // 0 - AP Label
    // 1 - AP Value
    // 2 - Ammo Label
    // 3 - Ammo Value

    private void Start()
    {
        playerTurnState = (PlayerTurnState)StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        playerAction = playerTurnState.GetPlayerAction();
        panel = transform.Find("Background").gameObject;
        buttonPrefab = UIManager.Instance.actionButtonPrefab;
        textObjects = GetComponentsInChildren<TextMeshProUGUI>();
    }

    private void OnDisable()
    {
        // When action panel is disabled, destroy all buttons

        foreach (ActionButton button in buttons)
        {
            Destroy(button.gameObject);
        }
        buttons = new List<ActionButton>();
    }

    void BuildActions()
    {
        // Dynamically creates buttons based on what actions the Character can perform
        // Will skip creating buttons for actions that do not use buttons (such as moving)

        Start();

        if (playerTurnState == null || playerAction == null) return;

        if (playerAction.selectedCharacter)
        {
            panel.SetActive(true);

            List<ActionList> actionsList = new List<ActionList>();
            foreach (ActionList characterAction in playerAction.selectedCharacter.GetAvailableActions())
            {
                if (!Action.ActionsDict.ContainsKey(characterAction)) continue;
                if (Action.ActionsDict[characterAction].buttonPath != null)
                {
                    actionsList.Add(characterAction);
                    buttons.Add(Instantiate(buttonPrefab, panel.transform));
                    int index = buttons.Count - 1;
                    string spritePath = Action.ActionsDict[actionsList[index]].buttonPath;

                    buttons[index].LoadResources(spritePath);
                    buttons[index].BindAction(characterAction);
                    buttons[index].SetLabel((index + 1).ToString());
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

    private void Update()
    {
        // Update ammo and ap

        if (playerAction.selectedCharacter != null && textObjects.Length > 0)
        {
            textObjects[1].text = playerAction.selectedCharacter.stats.actionPointsCurrent.ToString();
            textObjects[3].text = playerAction.selectedCharacter.inventory.equippedWeapon.stats.ammoCurrent.ToString();
        }
    }

    public virtual void BindButtons()
    {
        // Binds actions to buttons

        // Build actions list before creating bindings
        BuildActions();

        // Remove existing bindings if any
        foreach (ActionButton button in buttons) button.UnbindButton();

        // Bind buttons to inputs
        foreach (ActionButton button in buttons) button.BindButton(buttons.IndexOf(button));
    }

    public List<ActionButton> GetButtons()
    {
        // Returns all currently-active buttons

        return buttons;
    }
}
