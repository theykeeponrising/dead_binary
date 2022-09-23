using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Handles all unit logic that is enemy-specific
public class EnemyUnit : Unit
{
    public enum Strategy {
        Aggressive,
        Defensive,
        Custom,
    };

    public Strategy unitStrategy;
    public float DamageWeight;
    public float KillWeight;
    public float CoverWeight;

    public bool isActing = false;

    //TODO: Maybe create a struct for an Enemyaction, with ActionsList, Tile, etc.
    //And then process the actions in order? 
    //Currently the actions happen at the same time so the robots shoot while moving and etc.
    public void ProcessUnitTurn()
    {
        if (GetFlag("dead")) return;
        OnTurnStart();
        //Find all units this one can attack
        //TODO: maybe handle this in EnemyTurnProcess so it doesn't get processed for every unit for performance reasons, and skip processing dead units or remove them from this list?
        List<Unit> oppFactionUnits = GetOppFactionUnits();

        //Get all tiles within range
        List<Tile> tilesInRange = GetTilesInMoveRange();

        //Perform actions, and store actions taken in list
        List<Action> enemyActions = EnemyTurnActions(tilesInRange, oppFactionUnits);        
    }

    public override void OnTurnStart()
    {
        base.OnTurnStart();
        isActing = true;
    }

    protected override void Update()
    {
        base.Update();
        if (isActing)
        {  
            //TODO: Also check animations, and whether other actions are in progress
            //Probably should have some combined method for units/characters in general that checks this
            if (!IsActing()) isActing = false;
        }
    }

    //Decide what to do with action points
    public List<Action> EnemyTurnActions(List<Tile> tilesInRange, List<Unit> oppFactionUnits)
    {
        List<Action> enemyActions = new List<Action>();

        //If no ammo, either reload or switch weapon
        if (inventory.equippedWeapon.stats.ammoCurrent == 0)
        {
            GetActor().ReloadAction();
            enemyActions.Add(Action.action_reload);
            stats.actionPointsCurrent--;
        }

        //If there's no tiles we can move to, or no enemies, do nothing
        if (tilesInRange.Count == 0 || oppFactionUnits.Count == 0) {
            enemyActions.Add(Action.action_none);
            return enemyActions;
        } 

        //Use the rest of the units action points
        while (stats.actionPointsCurrent >= 2) {
            List<Action> moveAndShootActions = MoveAndShoot(tilesInRange);
            enemyActions.AddRange(moveAndShootActions);
            stats.actionPointsCurrent -= 2;
        }

        while (stats.actionPointsCurrent > 0)
        {
            //Find cover if possible
            Action findCoverResult = FindCover(tilesInRange);
            
            //If no cover, just shoot at something
            if (findCoverResult == Action.action_none)
            {
                //TODO: Uses same logic as MoveAndShoot, should probably consolidate
                float highestExpectedDamage = 0.0f;
                Unit currentTarget = oppFactionUnits[0];
                bool wouldKill = false;
                foreach (Unit other in oppFactionUnits)
                {
                    float expectedDamage = CalculateExpectedDamage(this, other, currentTile);
                    expectedDamage = Math.Min(expectedDamage, other.stats.healthCurrent);

                    //Determine whether shooting the unit would kill
                    //Always favor shots that would kill
                    if (expectedDamage == other.stats.healthCurrent && !wouldKill)
                    {
                        wouldKill = true;
                        highestExpectedDamage = expectedDamage;    
                        currentTarget = other;        
                    //Break if shot wouldn't kill (and another shot would)
                    } else if (wouldKill) break;

                    //If this shot would do more damage, choose it
                    if (expectedDamage > highestExpectedDamage) {
                        highestExpectedDamage = expectedDamage;
                        currentTarget = other;
                    }
                }
                GetActor().ProcessAction(Action.action_shoot, null, null, currentTarget);
                enemyActions.Add(Action.action_shoot);
            } 

            stats.actionPointsCurrent--;
        }
        return enemyActions;
    }

    Action FindCover(List<Tile> tilesInRange)
    {
        //Just find any cover
        //TODO: Make this better
        foreach (Tile tile in tilesInRange)
        {
            if (tile.IsCover()) {
                GetActor().MoveAction(tile, null);
                return Action.action_move;
            }
        }

        return Action.action_none;
    }

    List<Action> MoveAndShoot(List<Tile> tilesInRange)
    {
        //TODO: Find optimal position for each attackable unit, weight by cover, etc.
        //For each tile, calculate highest expected damage from attacking
        //For now, just choose tile with highest expected attack (or if unit can kill, choose that)

        //TODO: Allow enemy to just shoot twice without moving, if that would be better
        //Currently assumes the enemy moves no matter what

        Tile bestTile = tilesInRange[0];
        Unit currentTarget = oppFactionUnits[0];
        float highestExpectedDamage = 0.0f;
        bool wouldKill = false;

        foreach (Tile tile in tilesInRange)
        {
            foreach (Unit other in oppFactionUnits)
            {
                float expectedDamage = CalculateExpectedDamage(this, other, tile);
                expectedDamage = Mathf.Min(expectedDamage, other.stats.healthCurrent);

                //Determine whether shooting the unit would kill
                //Always favor shots that would kill
                if (expectedDamage == other.stats.healthCurrent && !wouldKill)
                {
                    wouldKill = true;
                    highestExpectedDamage = expectedDamage;    
                    bestTile = tile;
                    currentTarget = other;        
                //Break if shot wouldn't kill (and another shot would)
                } else if (wouldKill) break;

                //If this shot would do more damage, choose it
                if (expectedDamage > highestExpectedDamage) {
                    highestExpectedDamage = expectedDamage;
                    bestTile = tile;
                    currentTarget = other;
                }
            }
        }

        //Perform actions
        GetActor().ProcessAction(Action.action_move, bestTile, null, null);
        GetActor().ProcessAction(Action.action_shoot, null, null, currentTarget);

        List<Action> enemyActions = new List<Action>();
        enemyActions.Add(Action.action_move);
        enemyActions.Add(Action.action_shoot);
        return enemyActions;
    }
}