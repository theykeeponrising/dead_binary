using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoPanelScript : MonoBehaviour
{
    Button button;
    InfoData infoData;

    Transform targetContainer;
    List<TargetButton> targetButtons = new List<TargetButton>();
    public TargetButton targetButtonPrefab;

    public class InfoData
    {
        RectTransform container;
        TextMeshProUGUI[] infoText;// = new TextMeshProUGUI[] { };

        public string actionName { set { infoText[0].text = string.Format(">> {0} <<", value); ; } }
        public string description { set { infoText[1].text = value; ; } }
        public string hitValue { set { infoText[2].text = value; ; } }
        public string hitLabel { set { infoText[3].text = value; ; } }
        public string damageValue { set { infoText[4].text = value; ; } }
        public string damageLabel { set { infoText[5].text = value; ; } }

        public InfoData(RectTransform container)
        {
            this.container = container;
            infoText = container.GetComponentsInChildren<TextMeshProUGUI>();
        }
    }

    void Awake()
    {
        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(ButtonPress);

        infoData = new InfoData(GetComponent<RectTransform>());
        targetContainer = transform.Find("TargetContainer");
    }

    private void OnDisable()
    {
        DestroyTargetButtons();
    }

    public void UpdateAction(UnitAction unitAction)
    {
        if (infoData == null) infoData = new InfoData(GetComponentInChildren<RectTransform>());
        infoData.actionName = unitAction.actionName;
        infoData.description = unitAction.actionDescription;

        //infoText[0].text = string.Format(">> {0} <<", actionName);
        //infoText[1].text = actionDesc;
    }

    public void UpdateDamage(int hpAmount)
    {
        // Updates damage value displayed

        // If we are not doing damage, set the damage value and label to blank
        if (hpAmount == 0)
        {
            //infoText[4].text = "";
            //infoText[5].text = "";
            return;
        }
        
        // If damage value is negative, we are healing
        if (hpAmount < 0)
            infoData.damageLabel = "Damage";
        else if (hpAmount > 0)
            infoData.damageLabel = "Healed";


        // Show damage or heal value without negative
        infoData.damageValue = Mathf.Abs(hpAmount).ToString();
    }

    public void UpdateHit(float hitChance)
    {
        // Updates hit chance value displayed

        // If hit chance is less than zero, set the hit value and label to blank
        if (hitChance < 0)
        {
            infoData.hitValue = "";
            infoData.hitLabel = "";
            return;
        }

        string displayText = string.Format("{0}%", (hitChance * 100).ToString("0"));
        infoData.hitValue = "to Hit";
        infoData.hitLabel = displayText;
    }

    public void ButtonPress()
    {
        // On button press, play sound and execute indexed action from the state

        PlayerTurnState playerTurnState = (PlayerTurnState)StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        InCombatPlayerAction playerAction = playerTurnState.GetPlayerAction();
        FiniteState<InCombatPlayerAction> state = playerAction.stateMachine.GetCurrentState();
        state.InputSpacebar(playerAction);
    }

    public void CreateTargetButtons(List<Unit> units)
    {
        DestroyTargetButtons();
        foreach (Unit unit in units)
        {
            TargetButton newButton = Instantiate(targetButtonPrefab, targetContainer);
            newButton.BindUnit(unit);
            targetButtons.Add(newButton);
            int index = targetButtons.IndexOf(newButton);

            if (index > 0)
            {
                Vector3 offset = new Vector3(50 * index, 0, 0);
                targetButtons[index].transform.position += offset;
            }
        }

        // Resize action panel to fit number of buttons
        float height = 45;
        float width = 50 * targetButtons.Count;
        targetContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        UpdateTargetButtons();
    }

    public void UpdateTargetButtons()
    {
        PlayerTurnState playerTurnState = (PlayerTurnState)StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        InCombatPlayerAction playerAction = playerTurnState.GetPlayerAction();

        foreach (TargetButton button in targetButtons)
        {
            button.ShowBracket(playerAction.selectedCharacter.GetActor().targetCharacter == button.boundUnit);
        }
    }

    void DestroyTargetButtons()
    {
        // Destroy existing buttons
        if (targetButtons.Count > 0)
            foreach (TargetButton button in targetButtons)
            {
                Destroy(button.gameObject);
            }
        targetButtons = new List<TargetButton>();
    }
}
