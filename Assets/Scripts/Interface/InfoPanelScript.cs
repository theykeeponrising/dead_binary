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

    public void UpdateAction(Actions.ActionList actionEnum)
    {
        Actions.Action action = Actions.ActionsDict[actionEnum];
        string actionName = action.aname;
        string actionDesc = action.description;

        textObjects[0].text = actionName;
        textObjects[1].text = actionDesc;
    }

    public void UpdateDamage(int weaponDamage)
    {
        // Updates damage value displayed

        textObjects[4].text = weaponDamage.ToString();
    }

    public void UpdateHit(float hitChance)
    {
        // Updates hit chance value displayed

        string displayText = string.Format("{0}%", (hitChance * 100).ToString("0"));
        textObjects[2].text = displayText;
    }
}
