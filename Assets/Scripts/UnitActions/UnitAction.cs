using System.Collections.Generic;
using UnityEngine;

public abstract class UnitAction : MonoBehaviour
{
    [SerializeField] private string _actionName;
    [SerializeField] private string _actionDescription;
    [SerializeField] private int _actionCost;
    [SerializeField] private int _actionCooldown;
    [SerializeField] private int _actionCooldownRemaining;

    [SerializeField] private UnitActionRequirement[] _actionRequirements;
    [SerializeField] private UnitActionEnum _actionSprite;

    protected int _actionStage = 0;
    private bool _actionPerforming;
    private bool _actionPerformed;

    private Unit _unit;
    private Item _item;

    protected FiniteState<InCombatPlayerAction> CurrentState;
    [SerializeField] protected float BufferStart;
    [SerializeField] protected float BufferEnd;

    public Unit Unit { get { return _unit; } }
    public Item Item { get { return _item; } }
    public string ActionName { get { return _actionName; } }
    public string ActionDescription { get { return _actionDescription; } }
    public int ActionCost { get { return _actionCost; } }
    public UnitActionEnum ActionSprite { get { return _actionSprite; } }

    public bool HasSprite() => _actionSprite != UnitActionEnum.NONE;
    public bool Performing() => _actionPerforming;
    public bool Performed() => _actionPerformed;
    public bool OnCooldown() => _actionCooldownRemaining > 0;

    private void Awake()
    {
        _unit = GetComponentInParent<Unit>();
        _item = GetComponentInParent<Item>();
    }

    private void LateUpdate()
    {
        if (Performing()) CheckAction();
    }

    public void SetName(string name)
    { _actionName = name; }

    public void SetDescription(string description)
    { _actionDescription = description; }

    public void SetPerformed(bool performed)
    { _actionPerformed = performed; }

    public bool IsType(System.Type type)
    { return GetType() == type; }

    public int GetActionStage()
    { return _actionStage; }

    public void NextActionStage()
    { _actionStage += 1; }

    public bool IsActionStage(int stageCheck)
    { return _actionStage == stageCheck; }

    public void OnTurnStart()
    {
        // Called at the start of the unit's turn
        // Reduce cooldown (if any) and reset the performed flag

        if (OnCooldown()) _actionCooldownRemaining -= 1;
        _actionPerformed = false;
    }

    public bool CheckRequirements(bool printDebug = false)
    {
        // Checks each requirement item in list
        // If any requirement fails, returns false, otherwise return true
        if (_actionRequirements.Length > 0)
            foreach (UnitActionRequirement requirement in _actionRequirements)
            {
                // Check that unit meets AP cost
                if (requirement == UnitActionRequirement.AP)
                    if (!Unit || Unit.Stats.ActionPointsCurrent < ActionCost)
                    {
                        if (printDebug) Debug.Log("failed requirement ap");
                        return false;
                    }

                // Check that unit meets Ammo cost
                if (requirement == UnitActionRequirement.AMMO)
                    if (!Unit || Unit.EquippedWeapon.Stats.AmmoCurrent <= 0)
                    {
                        if (printDebug) Debug.Log("failed requirement ammo");
                        return false;
                    }

                // Check that unit's ammo isn't full
                if (requirement == UnitActionRequirement.RELOAD)
                    if (!Unit || Unit.EquippedWeapon.Stats.AmmoCurrent >= Unit.EquippedWeapon.Stats.AmmoMax)
                    {
                        if (printDebug) Debug.Log("failed requirement reload");
                        return false;
                    }

                if (requirement == UnitActionRequirement.QUANTITY)
                    if (Item.itemUsesCurrent <= 0)
                    {
                        if (printDebug) Debug.Log("failed requirement quantity");
                        return false;
                    }

                // Check that there are targets in unit's line of sight
                if (requirement == UnitActionRequirement.TARGETS)
                    if (!Unit || Unit.GetTargetsInLineOfSight<Unit>().Count == 0)
                    {
                        if (printDebug) Debug.Log("failed requirement targets");
                        return false;
                    }

            }

        return true;
    }

    public void StartPerformance()
    {
        // Starts action's performing flag

        _actionPerforming = true;
        SetPerformed(true);
    }

    public void StartPerformance(string animation)
    {
        // Starts action's performing flag with an animation

        _actionPerforming = true;
        SetPerformed(true);
        Unit.PlayAnimation(animation);
    }

    public void EndPerformance()
    {
        // Ends action's performing flag
        // Returns target to cover if applicable

        _actionPerforming = false;
        _actionStage = 0;
        Unit.CoverCrouch();
        Unit.PlayerAction.CheckTurnEnd();
    }

    public virtual void UseAction()
    {
        Debug.Log(string.Format("No action use found for {0}", ActionName));
    }

    public virtual void UseAction(Unit unit)
    {
        Debug.Log(string.Format("No action use found for {0} (unit)", ActionName));
    }

    public virtual void UseAction(Vector3 position)
    {
        Debug.Log(string.Format("No action use found for {0} (vector3)", ActionName));
    }

    public virtual void UseAction(Tile tile)
    {
        Debug.Log(string.Format("No action use found for {0} (tile)", ActionName));
    }

    public virtual void UseAction(Tile tile, List<Tile> path)
    {
        Debug.Log(string.Format("No action use found for {0} (tile, path)", ActionName));
    }

    public virtual void UseAction(FiniteState<InCombatPlayerAction> setState)
    {
        Debug.Log(string.Format("No action use found for {0} (finite state)", ActionName));
    }

    public virtual void UseAction(FiniteState<InCombatPlayerAction> setState, Item item)
    {
        Debug.Log(string.Format("No action use found for {0} (finite state, item)", ActionName));
    }

    public virtual void CheckAction()
    {
        Debug.Log(string.Format("No action check found for {0}", ActionName));
    }
}

public enum UnitActionRequirement { NONE, AP, AMMO, RELOAD, QUANTITY, TARGETS };
