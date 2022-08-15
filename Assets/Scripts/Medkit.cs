using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Restores target to Max health
/// </summary>
public class Medkit : Item
{
    enum RestoreType { Percentage, Flat }
    [SerializeField] private RestoreType _restoreType;

    protected override void UseItem
        (Character owner, Character charTarget = null, CoverObject covTarg = null)
    {
        RestoreHealth(charTarget);
    }

    public void RestoreHealth(Character target)
    {
        target.RestoreHealth(target.stats.healthMax);
        Debug.Log("Restored '" + target.name + "' to full health!");
    }
}

