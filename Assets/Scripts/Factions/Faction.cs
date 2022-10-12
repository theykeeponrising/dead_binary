using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faction : MonoBehaviour
{
    public string FactionName;
    public Color32 FactionColor;
    public List<Unit> FactionUnits;
    Dictionary<Faction, FactionAffinity> _factionRelation = new Dictionary<Faction, FactionAffinity>();

    [SerializeField] List<Faction> FriendlyTowards = new List<Faction>();
    [SerializeField] List<Faction> HostileTowards = new List<Faction>();

    public void Init()
    {
        foreach (Faction faction in FactionManager.AllFactions)
            _factionRelation[faction] = FactionAffinity.NEUTRAL;

        foreach (Faction faction in FriendlyTowards)
            _factionRelation[faction] = FactionAffinity.FRIENDLY;

        foreach (Faction faction in HostileTowards)
            _factionRelation[faction] = FactionAffinity.ENEMY;

        _factionRelation[this] = FactionAffinity.FRIENDLY;
    }

    public void SetFactionRelation(Faction faction, FactionAffinity factionAffinity)
    {
        // Sets faction affinity to provided value

        _factionRelation[faction] = factionAffinity;
    }

    public FactionAffinity GetFactionRelation(Faction faction)
    {
        // Returns the affinity of a specific faction relative to this one

        if (_factionRelation.ContainsKey(faction))
            return _factionRelation[faction];
        return FactionAffinity.NEUTRAL;
    }

    public List<Faction> GetFactionsByRelation(FactionAffinity factionAffinity)
    {
        // Gets all factions of the relative affinity to this faction

        List<Faction> foundFactions = new List<Faction>();
        foreach (Faction faction in _factionRelation.Keys)
        {
            if (_factionRelation[faction] == factionAffinity)
                foundFactions.Add(faction);
        }
        return foundFactions;
    }
}

public enum FactionAffinity { FRIENDLY, ENEMY, NEUTRAL, ANY, NONE }
