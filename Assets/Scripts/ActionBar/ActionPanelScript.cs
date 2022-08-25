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
        BuildActions();
    }

    private void OnEnable()
    {
        BuildActions();
    }

    private void OnDisable()
    {
        ClearActions();
    }

    public void BuildActions()
    {
        // Dynamically creates buttons based on what actions the Character can perform
        // Will skip creating buttons for actions that do not use buttons (such as moving)

        if (playerTurnState == null || playerAction == null) return;

        if (playerAction.selectedCharacter)
        {
            panel.SetActive(true);

            List<Actions.ActionsList> actionsList = new List<Actions.ActionsList>();
            foreach (Actions.ActionsList characterAction in playerAction.selectedCharacter.availableActions)
            {
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

    public void ClearActions()
    {
        // Remove buttons and clean button list

        foreach (Button button in buttons)
        {
            Destroy(button.gameObject);
        }
        buttons = new List<Button>();
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
        // Remove existing bindings if any
        foreach (Button button in buttons)
        {
            button.GetComponent<ActionButton>().button.onClick.RemoveAllListeners();
        }

        // Bind buttons to inputs
        foreach (Button button in buttons)
        {
            int index = buttons.IndexOf(button);
            var state = playerAction.stateMachine.GetCurrentState();
            button.GetComponent<ActionButton>().button.onClick.AddListener(delegate { state.InputActionBtn(playerAction, index + 1); });
        }
    }

    public void ClearBindings()
    {
        // Removes bindings from buttons

        foreach (Button button in buttons)
        {
            button.GetComponent<ActionButton>().button.onClick.RemoveAllListeners();
        }
    }
}
