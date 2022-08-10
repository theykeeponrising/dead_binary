using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionPanelScript : MonoBehaviour
{
    [SerializeField] private InCombatPlayerAction player;
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI apTextBox;
    [SerializeField] private TextMeshProUGUI ammoTextBox;
    [SerializeField] private List<Button> buttons;
    public Button buttonPrefab; // TO DO -- Alternative to using inspector prefab

    private void Start()
    {
        // Assign the player.
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<InCombatPlayerAction>();
    }

    private void OnEnable()
    {
        // Dynamically creates buttons based on what actions the Character can perform
        // Will skip creating buttons for actions that do not use buttons (such as moving)

        if (!player)
            return;
       
        if (player.selectedCharacter)
        {
            panel.SetActive(true);

            List<Actions.ActionsList> actionsList = new List<Actions.ActionsList>();
            foreach(Actions.ActionsList characterAction in player.selectedCharacter.availableActions)
            {
                if (Actions.ActionsDict[characterAction].button != null)
                {
                    actionsList.Add(characterAction);
                    buttons.Add(Instantiate(buttonPrefab, panel.transform));
                    int index = buttons.Count - 1;

                    buttons[index].GetComponentsInChildren<Image>()[1].sprite = Actions.ActionsDict[actionsList[index]].button.active;
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

    private void OnDisable()
    {
        // Clean up buttons

        foreach (Button button in buttons)
        {
            Destroy(button.gameObject);
        }
        buttons = new List<Button>();
    }

    private void Update()
    {
        // Update ammo and ap

        apTextBox.text = player.selectedCharacter.stats.actionPointsCurrent.ToString();
        ammoTextBox.text = player.selectedCharacter.inventory.equippedWeapon.stats.ammoCurrent.ToString();
    }
}
