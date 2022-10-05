

public interface IFaction
{
    /// <summary>
    /// The faction the Unit belongs to.
    /// </summary>
    abstract Faction faction { get; set; }

    /// <summary>
    /// Checks if this should be hostile toward the target Unit.
    /// </summary>
    /// <param name="target">The unit to check faction</param>
    /// <returns>Whether to be hostile toward the Unit.</returns>
    public bool beHostileToTarget(IFaction target)
    {
        bool beHostile = false;

        if(target.faction != faction)
        { beHostile = true; }

        return beHostile;
    }

    public static Faction GetFactionByRelation(Unit sourceUnit, TargetFaction targetFaction = TargetFaction.ENEMY)
    {
        // Returns the target faction based on relative affinity to source unit

        if (targetFaction == TargetFaction.FRIENDLY)
            return sourceUnit.attributes.faction;
        else if (targetFaction == TargetFaction.ENEMY)
        {
            if (sourceUnit.attributes.faction == Faction.Good)
                return Faction.Bad;
            else
                return Faction.Good;
        }
        else return Faction.Any;
    }
}

public enum Faction
{
    Neutral, Good, Bad, Any
}
