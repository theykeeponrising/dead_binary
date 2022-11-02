using UnityEngine;

public abstract class UnitTargetAction : UnitAction
{
    [SerializeField] bool _useCharacterCamera = false;
    protected Unit TargetUnit;
    protected Vector3 TargetPosition;
    public FactionAffinity TargetFaction;

    public bool UseCharacterCamera { get { return _useCharacterCamera; } }

    public override void UseAction(Unit unit)
    {
        base.UseAction();
    }
}
