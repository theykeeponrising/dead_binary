using System.Collections.Generic;
using UnityEngine;

public class Faction : MonoBehaviour
{
    public string FactionName;
    public Color32 FactionColor;
    public List<Unit> FactionUnits;

    private bool _turnInProgress = false;
    public bool TurnInProgress { get { return _turnInProgress; } }

    private AudioSource _audioSource;
    private readonly Dictionary<Faction, FactionAffinity> _factionRelation = new();

    [SerializeField] private AudioClip _sfxFactionTurnStart;
    [SerializeField] private List<Faction> _friendlyTowards = new();
    [SerializeField] private List<Faction> _hostileTowards = new();

    public void Init()
    {
        _audioSource = UIManager.Instance.GetComponent<AudioSource>();

        foreach (Faction faction in FactionManager.AllFactions)
            _factionRelation[faction] = FactionAffinity.NEUTRAL;

        foreach (Faction faction in _friendlyTowards)
            _factionRelation[faction] = FactionAffinity.FRIENDLY;

        foreach (Faction faction in _hostileTowards)
            _factionRelation[faction] = FactionAffinity.ENEMY;

        _factionRelation[this] = FactionAffinity.FRIENDLY;

        _turnInProgress = false;
    }

    public void StartTurn()
    {
        Debug.Log(string.Format("{0} turn start called", FactionName));
        PlayFactionSFX();
        _turnInProgress = true;
    }

    public void EndTurn()
    {
        PlayFactionSFX();
        _turnInProgress = false;
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

        List<Faction> factions = new();
        List<Faction> foundFactions = factions;

        foreach (Faction faction in _factionRelation.Keys)
        {
            if (_factionRelation[faction] == factionAffinity)
                foundFactions.Add(faction);
        }
        return foundFactions;
    }

    public void PlayFactionSFX()
    {
        _audioSource.PlayOneShot(_sfxFactionTurnStart);
    }
}

public enum FactionAffinity { FRIENDLY, ENEMY, NEUTRAL, ANY, NONE }
