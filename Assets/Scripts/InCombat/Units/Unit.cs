using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : GridObject
{
    //List of units on opposing faction that are alive
    protected List<Unit> oppFactionUnits;
    protected Grid grid;
    public Character character;
    
    void Start() 
    {
        character = gameObject.GetComponent<Character>();
        grid = character.currentTile.GetGrid();
    }

    public void OnTurnStart()
    {
        oppFactionUnits = new List<Unit>();
        character.stats.actionPointsCurrent = character.stats.actionPointsMax;
    }

    //Construct oppFactionUnits List
    //TODO: Should maybe put this in the PlayerTurn/EnemyTurnProcess so we don't duplicate work or something
    public List<Unit> GetOppFactionUnits()
    {
        if (oppFactionUnits != null && oppFactionUnits.Count != 0) return oppFactionUnits;
        Unit[] gos = GameObject.FindObjectsOfType<Unit>();

        foreach (var v in gos)
        {
            if (v.character.GetComponent<IFaction>() != null)
                if (character.attributes.faction != v.character.attributes.faction)
                    if (v.character.stats.healthCurrent > 0)
                        oppFactionUnits.Add(v);
        }

        //Sort list by distance to current unit
        if(oppFactionUnits.Count > 0)
        {
            oppFactionUnits.Sort(delegate (Unit a, Unit b)
            {
                return Vector2.Distance(character.gameObject.transform.position, a.character.transform.position).CompareTo(
                    Vector2.Distance(character.gameObject.transform.position, b.character.gameObject.transform.position));
            });
        }
        return oppFactionUnits;
    }

    //TODO: Get all tiles this unit can move to
    public List<Tile> GetTilesInMoveRange()
    {
        Vector3 pos = character.currentTile.transform.position;
        return grid.GetTilesInRange(pos, character.stats.movement);
    }

    
    public bool Shoot(Unit otherUnit)
    {
        // Calculate whether target hit
        // Dice roll performed
        int randomChance = Random.Range(1, 100);
        float hitChance = CalculateHitChance(this, otherUnit);
        int distance = grid.GetTileDistance(character.currentTile, otherUnit.character.currentTile);

        // FOR TESTING PURPOSES ONLY -- REMOVE WHEN FINISHED
        Debug.Log(string.Format("Distance: {0}, Base chance to hit: {1}%, Dice roll: {2}", distance, hitChance, randomChance));

        // Return true/false if hit connected
        return (hitChance >= randomChance);
    }

    protected float CalculateExpectedDamage(Unit attacker, Unit defender, Tile attackerTile)
    {
        float weaponDamange = attacker.character.inventory.equippedWeapon.GetDamage();
        float hitChance = CalculateHitChance(attacker, defender, attackerTile);
        return weaponDamange * hitChance;
    }

    //Overload for simplicity
    protected float CalculateHitChance(Unit attacker, Unit defender)
    {
        return CalculateExpectedDamage(attacker, defender, attacker.character.currentTile);
    }

    //Calculate Hit Chance
    protected float CalculateHitChance(Unit attacker, Unit defender, Tile attackerTile)
    {
        int distance = grid.GetTileDistance(attackerTile, defender.character.currentTile);
        float weaponAccuracyModifier = attacker.character.inventory.equippedWeapon.stats.baseAccuracyModifier;

        float weaponAccuracyPenalty = attacker.character.inventory.equippedWeapon.GetAccuracyPenalty(distance);

        // Calculate chance to be hit
        float hitModifier = GlobalManager.globalHit - character.stats.dodge - weaponAccuracyPenalty;

        // Add cover bonus if not being flanked
        if (character.currentCover && character.CheckIfCovered(attacker.character)) hitModifier -= character.currentCover.CoverBonus();
        
        float hitChance = (20 * attacker.character.stats.aim * weaponAccuracyModifier * hitModifier) / 100;
        return hitChance;
    }
}