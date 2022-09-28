using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoPanelScript : MonoBehaviour
{
    AudioSource audioSource;
    TextMeshProUGUI[] textObjects;
    Button button;

    // ELEMENT LIST
    // 0 - Action name
    // 1 - Description
    // 2 - Hit value
    // 3 - Hit label
    // 4 - Dmg value
    // 5 - Dmg label

    // Start is called before the first frame update
    void Awake()
    {
        audioSource = GetComponentInParent<AudioSource>();
        textObjects = GetComponentsInChildren<TextMeshProUGUI>();
        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(ButtonPress);
    }

    public void UpdateAction(ActionList actionEnum)
    {
        Action action = Action.ActionsDict[actionEnum];
        string actionName = action.aname;
        string actionDesc = action.description;
        textObjects = GetComponentsInChildren<TextMeshProUGUI>();
        textObjects[0].text = string.Format(">> {0} <<", actionName);
        textObjects[1].text = actionDesc;
    }

    public void UpdateDamage(int hpAmount)
    {
        // Updates damage value displayed

        // If we are not doing damage, set the damage value and label to blank
        if (hpAmount == 0)
        {
            textObjects[4].text = "";
            textObjects[5].text = "";
            return;
        }
        
        // If damage value is negative, we are healing
        if (hpAmount < 0)
            textObjects[5].text = "Damage";
        else if (hpAmount > 0)
            textObjects[5].text = "Healed";


        // Show damage or heal value without negative
        textObjects[4].text = Mathf.Abs(hpAmount).ToString();
    }

    public void UpdateHit(float hitChance)
    {
        // Updates hit chance value displayed

        // If hit chance is less than zero, set the hit value and label to blank
        if (hitChance < 0)
        {
            textObjects[2].text = "";
            textObjects[3].text = "";
            return;
        }

        string displayText = string.Format("{0}%", (hitChance * 100).ToString("0"));
        textObjects[2].text = "to Hit";
        textObjects[2].text = displayText;
    }

    public void ButtonPress()
    {
        // On button press, play sound and execute indexed action from the state

        //if (!requirementsMet)
        //return;

        PlayerTurnState playerTurnState = (PlayerTurnState)StateHandler.Instance.GetStateObject(StateHandler.State.PlayerTurnState);
        InCombatPlayerAction playerAction = playerTurnState.GetPlayerAction();
        FiniteState<InCombatPlayerAction> state = playerAction.stateMachine.GetCurrentState();
        state.InputSpacebar(playerAction);
    }

    public void ButtonTrigger()
    {
        // Handle to kick off button trigger effect

        StartCoroutine(ButtonTriggerEffect());
    }

    IEnumerator ButtonTriggerEffect()
    {
        // Simulates the visual look of the action button being clicked
        // For use when action button is triggered via another input

        AudioClip audioClip = AudioManager.Instance.GetInterfaceSound(AudioManager.InterfaceSFX.MOUSE_CLICK, 0);
        audioSource.PlayOneShot(audioClip);

        //currentButtonState = ButtonState.ACTIVE;
        yield return new WaitForSeconds(0.2f);
        //currentButtonState = ButtonState.PASSIVE;
    }
}
