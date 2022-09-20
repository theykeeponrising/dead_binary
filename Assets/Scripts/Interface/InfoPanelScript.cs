using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoPanelScript : MonoBehaviour
{
    TextMeshProUGUI[] textObjects;

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
        textObjects = GetComponentsInChildren<TextMeshProUGUI>();
    }

    public void UpdateAction(ActionList actionEnum)
    {
        Action action = Action.ActionsDict[actionEnum];
        string actionName = action.aname;
        string actionDesc = action.description;

        textObjects[0].text = string.Format(">> {0} <<", actionName);
        textObjects[1].text = actionDesc;
    }

    public void UpdateDamage(int weaponDamage)
    {
        // Updates damage value displayed

        // If we are not doing damage, set the damage value and label to blank
        if (weaponDamage == 0)
        {
            textObjects[4].text = "";
            textObjects[5].text = "";
            return;
        }
        
        // If damage value is negative, we are healing
        if (weaponDamage > 0)
            textObjects[5].text = "Damage";
        else if (weaponDamage < 0)
            textObjects[5].text = "Healed";


        // Show damage or heal value without negative
        textObjects[4].text = Mathf.Abs(weaponDamage).ToString();
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
}
