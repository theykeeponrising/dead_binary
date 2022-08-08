using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ActionPanelScript : MonoBehaviour
{
    [SerializeField] private InCombatPlayerAction player;
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI apTextBox;
    [SerializeField] private TextMeshProUGUI ammoTextBox;

    private void Start()
    {
        // Assign the player.
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<InCombatPlayerAction>(); 
    }

    private void Update()
    {
        bool p = player.selectedCharacter ? true : false;
        panel.SetActive(p);

        if (p == true)
        {
            apTextBox.text = player.selectedCharacter.stats.actionPointsCurrent.ToString();
            ammoTextBox.text = player.selectedCharacter.equippedWeapon.stats.ammoCurrent.ToString();
        }
    }    
}
