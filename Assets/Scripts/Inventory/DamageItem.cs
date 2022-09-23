using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DamageItem : Item
{
    // Base item class for all items that change HP amount.
    // Healing items will use a negative amount.

    [Tooltip("HP amount change from use. Negative amounts will heal.")]
    public int hpAmount;

}
