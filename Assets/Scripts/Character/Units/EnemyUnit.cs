using System.Collections.Generic;
using UnityEngine;

//Handles all unit logic that is enemy-specific
public class EnemyUnit : Unit
{
    private Queue<EnemyAction> _actionsQueue;

    //Prefer approaching player units
    private float _approachWeight;

    //Prefer conserving ammo if e.g. expect to do very little damage
    private float _conserveAmmoWeight;

    //Prefer being under cover
    private float _coverWeight;

    //Prefer doing damage
    private float _damageWeight;

    //TODO: Maybe a small weight for moving towards the player?
    private bool _isProcessingTurn = false;

    //Prefer kill
    private float _killWeight;

    //Penalize leaving current cover
    private float _leaveCoverPenalty;

    private Strategy _unitStrategy;

    private enum CoverScalingType
    {
        LINEAR
    };

    private enum Strategy
    {
        Default,
        Aggressive,
        Defensive,
        Custom,
    };

    public bool IsProcessingTurn() => _isProcessingTurn;

    public override void OnTurnStart()
    {
        base.OnTurnStart();
        _isProcessingTurn = true;
        _actionsQueue = new Queue<EnemyAction>();
    }

    public void ProcessUnitTurn()
    {
        Debug.Log(string.Format("Beginning Unit turn: {0}", gameObject.name));

        if (GetFlag(FlagType.DEAD))
            return;

        OnTurnStart();
    }

    protected override void Awake()
    {
        base.Awake();
        SetUnitStrategy(_unitStrategy);
    }

    protected override void Update()
    {
        base.Update();

        if (GetActor().IsActing() || !IsProcessingTurn())
            return;

        if (_actionsQueue.Count == 0 && stats.actionPointsCurrent == 0)
        {
            _isProcessingTurn = false;
        }
        else if (_actionsQueue.Count > 0)
        {
            //Process queued actions
            EnemyAction nextAction = _actionsQueue.Dequeue();
            PerformAction(nextAction);
        }
        else
        {
            //If no actions queued up, get more
            List<EnemyAction> enemyActions = GetNextEnemyActions();

            foreach (EnemyAction action in enemyActions)
                _actionsQueue.Enqueue(action);
        }
    }

    //Determines the total expected benefit of proposed action plan
    //Maybe do some sort of deferred reward calculation?
    private float CalculateActionsValue(List<EnemyAction> actions)
    {
        float numKill = 0;
        float totalExpectedDamage = 0;
        int numShots = 0;
        int numTilesCloserToBestShootTarget = 0;

        List<Unit> oppFactionUnits = GetOppFactionUnits();
        //If current tile is cover, don't add additional bonus
        bool isCover = false;
        Tile unitTile = currentTile;

        foreach (EnemyAction enemyAction in actions)
        {
            UnitAction unitAction = enemyAction.UnitAction;

            //The last tile we move to will determine the cover value
            if (unitAction.IsType(typeof(UnitActionMove)))
            {
                isCover = grid.CheckIfCovered(GetNearestTarget(enemyAction.Tile, oppFactionUnits).currentTile, enemyAction.Tile);
                //Update the tile we do calculations with
                unitTile = enemyAction.Tile;
            }

            if (unitAction.IsType(typeof(UnitActionShoot)))
            {
                numShots++;
                //Calculated expected damage, if it would kill, etc.
                float damage = CalculateExpectedDamage(this, enemyAction.ContextChar, unitTile);

                //TODO: Make this probabilistic (i.e. numKill += prob(kill))
                if (damage > enemyAction.ContextChar.GetHealth())
                    numKill += 1.0f;

                totalExpectedDamage += Mathf.Min(enemyAction.ContextChar.GetHealth(), damage);
            }
        }

        //Currently using best shoot target as a proxy for closest/most dangerous enemy
        //TODO: Replace best shoot target with enemy that would do highest expected damage or something

        //Make the unit approach the player
        GetBestShootTarget(oppFactionUnits, currentTile, out Unit shootTarget, out _);

        int oldDist, newDist = 0;

        if (shootTarget)
        {
            oldDist = grid.GetTileDistance(currentTile, shootTarget.currentTile);
            newDist = grid.GetTileDistance(unitTile, shootTarget.currentTile);
            numTilesCloserToBestShootTarget = oldDist - newDist;
        }

        float actionValue = 0;

        if (actions.Count > 0)
        {
            float shootActionValue = (totalExpectedDamage * _damageWeight) + numKill * _killWeight - numShots * _conserveAmmoWeight;
            float coverActionValue = System.Convert.ToSingle(isCover) * _coverWeight * GetCoverWeightScaling(newDist, CoverScalingType.LINEAR);
            float moveActionValue = numTilesCloserToBestShootTarget * _approachWeight;
            actionValue = Mathf.Max((shootActionValue + coverActionValue + moveActionValue) / actions.Count, 0.1f);
        }

        return actionValue;
    }

    private float CalculateActionValue(EnemyAction action)
    {
        List<EnemyAction> actions = new() { action };
        return CalculateActionsValue(actions);
    }

    private EnemyAction CreateMoveAction(Tile targetTile)
    {
        UnitAction moveAction = GetActor().FindActionOfType(typeof(UnitActionMove));
        return new EnemyAction(moveAction, targetTile, null, null);
    }

    private EnemyAction CreateNoneAction(Tile unitTile)
    {
        UnitAction noneAction = GetActor().FindActionOfType(typeof(UnitActionWait));
        return new EnemyAction(noneAction, unitTile, null, null);
    }

    private EnemyAction CreateReloadAction(Tile unitTile)
    {
        UnitAction reloadAction = GetActor().FindActionOfType(typeof(UnitActionReload));
        return new EnemyAction(reloadAction, unitTile, null, null);
    }

    private EnemyAction CreateShootAction(Unit contextChar, Tile unitTile)
    {
        UnitAction shootAction = GetActor().FindActionOfType(typeof(UnitActionShoot));
        return new EnemyAction(shootAction, unitTile, null, contextChar);
    }

    private void GetBestMoveTile(List<Tile> tilesInRange, out Tile tile)
    {
        float bestActionVal = 0;
        tile = null;

        foreach (Tile nextTile in tilesInRange)
        {
            if (nextTile == currentTile)
                continue;

            EnemyAction action = CreateMoveAction(nextTile);
            float actionVal = CalculateActionValue(action);

            if (actionVal > bestActionVal)
            {
                tile = nextTile;
                bestActionVal = actionVal;
            }
        }
    }

    //Get the target that the unit would do the most damage to
    //If unit can kill multiple target(s), choose the one it would deal the most damage to
    private void GetBestShootTarget(
        List<Unit> oppFactionUnits,
        Tile tile,
        out Unit currentTarget,
        out float highestExpectedDamage)
    {
        highestExpectedDamage = 0;

        if (oppFactionUnits.Count == 0)
        {
            currentTarget = null;
            return;
        };

        currentTarget = oppFactionUnits[0];
        bool wouldKill = false;

        foreach (Unit other in oppFactionUnits)
        {
            float expectedDamage = CalculateExpectedDamage(this, other, tile);
            expectedDamage = Mathf.Min(expectedDamage, other.stats.healthCurrent);

            //Determine whether shooting the unit would kill
            //Always favor shots that would kill
            if (!wouldKill && expectedDamage == other.stats.healthCurrent)
            {
                wouldKill = true;
                highestExpectedDamage = expectedDamage;
                currentTarget = other;
                //Continue if shot wouldn't kill (and another shot would)
            }
            else if (wouldKill && expectedDamage < other.stats.healthCurrent)
            {
                continue;
            }

            //If this shot would do more damage, choose it
            if (expectedDamage > highestExpectedDamage)
            {
                highestExpectedDamage = expectedDamage;
                currentTarget = other;
            }
        }
    }

    //Scale the cover weight based on how close the unit is to the enemy unit
    //The closer it is, the more that being under cover should matter
    //TODO: Could maybe base this on enemy hit% instead of on tile distance
    private float GetCoverWeightScaling(int tileDist, CoverScalingType scaling)
    {
        return scaling switch
        {
            CoverScalingType.LINEAR => ScaleCoverLinear(tileDist),
            _ => 0
        };
    }

    private List<EnemyAction> GetNextEnemyActions()
    {
        //Get tiles unit can move to
        List<Tile> tilesInRange = GetTilesInMoveRange();
        //TODO: maybe handle this in EnemyTurnProcess so it doesn't get processed for every unit for performance reasons, and skip processing dead units or remove them from this list?
        List<Unit> oppFactionUnits = GetOppFactionUnits();
        List<EnemyAction> enemyActions = new();

        //If no ammo, either reload or switch weapon
        if (inventory.equippedWeapon.Stats.AmmoCurrent == 0)
        {
            enemyActions.Add(CreateReloadAction(currentTile));
            return enemyActions;
        }

        //If there's no tiles we can move to, or no enemies, do nothing
        if (tilesInRange.Count == 0 || oppFactionUnits.Count == 0)
        {
            enemyActions.Add(CreateNoneAction(currentTile));
            return enemyActions;
        }

        //Next, we want to consider different combinations of actions, and their expected values
        //I.e. move+shoot, move+move, shoot+shoot, etc.

        //Get best 1AP Shoot Action
        List<EnemyAction> bestActions = ShootActionStrategy(oppFactionUnits);

        //Get Best 1AP Movement Action
        List<EnemyAction> moveActions = MoveActionStrategy(tilesInRange);

        if (CalculateActionsValue(moveActions) > CalculateActionsValue(bestActions))
        {
            bestActions = moveActions;
        }

        //Get Best 2AP Move + Shoot actions
        //Note: Action values for 2AP actions are averaged
        if (stats.actionPointsCurrent >= 2)
        {
            List<EnemyAction> moveAndShootActions = MoveAndShoot(oppFactionUnits, tilesInRange);
            float moveAndShootValue = CalculateActionsValue(moveAndShootActions);

            if (moveAndShootValue > CalculateActionsValue(bestActions))
            {
                bestActions = moveAndShootActions;
            }
        }

        enemyActions.AddRange(bestActions);

        if (enemyActions.Count == 0)
            enemyActions.Add(CreateNoneAction(currentTile));

        return enemyActions;
    }

    //Calculate best tile to move to, using 1 AP
    private List<EnemyAction> MoveActionStrategy(List<Tile> tilesInRange)
    {
        List<EnemyAction> actions = new();

        GetBestMoveTile(tilesInRange, out Tile bestMoveTile);

        if (bestMoveTile != null)
        {
            EnemyAction moveAction = CreateMoveAction(bestMoveTile);
            actions.Add(moveAction);
        }

        return actions;
    }

    private List<EnemyAction> MoveAndShoot(List<Unit> oppFactionUnits, List<Tile> tilesInRange)
    {
        //TODO: Find optimal position for each attackable unit, weight by cover, etc.
        //For each tile, calculate highest expected damage from attacking
        //For now, just choose tile with highest expected attack (or if unit can kill, choose that)

        Unit currentTarget = oppFactionUnits[0];
        List<EnemyAction> bestActions = new();

        foreach (Tile tile in tilesInRange)
        {
            if (tile == currentTile)
                continue;

            GetBestShootTarget(oppFactionUnits, tile, out Unit bestTarget, out float expectedDamage);

            EnemyAction moveAction = CreateMoveAction(tile);
            EnemyAction shootAction = CreateShootAction(currentTarget, tile);
            List<EnemyAction> enemyActions = new() { moveAction, shootAction };

            if (CalculateActionsValue(enemyActions) > CalculateActionsValue(bestActions))
                bestActions = enemyActions;
        }

        return bestActions;
    }

    private void PerformAction(EnemyAction action)
    {
        UnitAction unitAction = action.UnitAction;

        System.Action useActionDelegate = action.UnitAction switch
        {
            UnitActionMove => () => unitAction.UseAction(action.Tile),
            UnitActionShoot => () => unitAction.UseAction(action.ContextChar),
            _ => () => unitAction.UseAction()
        };

        useActionDelegate.Invoke();
    }

    private float ScaleCoverLinear(float tileDist, float maxDist = 12.0f)
    {
        float coverScale = (maxDist - tileDist) / maxDist;
        return coverScale;
    }

    private void SetUnitStrategy(Strategy strategy)
    {
        switch (strategy)
        {
            case Strategy.Aggressive:
                {
                    _killWeight = 10.0f;
                    _damageWeight = 1.2f;
                    _coverWeight = 2.0f;
                    _conserveAmmoWeight = 0.5f;
                    _approachWeight = 0.4f;
                    _leaveCoverPenalty = 0.5f;
                    break;
                }
            case Strategy.Defensive:
                {
                    _killWeight = 5.0f;
                    _damageWeight = 1.0f;
                    _coverWeight = 3.0f;
                    _conserveAmmoWeight = 1.0f;
                    _approachWeight = 0.2f;
                    _leaveCoverPenalty = 0.5f;
                    break;
                }
            case Strategy.Default:
                {
                    _killWeight = 7.0f;
                    _damageWeight = 1.0f;
                    _coverWeight = 2.0f;
                    _conserveAmmoWeight = 0.5f;
                    _approachWeight = 0.3f;
                    _leaveCoverPenalty = 0.5f;
                    break;
                }
        }
    }

    //Decide what to do with action points
    //Calculate best target to shoot, using 1 AP
    private List<EnemyAction> ShootActionStrategy(List<Unit> oppFactionUnits)
    {
        //Get Shoot Action value
        List<EnemyAction> actions = new();

        GetBestShootTarget(oppFactionUnits, currentTile, out Unit shootTarget, out _);

        if (shootTarget != null)
        {
            EnemyAction shootAction = CreateShootAction(shootTarget, currentTile);
            actions.Add(shootAction);
        }

        return actions;
    }

    private struct EnemyAction
    {
        public EnemyAction(UnitAction unitAction, Tile tile, List<Tile> path, Unit contextChar)
        {
            UnitAction = unitAction;
            Tile = tile;
            Path = path;
            ContextChar = contextChar;
        }

        public Unit ContextChar { get; }
        public List<Tile> Path { get; }
        public Tile Tile { get; }
        public UnitAction UnitAction { get; }
    };

    public override void TakeDamage(Unit attacker, int damage, int distanceToTarget, MessageType damageType = MessageType.DMG_CONVENTIONAL)
    {
        // Called by an attacking source when taking damage
        // TO DO: More complex damage reduction will be added here

        // If attacked missed, do not take damage
        if (!RollForHit(attacker, distanceToTarget))
        {
            if (currentCover) currentCover.PlayImpactSFX();
            GetAnimator().SetTrigger("dodge");
            Debug.Log(string.Format("{0} missed target {1}!", attacker.attributes.name, attributes.name));
            return;
        }

        UIManager.GetTurnIndicator().SetTurnIndicatorMessage(damageType);
        Vector3 direction = (transform.position - attacker.transform.position);
        float distance = (transform.position - attacker.transform.position).magnitude;
        CheckDeath(attacker, direction, distance, damage);
    }

    public override void TakeDamage(Unit attacker, int damage, Vector3 attackPoint, MessageType damageType = MessageType.DMG_CONVENTIONAL)
    {
        // Called by an attacking item when taking damage
        // TO DO: More complex damage reduction will be added here

        UIManager.GetTurnIndicator().SetTurnIndicatorMessage(damageType);
        Vector3 direction = (transform.position - attackPoint);
        float distance = (transform.position - attackPoint).magnitude;
        CheckDeath(attacker, direction, distance, damage, 50f);
    }
}