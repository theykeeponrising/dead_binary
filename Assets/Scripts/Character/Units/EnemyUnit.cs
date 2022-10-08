using System.Collections.Generic;
using UnityEngine;

//Handles all unit logic that is enemy-specific
public class EnemyUnit : Unit
{
    private enum Strategy
    {
        Default,
        Aggressive,
        Defensive,
        Custom,
    };

    private enum CoverScalingType
    {
        LINEAR
    };

    private Strategy unitStrategy;
    //Prefer doing damage
    private float DamageWeight;
    //Prefer kill
    private float KillWeight;
    //Prefer being under cover
    private float CoverWeight;
    //Prefer conserving ammo if e.g. expect to do very little damage
    private float ConserveAmmoWeight;
    //Prefer approaching player units
    private float ApproachWeight;
    //Penalize leaving current cover
    private float LeaveCoverPenalty;

    private Queue<EnemyAction> actionsQueue;
    //TODO: Maybe a small weight for moving towards the player?

    private bool isProcessingTurn = false;

    private struct EnemyAction
    {
        private readonly UnitAction unitAction;
        public UnitAction GetUnitAction() => unitAction;

        private readonly Tile tile;
        public Tile Tile { get { return tile; } }

        private readonly List<Tile> path;
        public List<Tile> Path { get { return path; } }

        private readonly Unit contextChar;
        public Unit ContextChar { get { return contextChar; } }

        public EnemyAction(UnitAction unitAction, Tile tile, List<Tile> path, Unit contextChar)
        {
            this.unitAction = unitAction;
            this.tile = tile;
            this.path = path;
            this.contextChar = contextChar;
        }
    };

    protected override void Awake()
    {
        base.Awake();
        SetUnitStrategy(unitStrategy);
    }

    private void SetUnitStrategy(Strategy strategy)
    {
        switch (strategy)
        {
            case Strategy.Aggressive:
                KillWeight = 10.0f;
                DamageWeight = 1.2f;
                CoverWeight = 2.0f;
                ConserveAmmoWeight = 0.5f;
                ApproachWeight = 0.4f;
                LeaveCoverPenalty = 0.5f;
                break;
            case Strategy.Defensive:
                KillWeight = 5.0f;
                DamageWeight = 1.0f;
                CoverWeight = 3.0f;
                ConserveAmmoWeight = 1.0f;
                ApproachWeight = 0.2f;
                LeaveCoverPenalty = 0.5f;
                break;
            case Strategy.Default:
                KillWeight = 7.0f;
                DamageWeight = 1.0f;
                CoverWeight = 2.0f;
                ConserveAmmoWeight = 0.5f;
                ApproachWeight = 0.3f;
                LeaveCoverPenalty = 0.5f;
                break;
        }
    }

    public void ProcessUnitTurn()
    {
        Debug.Log(string.Format("Beginning Unit turn: {0}", gameObject.name));
        if (GetFlag(FlagType.DEAD)) return;
        OnTurnStart();
    }

    public override void OnTurnStart()
    {
        base.OnTurnStart();
        isProcessingTurn = true;
        actionsQueue = new Queue<EnemyAction>();
    }

    public bool IsProcessingTurn()
    {
        return isProcessingTurn;
    }

    protected override void Update()
    {
        base.Update();
        if (GetActor().IsActing() || !IsProcessingTurn()) return;
        if (actionsQueue.Count == 0 && stats.actionPointsCurrent == 0)
        {
            isProcessingTurn = false;
        }
        else if (actionsQueue.Count > 0)
        {
            //Process queued actions
            EnemyAction nextAction = actionsQueue.Dequeue();
            PerformAction(nextAction);
        }
        else
        {
            //If no actions queued up, get more
            List<EnemyAction> enemyActions = GetNextEnemyActions();
            foreach (EnemyAction action in enemyActions) actionsQueue.Enqueue(action);
        }
    }

    //Decide what to do with action points

    private List<EnemyAction> GetNextEnemyActions()
    {
        //Get tiles unit can move to
        List<Tile> tilesInRange = GetTilesInMoveRange();

        //TODO: maybe handle this in EnemyTurnProcess so it doesn't get processed for every unit for performance reasons, and skip processing dead units or remove them from this list?
        List<Unit> oppFactionUnits = GetOppFactionUnits();

        List<EnemyAction> enemyActions = new List<EnemyAction>();

        //If no ammo, either reload or switch weapon
        if (inventory.equippedWeapon.stats.ammoCurrent == 0)
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

        List<EnemyAction> bestActions;

        //Get best 1AP Shoot Action
        bestActions = ShootActionStrategy(oppFactionUnits);

        //Debug.Log(string.Format("Shoot AV: {0}", CalculateActionsValue(bestActions)));
        //Get Best 1AP Movement Action
        List<EnemyAction> moveActions = MoveActionStrategy(tilesInRange);
        //Debug.Log(string.Format("Move AV: {0}", CalculateActionsValue(moveActions)));
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
            //Debug.Log(string.Format("Shoot+Move AV: {0}", moveAndShootValue));
            if (moveAndShootValue > CalculateActionsValue(bestActions))
            {
                bestActions = moveAndShootActions;
            }
        }

        enemyActions.AddRange(bestActions);

        if (enemyActions.Count == 0) enemyActions.Add(CreateNoneAction(currentTile));
        return enemyActions;
    }

    private EnemyAction CreateMoveAction(Tile targetTile)
    {
        UnitAction moveAction = GetActor().FindActionOfType(typeof(UnitActionMove));
        return new EnemyAction(moveAction, targetTile, null, null);
    }

    private EnemyAction CreateShootAction(Unit contextChar, Tile unitTile)
    {
        UnitAction shootAction = GetActor().FindActionOfType(typeof(UnitActionShoot));
        return new EnemyAction(shootAction, unitTile, null, contextChar);
    }

    private EnemyAction CreateReloadAction(Tile unitTile)
    {
        UnitAction reloadAction = GetActor().FindActionOfType(typeof(UnitActionReload));
        return new EnemyAction(reloadAction, unitTile, null, null);
    }

    private EnemyAction CreateNoneAction(Tile unitTile)
    {
        UnitAction noneAction = GetActor().FindActionOfType(typeof(UnitActionWait));
        return new EnemyAction(noneAction, unitTile, null, null);
    }

    //Calculate best target to shoot, using 1 AP
    private List<EnemyAction> ShootActionStrategy(List<Unit> oppFactionUnits)
    {
        //Get Shoot Action value
        Unit shootTarget; float expectedDamage;
        List<EnemyAction> actions = new List<EnemyAction>();
        GetBestShootTarget(oppFactionUnits, currentTile, out shootTarget, out expectedDamage);
        if (shootTarget != null)
        {
            EnemyAction shootAction = CreateShootAction(shootTarget, currentTile);
            actions.Add(shootAction);
        }
        return actions;
    }

    //Calculate best tile to move to, using 1 AP
    private List<EnemyAction> MoveActionStrategy(List<Tile> tilesInRange)
    {
        Tile bestMoveTile;
        List<EnemyAction> actions = new List<EnemyAction>();
        GetBestMoveTile(tilesInRange, out bestMoveTile);
        if (bestMoveTile != null)
        {
            EnemyAction moveAction = CreateMoveAction(bestMoveTile);
            actions.Add(moveAction);
        }
        return actions;
    }

    //Determines the total expected benefit of proposed action plan
    //Maybe do some sort of deferred reward calculation?
    private float CalculateActionsValue(List<EnemyAction> actions, bool debug = false)
    {
        float numKill = 0.0f;
        float totalExpectedDamage = 0.0f;
        int numShots = 0;
        int numTilesCloserToBestShootTarget = 0;

        List<Unit> oppFactionUnits = GetOppFactionUnits();

        //If current tile is cover, don't add additional bonus
        bool prevCover = grid.CheckIfCovered(GetNearestTarget(currentTile, oppFactionUnits).currentTile, currentTile);
        bool isCover = false;

        Tile unitTile = currentTile;
        foreach (EnemyAction enemyAction in actions)
        {
            UnitAction unitAction = enemyAction.GetUnitAction();

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
                if (damage > enemyAction.ContextChar.GetHealth()) numKill += 1.0f;
                totalExpectedDamage += Mathf.Min(enemyAction.ContextChar.GetHealth(), damage);
            }
        }

        //Currently using best shoot target as a proxy for closest/most dangerous enemy
        //TODO: Replace best shoot target with enemy that would do highest expected damage or something

        //Make the unit approach the player
        Unit shootTarget; float expectedDamage;
        GetBestShootTarget(oppFactionUnits, currentTile, out shootTarget, out expectedDamage);
        int oldDist, newDist = 0;

        if (shootTarget)
        {
            oldDist = grid.GetTileDistance(currentTile, shootTarget.currentTile);
            newDist = grid.GetTileDistance(unitTile, shootTarget.currentTile);
            numTilesCloserToBestShootTarget = oldDist - newDist;
        }

        if (isCover)
        {
            float a = System.Convert.ToSingle(isCover);
            float b = GetCoverWeightScaling(newDist, CoverScalingType.LINEAR);
        }

        float actionValue = 0.0f;
        if (actions.Count > 0)
        {
            float shootActionValue = (totalExpectedDamage * DamageWeight) + numKill * KillWeight - numShots * ConserveAmmoWeight;
            float coverActionValue = (System.Convert.ToSingle(isCover) * CoverWeight * GetCoverWeightScaling(newDist, CoverScalingType.LINEAR));
            float leaveCoverPenalty = System.Convert.ToSingle(prevCover) * LeaveCoverPenalty;
            float moveActionValue = numTilesCloserToBestShootTarget * ApproachWeight;
            actionValue = Mathf.Max((shootActionValue + coverActionValue + moveActionValue) / actions.Count, 0.1f);
        }
        return actionValue;
    }

    //Scale the cover weight based on how close the unit is to the enemy unit
    //The closer it is, the more that being under cover should matter
    //TODO: Could maybe base this on enemy hit% instead of on tile distance
    private float GetCoverWeightScaling(int tileDist, CoverScalingType scaling)
    {
        switch (scaling)
        {
            case CoverScalingType.LINEAR:
                return ScaleCoverLinear((float)tileDist);
            default:
                return 0.0f;
        }
    }

    private float ScaleCoverLinear(float tileDist, float maxDist = 12.0f)
    {
        float coverScale = (maxDist - tileDist) / maxDist;
        return coverScale;
    }

    private float CalculateActionValue(EnemyAction action)
    {
        List<EnemyAction> actions = new List<EnemyAction>();
        actions.Add(action);
        return CalculateActionsValue(actions);
    }

    private void GetBestMoveTile(List<Tile> tilesInRange, out Tile tile)
    {
        float bestActionVal = 0.0f;
        tile = null;
        foreach (Tile nextTile in tilesInRange)
        {
            if (nextTile == currentTile) continue;
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
    private void GetBestShootTarget(List<Unit> oppFactionUnits, Tile tile, out Unit currentTarget, out float highestExpectedDamage)
    {
        highestExpectedDamage = 0.0f;
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
            else if (wouldKill && expectedDamage < other.stats.healthCurrent) continue;

            //If this shot would do more damage, choose it
            if (expectedDamage > highestExpectedDamage)
            {
                highestExpectedDamage = expectedDamage;
                currentTarget = other;
            }
        }
    }

    //Get tiles unit could move to, given unit is on startTile
    private List<Tile> GetTilesInMoveRange(Tile startTile)
    {
        Vector3 pos = startTile.transform.position;
        return grid.GetTilesInRange(pos, stats.movement);
    }

    private List<EnemyAction> MoveAndShoot(List<Unit> oppFactionUnits, List<Tile> tilesInRange)
    {
        //TODO: Find optimal position for each attackable unit, weight by cover, etc.
        //For each tile, calculate highest expected damage from attacking
        //For now, just choose tile with highest expected attack (or if unit can kill, choose that)

        Unit currentTarget = oppFactionUnits[0];

        List<EnemyAction> bestActions = new List<EnemyAction>();

        foreach (Tile tile in tilesInRange)
        {
            if (tile == currentTile) continue;
            Unit bestTarget;
            float expectedDamage;
            GetBestShootTarget(oppFactionUnits, tile, out bestTarget, out expectedDamage);

            EnemyAction moveAction = CreateMoveAction(tile);
            EnemyAction shootAction = CreateShootAction(currentTarget, tile);

            List<EnemyAction> enemyActions = new List<EnemyAction> { moveAction, shootAction };
            if (CalculateActionsValue(enemyActions) > CalculateActionsValue(bestActions))
                bestActions = enemyActions;
        }
        return bestActions;
    }

    private void PerformAction(EnemyAction action)
    {
        UnitAction unitAction = action.GetUnitAction();

        if (unitAction.IsType(typeof(UnitActionMove))) unitAction.UseAction(action.Tile);

        else if (unitAction.IsType(typeof(UnitActionShoot))) unitAction.UseAction(action.ContextChar);

        else unitAction.UseAction();

    }
}

//Recursively determines best actions. Shelved for now.
//TODO: Use DetermineOptimalActions to get the best set of actions recursively
//On backburner for now - kinda complicated to implement

// public List<EnemyAction> DetermineOptimalActions(List<Tile> tilesInRange, List<Unit> oppFactionUnits)
// {
//     List<EnemyAction> turnActions = new List<EnemyAction>();
//     List<Actions.Action> availableActions = new List<Actions.Action>() {
//         Actions.action_move,
//         Actions.action_shoot,
//         Actions.action_reload
//     };
//     CalculateBestAction(ref turnActions, ref availableActions, stats.actionPointsMax, inventory.equippedWeapon.stats.ammoCurrent);
// return turnActions;

//Get the best action for each unit
//Note: Exponential runtime in number of action points (and potentially high mem) so don't increase numActionPoints by a huge amount
// private float CalculateBestAction(ref List<EnemyAction> currentActions, ref List<Actions.Action> availableActions, int numActionPoints, int numBullets)
// {
//     // Base Case
//     if (numActionPoints == 0) return CalculateActionsValue(currentActions);
//     Tile unitTile;
//     if (currentActions.Count == 0) unitTile = currentTile;
//     else unitTile = currentActions.Last().Tile;

//     List<EnemyAction> actionsToTest = new List<EnemyAction>();
//     //Get current tile

//     //Generate list of enemy actions we want to test
//     foreach (Actions.Action possibleAction in availableActions)
//     {
//         //Can't shoot with no bullets
//         if (numBullets == 0 && possibleAction == Actions.action_shoot) continue;

//         //Determine best target to shoot
//         if (possibleAction == Actions.action_shoot)
//         {
//             Unit contextChar = null;
//             float expectedDamage = 0.0f;
//             GetBestShootTarget(unitTile, out contextChar, out expectedDamage);

//             EnemyAction nextAction = CreateShootAction(contextChar, unitTile);
//             actionsToTest.Add(nextAction);
//         }
//         //Recurse on all possible move tiles
//         //Maybe memoize based on visited tile?
//         else if (possibleAction == Actions.action_move)
//         {
//             List<Tile> tilesInRange = GetTilesInMoveRange(unitTile);
//             foreach (Tile nextTile in tilesInRange)
//             {
//                 EnemyAction nextAction = CreateMoveAction(nextTile);
//                 actionsToTest.Add(nextAction);
//             }
//         }
//         else if (possibleAction == Actions.action_reload)
//         {
//             EnemyAction nextAction = CreateReloadAction(unitTile);
//             actionsToTest.Add(nextAction);
//         }
//     }

//     List<EnemyAction> bestActions = null;
//     float bestActionVal = -1.0f;

//     foreach (EnemyAction testAction in actionsToTest)
//     {
//         List<EnemyAction> newActionsList = new List<EnemyAction>(currentActions);
//         newActionsList.Add(testAction);

//         //Adjust number of bullets based on reloading/shooting
//         int nextNumBullets = numBullets;
//         if (testAction.ActionType == Actions.action_reload) nextNumBullets = inventory.equippedWeapon.stats.ammoMax;
//         else if (testAction.ActionType == Actions.action_shoot) nextNumBullets--;

//         //Recursively calculate action value for a list of actions
//         float nextActionVal = CalculateBestAction(ref newActionsList, ref availableActions, numActionPoints - 1, nextNumBullets);
//         if (nextActionVal > bestActionVal)
//         {
//             bestActionVal = nextActionVal;
//             bestActions = newActionsList;
//         }
//     }

//     //Combine the current actions with the new best action list
//     //Note: the resulting list will always have length = numActionPoints, and represent a full set of actions
//     //At the top level, this will be the optimal set of actions
//     currentActions.AddRange(bestActions);
//     return bestActionVal;
// }