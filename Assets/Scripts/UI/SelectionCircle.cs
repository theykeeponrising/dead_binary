using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionCircle : MonoBehaviour
{
    private Unit _unit;
    private Material _material;
    private Color32 _factionColor => _unit.attributes.faction.FactionColor;
    private bool _unitSelected => _unit.GetActor().GetPlayerAction().selectedCharacter == _unit;

    private void Awake()
    {
        _unit = GetComponentInParent<Unit>();
        _material = GetComponent<Renderer>().material;
    }

    public void ChangeSelection(SelectionType selectionType)
    {
        // Changes selection circle color based on selection type

        // Used to prevent selection circle from disappearing while a unit is still selected
        List<SelectionType> conflictingTypes = new List<SelectionType>() { SelectionType.HIGHLIGHT, SelectionType.TARGET_POTENTIAL, SelectionType.CLEAR };
        if (_unitSelected && conflictingTypes.Contains(selectionType))
            return;

        switch (selectionType)
        {
            case (SelectionType.SELECT):
            case (SelectionType.TARGET_MAIN):
                _material.color = (Color)_factionColor;
                _material.SetColor("_EmissiveColor", (Color)_factionColor);
                break;
            case (SelectionType.HIGHLIGHT):
            case (SelectionType.TARGET_POTENTIAL):
                _material.color = (Color)_factionColor / 2;
                _material.SetColor("_EmissiveColor", (Color)_factionColor /2);
                break;
            case (SelectionType.CLEAR):
            case (SelectionType.DESELECT):
                _material.color = Color.clear;
                _material.SetColor("_EmissiveColor", Color.clear);
                break;
        }
    }
}

public enum SelectionType { CLEAR, SELECT, DESELECT, HIGHLIGHT, TARGET_MAIN, TARGET_POTENTIAL }
