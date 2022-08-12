using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnPanelScript : MonoBehaviour
{
    public InCombatPlayerAction player;
    public Button button;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<InCombatPlayerAction>();
        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(player.EndTurn);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
