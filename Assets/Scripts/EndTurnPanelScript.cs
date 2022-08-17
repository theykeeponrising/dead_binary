using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnPanelScript : MonoBehaviour
{
    public InCombatPlayerAction playerAction;
    public Button button;

    // Start is called before the first frame update
    void Start()
    {
        //A bit of a hack to get the InCombatPlayerAction
        StateHandler stateHandler = GameObject.FindGameObjectWithTag("StateHandler").GetComponent<StateHandler>();
        PlayerTurnState playerTurnState = (PlayerTurnState) stateHandler.GetStateObject(StateHandler.State.PlayerTurnState);
        playerAction = playerTurnState.GetPlayerAction();

        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(playerAction.EndTurn);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
