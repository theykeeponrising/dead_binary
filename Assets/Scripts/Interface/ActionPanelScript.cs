using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionPanelScript : MonoBehaviour
{
    [SerializeField] private InCombatPlayerAction playerAction;
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI apTextBox;
    [SerializeField] private TextMeshProUGUI ammoTextBox;
    [SerializeField] private List<Button> buttons;
    PlayerTurnState playerTurnState;
    public Button buttonPrefab; // TO DO -- Alternative to using inspector prefab

    private void Start()
    {
        playerTurnState = (PlayerTurnState)StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        playerAction = playerTurnState.GetPlayerAction();
    }

    private void OnDisable()
    {
        // When action panel is disabled, destroy all buttons

        foreach (Button button in buttons)
        {
            Destroy(button.gameObject);
        }
        buttons = new List<Button>();
    }

    void BuildActions()
    {
        // Dynamically creates buttons based on what actions the Character can perform
        // Will skip creating buttons for actions that do not use buttons (such as moving)

        playerTurnState = (PlayerTurnState)StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        playerAction = playerTurnState.GetPlayerAction();

        if (playerTurnState == null || playerAction == null) return;

        if (playerAction.selectedCharacter)
        {
            panel.SetActive(true);

            List<Actions.ActionsList> actionsList = new List<Actions.ActionsList>();
            foreach (Actions.ActionsList characterAction in playerAction.selectedCharacter.GetAvailableActions())
            {
                if (!Actions.ActionsDict.ContainsKey(characterAction)) continue;
                if (Actions.ActionsDict[characterAction].buttonPath != null)
                {
                    actionsList.Add(characterAction);
                    buttons.Add(Instantiate(buttonPrefab, panel.transform));
                    int index = buttons.Count - 1;
                    string spritePath = Actions.ActionsDict[actionsList[index]].buttonPath;

                    buttons[index].GetComponent<ActionButton>().LoadResources(spritePath);
                    buttons[index].GetComponentInChildren<TextMeshProUGUI>().text = (index + 1).ToString();
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

        if (playerAction.selectedCharacter != null)
        {
            apTextBox.text = playerAction.selectedCharacter.stats.actionPointsCurrent.ToString();
            ammoTextBox.text = playerAction.selectedCharacter.inventory.equippedWeapon.stats.ammoCurrent.ToString();
        }
    }

    public void BindButtons()
    {
        // Binds actions to buttons

        // Build actions list before creating bindings
        BuildActions();

        // Remove existing bindings if any
        foreach (Button button in buttons) button.GetComponent<ActionButton>().UnbindButton();

        // Bind buttons to inputs
        foreach (Button button in buttons) button.GetComponent<ActionButton>().BindButton(buttons.IndexOf(button));
    }
}
