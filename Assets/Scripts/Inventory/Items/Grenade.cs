using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : DamageItem
{
    public override void UseItem(Unit target)
    {
        Debug.Log(string.Format("Healed {0} for {1} health!", target.attributes.name, -hpAmount));
        target.RestoreHealth(-hpAmount);
        itemUsesCurrent -= 1;
    }
}
