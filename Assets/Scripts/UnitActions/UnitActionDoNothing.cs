using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitActionDoNothing : UnitAction
{
    public override void UseAction()
    {
        Debug.Log(string.Format("{0} used a DoNothing action!"), unit.gameObject);
    }
}
