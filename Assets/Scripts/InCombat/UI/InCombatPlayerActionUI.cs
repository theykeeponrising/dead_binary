using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using TMPro;

//UI for the InCombatPlayerAction - for now just moved the stateText to here
public class InCombatPlayerActionUI : MonoBehaviour
{
    public TextMeshProUGUI stateText;
    
    [Tooltip("The object to float above the Target's head.")]
    public GameObject selectorBall;

    void Awake() { }
    void Start() { }

    public TextMeshProUGUI GetStateText()
    {
        return stateText;
    }
}
