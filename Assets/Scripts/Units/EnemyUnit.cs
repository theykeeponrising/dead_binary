using System.Collections.Generic;
using UnityEngine;

//Handles all unit logic that is enemy-specific
public class EnemyUnit : Unit
{
    //_approachWeight -- Prefer approaching player units
    //_conserveAmmoWeight -- Prefer conserving ammo if e.g. expect to do very little damage
    //_coverWeight -- Prefer being under cover
    //_damageWeight -- Prefer doing damage
    //_killWeight -- Prefer kill
    //_leaveCoverPenalty -- Penalize leaving current cover

    private float _approachWeight;
    private float _conserveAmmoWeight;
    private float _coverWeight;
    private float _damageWeight;
    private float _killWeight;
    private float _leaveCoverPenalty;

    private bool _isProcessingTurn = false;

    private Queue<EnemyAction> _actionsQueue;
    private Strategy _unitStrategy;

    private EnemyPod _enemyPod;
    [SerializeField] private EnemyPatrol _enemyPatrol;
    private Tile _patrolTile;

    public bool IsProcessingTurn { get { return _isProcessingTurn; } }
    public bool IsFollowing { get { return _enemyPod && !_enemyPod.IsLeader(this); } }

    protected override void Awake()
    {
        base.Awake();
        SetUnitStrategy(_unitStrategy);
    }

    protected override void Start()
    {
        base.Start();

        GetPatrolTile();
    }
    protected override void Update()
    {
        base.Update();

        if (IsActing() || !IsProcessingTurn)
            return;

        ProcessActions();           
    }

    public override void EnterCombat(bool alertFriendlies = false)
    {
        base.EnterCombat(alertFriendlies);
        SetAnimatorBool("patrolling", false);

        if (_enemyPod)
            _enemyPod.EnterCombat();
    }

    public void SetPod(EnemyPod enemyPod)
    {
        _enemyPod = enemyPod;
    }

    public void SetPod(EnemyPod enemyPod, EnemyPatrol enemyPatrol)
    {
        _enemyPod = enemyPod;
        _enemyPatrol = enemyPatrol;
    }

    public bool IsFollowingPod()
    {
        return _enemyPod && !_enemyPod.IsInCombat;
    }

    private void GetPatrolTile()
    {
        if (!_enemyPatrol || InCombat)
            return;

        _patrolTile = _enemyPatrol.GetNearestPatrolTile(this);
        SetAnimatorBool("patrolling", true);
    }

    private void CreatePatrolActions()
    {
        _patrolTile = _enemyPatrol.GetPatrolTile(this, _patrolTile);

        if (!_enemyPatrol.IsDestinationReached(this, _patrolTile))
        {
            EnemyAction patrolAction = CreateMoveAction(_patrolTile);
            _actionsQueue.Enqueue(patrolAction);
        }

        EnemyAction waitAction = CreateNoneAction();       
        _actionsQueue.Enqueue(waitAction);
    }

    private void CreateFollowActions()
    {
        EnemyAction waitAction = CreateNoneAction();

        if (_enemyPod.Leader.HasTurnEnded())
        {
            _actionsQueue.Enqueue(waitAction);
            return;
        }

        else if (_enemyPod.Leader.MoveData == null || _enemyPod.Leader.MoveData.Destination == null)
        {
            return;
        }

        Tile followTile = _enemyPod.Leader.MoveData.Destination;
        EnemyAction followAction = CreateMoveAction(followTile);
        _actionsQueue.Enqueue(followAction);
        _actionsQueue.Enqueue(waitAction);
    }

    private void ProcessActions()
    {
        if (_actionsQueue.Count == 0 && HasTurnEnded())
        {
            _isProcessingTurn = false;
        }
        else if (_actionsQueue.Count > 0)
        {
            //Process queued actions
            EnemyAction nextAction = _actionsQueue.Dequeue();
            PerformAction(nextAction);
        }
        else if (IsFollowing && !InCombat)
        {
            CreateFollowActions();
        }
        else if (IsPatrolling() && !InCombat)
        {
            CreatePatrolActions();
        }
        else
        {
            //If no actions queued up, get more
            List<EnemyAction> enemyActions = GetNextEnemyActions();

            foreach (EnemyAction action in enemyActions)
                _actionsQueue.Enqueue(action);
        }
    }

    public override void OnTurnStart()
    {
        base.OnTurnStart();
        _isProcessingTurn = true;
        _actionsQueue = new Queue<EnemyAction>();
    }

    public void ProcessTurn()
    {
        if (IsDead())
            return;

        Debug.Log(string.Format("Beginning Unit turn: {0}", gameObject.name));
        OnTurnStart();
    }

    //Determines the total expected benefit of proposed action plan
    //Maybe do some sort of deferred reward calculation?
    private float CalculateActionsValue(List<EnemyAction> actions)
    {
        float numKill = 0;
        float totalExpectedDamage = 0;
        int numShots = 0;
        int numTilesCloserToBestShootTarget = 0;

        List<Unit> hostileUnits = GetHostileUnits();
        //If current tile is cover, don't add additional bonus
        bool isCover = false;
        Tile unitTile = objectTile;

        foreach (EnemyAction enemyAction in actions)
        {
            UnitAction unitAction = enemyAction.UnitAction;

            //The last tile we move to will determine the cover value
            if (unitAction.IsType(typeof(UnitActionMove)))
            {
                isCover = Map.MapGrid.CheckIfCovered(GetNearestTarget(enemyAction.Tile, hostileUnits).objectTile, enemyAction.Tile);
                //Update the tile we do calculations with
                unitTile = enemyAction.Tile;
            }

            if (unitAction.IsType(typeof(UnitActionShoot)))
            {
                if (!IsTargetInLineOfSight(unitTile, enemyAction.ContextChar.objectTile)) continue;
                numShots++;
                //Calculated expected damage, if it would kill, etc.
                float damage = CalculateExpectedDamage(enemyAction.ContextChar);

                //TODO: Make this probabilistic (i.e. numKill += prob(kill))
                if (damage > enemyAction.ContextChar.GetHealth())
                    numKill += 1.0f;

                totalExpectedDamage += Mathf.Min(enemyAction.ContextChar.GetHealth(), damage);
            }
        }

        //Currently using best shoot target as a proxy for closest/most dangerous enemy
        //TODO: Replace best shoot target with enemy that would do highest expected damage or something

        //Make the unit approach the player
        GetBestShootTarget(hostileUnits, objectTile, out Unit shootTarget, out _);

        int oldDist, newDist = 0;

        if (shootTarget)
        {
            oldDist = Mathf.Max(Map.MapGrid.GetTileDistance(objectTile, shootTarget.objectTile) - Inventory.EquippedWeapon.GetMinimumRange(), 0);
            newDist = Mathf.Max(Map.MapGrid.GetTileDistance(unitTile, shootTarget.objectTile) - Inventory.EquippedWeapon.GetMinimumRange(), 0);

            // Note: More accurate calculation, but currently way too slow
            // oldDist = objectTile.GetMovementCost(shootTarget.objectTile).Count;
            // newDist = unitTile.GetMovementCost(shootTarget.objectTile).Count;
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

    private bool IsTargetInLineOfSight(Tile unitTile, Tile targetTile)
    {
        List<Tile> lineOfSightPath = Map.MapGrid.GetLineOfSightPath(unitTile, targetTile);
        if (lineOfSightPath.Count > 0) return true;
        return false;
    }

    private float CalculateActionValue(EnemyAction action)
    {
        List<EnemyAction> actions = new() { action };
        return CalculateActionsValue(actions);
    }

    private EnemyAction CreateMoveAction(Tile targetTile)
    {
        UnitAction moveAction = FindActionOfType(typeof(UnitActionMove));
        return new EnemyAction(moveAction, targetTile, null, null);
    }

    private EnemyAction CreateNoneAction()
    {
        UnitAction noneAction = FindActionOfType(typeof(UnitActionWait));
        return new EnemyAction(noneAction, objectTile, null, null);
    }

    private EnemyAction CreateReloadAction()
    {
        UnitAction reloadAction = FindActionOfType(typeof(UnitActionReload));
        return new EnemyAction(reloadAction, objectTile, null, null);
    }

    private EnemyAction CreateShootAction(Unit contextChar, Tile unitTile)
    {
        UnitAction shootAction = FindActionOfType(typeof(UnitActionShoot));
        return new EnemyAction(shootAction, unitTile, null, contextChar);
    }

    private void GetBestMoveTile(List<Tile> tilesInRange, out Tile tile)
    {
        float bestActionVal = 0;
        tile = null;

        foreach (Tile nextTile in tilesInRange)
        {
            if (nextTile == objectTile)
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
        List<Unit> hostileUnits,
        Tile tile,
        out Unit currentTarget,
        out float highestExpectedDamage)
    {
        highestExpectedDamage = 0;

        if (hostileUnits.Count == 0)
        {
            currentTarget = null;
            return;
        };

        currentTarget = hostileUnits[0];
        bool wouldKill = false;

        foreach (Unit other in hostileUnits)
        {
            float expectedDamage = CalculateExpectedDamage(other);
            expectedDamage = Mathf.Min(expectedDamage, other.Stats.HealthCurrent);

            //Determine whether shooting the unit would kill
            //Always favor shots that would kill
            if (!wouldKill && expectedDamage == other.Stats.HealthCurrent)
            {
                wouldKill = true;
                highestExpectedDamage = expectedDamage;
                currentTarget = other;
                //Continue if shot wouldn't kill (and another shot would)
            }
            else if (wouldKill && expectedDamage < other.Stats.HealthCurrent)
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
        List<Unit> hostileUnits = GetHostileUnits();
        List<EnemyAction> enemyActions = new();

        //If no ammo, either reload or switch weapon
        if (EquippedWeapon.Stats.AmmoCurrent == 0)
        {
            enemyActions.Add(CreateReloadAction());
            return enemyActions;
        }

        //If there's no tiles we can move to, or no enemies, do nothing
        if (tilesInRange.Count == 0 || hostileUnits.Count == 0)
        {
            enemyActions.Add(CreateNoneAction());
            return enemyActions;
        }

        //Next, we want to consider different combinations of actions, and their expected values
        //I.e. move+shoot, move+move, shoot+shoot, etc.

        //Get best 1AP Shoot Action
        List<EnemyAction> bestActions = ShootActionStrategy(hostileUnits);

        //Get Best 1AP Movement Action
        List<EnemyAction> moveActions = MoveActionStrategy(tilesInRange);

        if (CalculateActionsValue(moveActions) > CalculateActionsValue(bestActions))
        {
            bestActions = moveActions;
        }

        //Get Best 2AP Move + Shoot actions
        //Note: Action values for 2AP actions are averaged
        if (Stats.ActionPointsCurrent >= 2)
        {
            List<EnemyAction> moveAndShootActions = MoveAndShoot(hostileUnits, tilesInRange);
            float moveAndShootValue = CalculateActionsValue(moveAndShootActions);

            if (moveAndShootValue > CalculateActionsValue(bestActions))
            {
                bestActions = moveAndShootActions;
            }
        }

        enemyActions.AddRange(bestActions);

        if (enemyActions.Count == 0)
            enemyActions.Add(CreateNoneAction());

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

    private List<EnemyAction> MoveAndShoot(List<Unit> hostileUnits, List<Tile> tilesInRange)
    {
        //TODO: Find optimal position for each attackable unit, weight by cover, etc.
        //For each tile, calculate highest expected damage from attacking
        //For now, just choose tile with highest expected attack (or if unit can kill, choose that)

        Unit currentTarget = hostileUnits[0];
        List<EnemyAction> bestActions = new();

        foreach (Tile tile in tilesInRange)
        {
            if (tile == objectTile)
                continue;

            GetBestShootTarget(hostileUnits, tile, out Unit bestTarget, out float expectedDamage);

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
            case Strategy.AGGRESSIVE:
                {
                    _killWeight = 10.0f;
                    _damageWeight = 1.2f;
                    _coverWeight = 2.0f;
                    _conserveAmmoWeight = 0.5f;
                    _approachWeight = 0.4f;
                    _leaveCoverPenalty = 0.5f;
                    break;
                }
            case Strategy.DEFENSIVE:
                {
                    _killWeight = 5.0f;
                    _damageWeight = 1.0f;
                    _coverWeight = 3.0f;
                    _conserveAmmoWeight = 1.0f;
                    _approachWeight = 0.2f;
                    _leaveCoverPenalty = 0.5f;
                    break;
                }
            case Strategy.DEFAULT:
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
    private List<EnemyAction> ShootActionStrategy(List<Unit> hostileUnits)
    {
        //Get Shoot Action value
        List<EnemyAction> actions = new();

        GetBestShootTarget(hostileUnits, objectTile, out Unit shootTarget, out _);

        if (shootTarget != null)
        {
            EnemyAction shootAction = CreateShootAction(shootTarget, objectTile);
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

    private enum CoverScalingType { LINEAR }

    private enum Strategy { DEFAULT, AGGRESSIVE, DEFENSIVE, CUSTOM }

    private enum Behavior { PATROL, COMBAT, FLEE }
}