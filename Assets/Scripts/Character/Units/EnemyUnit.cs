using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Handles all unit logic that is enemy-specific
public class EnemyUnit : Unit
{
    public enum Strategy {
        Default,
        Aggressive,
        Defensive,
        Custom,
    };

    public Strategy unitStrategy;
    //Prefer doing damage
    public float DamageWeight;
    //Prefer kill
    public float KillWeight;
    //Prefer being under cover
    public float CoverWeight;
    //Prefer conserving ammo if e.g. expect to do very little damage
    public float ConserveAmmoWeight;
    //Prefer approaching player units
    public float ApproachWeight;

    Queue<EnemyAction> actionsQueue;
    //TODO: Maybe a small weight for moving towards the player?

    public bool isProcessingTurn = false;

    public struct EnemyAction {
        private readonly Action actionType;
        public Action ActionType { get { return actionType; } }

        private readonly Tile tile;
        public Tile Tile { get { return tile; } }

        private readonly List<Tile> path;
        public List<Tile> Path { get { return path; } }

        private readonly Unit contextChar;
        public Unit ContextChar { get { return contextChar; } }

        public EnemyAction(Action actionType, Tile tile, List<Tile> path, Unit contextChar)
        {
            this.actionType = actionType;
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

    public void SetUnitStrategy(Strategy strategy)
    {
        switch (strategy)
        {
            case Strategy.Aggressive:
                KillWeight = 10.0f;
                DamageWeight = 1.2f;
                CoverWeight = 2.0f;
                ConserveAmmoWeight = 0.5f;
                ApproachWeight = 0.3f;
                break;
            case Strategy.Defensive:
                KillWeight = 5.0f;
                DamageWeight = 1.0f;
                CoverWeight = 4.0f;
                ConserveAmmoWeight = 1.0f;
                ApproachWeight = 0.2f;
                break;
            case Strategy.Default:
                KillWeight = 7.0f;
                DamageWeight = 1.0f;
                CoverWeight = 2.0f;
                ConserveAmmoWeight = 0.5f;
                ApproachWeight = 0.3f;
                break;
        }
    }

    public void ProcessUnitTurn()
    {
        if (GetFlag("dead")) return;
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
        if (IsActing() || !IsProcessingTurn()) return;
        if (actionsQueue.Count == 0 && stats.actionPointsCurrent == 0) {
            isProcessingTurn = false;
        }
        else if (actionsQueue.Count > 0)
        {
            //Process queued actions
            EnemyAction nextAction = actionsQueue.Dequeue();
            PerformAction(nextAction);
        } else
        {
            //If no actions queued up, get more
            List<EnemyAction> enemyActions = GetNextEnemyActions();
            foreach (EnemyAction action in enemyActions) actionsQueue.Enqueue(action);
        }
    }

    //Decide what to do with action points

    public List<EnemyAction> GetNextEnemyActions()
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
        if (tilesInRange.Count == 0 || oppFactionUnits.Count == 0) {
            enemyActions.Add(CreateNoneAction(currentTile));
            return enemyActions;
        } 

        //Next, we want to consider different combinations of actions, and their expected values
        //I.e. move+shoot, move+move, shoot+shoot, etc.
        
        List<EnemyAction> bestActions;

        //Get best 1AP Shoot Action
        bestActions = ShootActionStrategy();

        //Get Best 1AP Movement Action
        List<EnemyAction> moveActions = MoveActionStrategy(tilesInRange);
        if (CalculateActionsValue(moveActions) > CalculateActionsValue(bestActions))
        {
            bestActions = moveActions;
        }
        
        //Get Best 2AP Move + Shoot actions
        //Note: Action values for 2AP actions are averaged
        if (stats.actionPointsCurrent >= 2) {
            List<EnemyAction> moveAndShootActions = MoveAndShoot(tilesInRange);
            float moveAndShootValue = CalculateActionsValue(moveAndShootActions);
            if (moveAndShootValue > CalculateActionsValue(bestActions)) 
            {
                bestActions = moveAndShootActions;
            }
        }

        enemyActions.AddRange(bestActions);
        return enemyActions;
    }

    private EnemyAction CreateMoveAction(Tile targetTile)
    {
        return new EnemyAction(Action.action_move, targetTile, null, null);
    }

    private EnemyAction CreateShootAction(Unit contextChar, Tile unitTile)
    {
        return new EnemyAction(Action.action_shoot, unitTile, null, contextChar);
    }

    private EnemyAction CreateReloadAction(Tile unitTile)
    {
        return new EnemyAction(Action.action_reload, unitTile, null, null);
    }

    private EnemyAction CreateNoneAction(Tile unitTile)
    {
        return new EnemyAction(Action.action_none, unitTile, null, null);
    }

    //Calculate best target to shoot, using 1 AP
    private List<EnemyAction> ShootActionStrategy()
    {
        //Get Shoot Action value
        Unit shootTarget; float expectedDamage;
        List<EnemyAction> actions = new List<EnemyAction>();
        GetBestShootTarget(currentTile, out shootTarget, out expectedDamage);
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
    private float CalculateActionsValue(List<EnemyAction> actions)
    {
        bool isCover = false;
        float numKill = 0.0f;
        float totalExpectedDamage = 0.0f;
        int numShots = 0;
        int numTilesCloserToBestShootTarget = 0;
        Tile unitTile = currentTile;
        // Tile unitTile = currentTile;
        foreach (EnemyAction enemyAction in actions)
        {
            //The last tile we move to will determine the cover value
            if (enemyAction.ActionType == Action.action_move)
            {                
                //If current tile is cover, don't add additional bonus
                isCover = enemyAction.Tile.IsCover() && !currentTile.IsCover();
                //Update the tile we do calculations with
                unitTile = enemyAction.Tile;
            } else if (enemyAction.ActionType == Action.action_shoot)
            {
                numShots++;
                //Calculated expected damage, if it would kill, etc.
                float damage = CalculateExpectedDamage(this, enemyAction.ContextChar, currentTile);

                //TODO: Make this probabilistic (i.e. numKill += prob(kill))
                if (damage > enemyAction.ContextChar.GetHealth()) numKill += 1.0f;
                totalExpectedDamage += Mathf.Min(enemyAction.ContextChar.GetHealth(), damage);
                // if(debug) Debug.Log(string.Format("Would kill? {0}, Damage: {1}", numKill, damage));
            }
        }

        //Make the unit approach the player
        Unit shootTarget; float expectedDamage;
        GetBestShootTarget(currentTile, out shootTarget, out expectedDamage);
        if (shootTarget) {
            int oldDist = grid.GetTileDistance(currentTile, shootTarget.currentTile);
            int newDist = grid.GetTileDistance(unitTile, shootTarget.currentTile);
            numTilesCloserToBestShootTarget = oldDist - newDist;
        }

        float actionValue = 0.0f;
        if (actions.Count > 0) 
        {
            float shootActionValue = (totalExpectedDamage * DamageWeight) + numKill * KillWeight - numShots * ConserveAmmoWeight;
            float moveActionValue = System.Convert.ToSingle(isCover) * CoverWeight + numTilesCloserToBestShootTarget * ApproachWeight;
            actionValue = (shootActionValue + moveActionValue) / actions.Count;
        }
        return actionValue;
    }

    private float CalculateActionValue(EnemyAction action)
    {
        List<EnemyAction> actions = new List<EnemyAction>();
        actions.Add(action);
        return CalculateActionsValue(actions);
    }

    public void GetBestMoveTile(List<Tile> tilesInRange, out Tile tile)
    {
        float bestActionVal = 0.0f;
        tile = null;
        foreach (Tile nextTile in tilesInRange)
        {
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
    public void GetBestShootTarget(Tile tile, out Unit currentTarget, out float highestExpectedDamage)
    {
        currentTarget = oppFactionUnits[0];
        highestExpectedDamage = 0.0f;
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
            } else if (wouldKill && expectedDamage < other.stats.healthCurrent) continue;

            //If this shot would do more damage, choose it
            if (expectedDamage > highestExpectedDamage) {
                highestExpectedDamage = expectedDamage;
                currentTarget = other;
            }
        }
    }

    //Get tiles unit could move to, given unit is on startTile
    List<Tile> GetTilesInMoveRange(Tile startTile)
    {
        Vector3 pos = startTile.transform.position;
        return grid.GetTilesInRange(pos, stats.movement);
    }

    List<EnemyAction> MoveAndShoot(List<Tile> tilesInRange)
    {
        //TODO: Find optimal position for each attackable unit, weight by cover, etc.
        //For each tile, calculate highest expected damage from attacking
        //For now, just choose tile with highest expected attack (or if unit can kill, choose that)

        Unit currentTarget = oppFactionUnits[0];

        List<EnemyAction> bestActions = new List<EnemyAction>();

        foreach (Tile tile in tilesInRange)
        {
            Unit bestTarget;
            float expectedDamage;
            GetBestShootTarget(tile, out bestTarget, out expectedDamage);

            EnemyAction moveAction = CreateMoveAction(tile);
            EnemyAction shootAction = CreateShootAction(currentTarget, tile);

            List<EnemyAction> enemyActions = new List<EnemyAction> {moveAction, shootAction};
            if (CalculateActionsValue(enemyActions) > CalculateActionsValue(bestActions)) 
                bestActions = enemyActions;
        }
        return bestActions;
    }

    private void PerformAction(EnemyAction action) 
    {
        Action actionType = action.ActionType;
        if (actionType == Action.action_move)
        {
            GetActor().ProcessAction(actionType, action.Tile, null, null);
        } else if (actionType == Action.action_shoot)
        {
            GetActor().ProcessAction(actionType, null, null, action.ContextChar);
        } else if (actionType == Action.action_reload)
        {
            GetActor().ProcessAction(actionType, null, null, null);
        }
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