using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionManager : MonoBehaviour
{
    public static FactionManager Instance = null;

    public static Faction PV;
    public static Faction ACS;

    [SerializeField] Faction _pv; //= new Faction("Primogen Volition", new Color32(37, 232, 232, 255));
    [SerializeField] Faction _acs; //= new Faction("Anonymous Civil Service", new Color32(255, 0, 0, 255));

    [HideInInspector] public static List<Faction> AllFactions = new List<Faction>();

    private void Awake()
    {
        Instance = this;

        PV = _pv;
        ACS = _acs;

        AllFactions.Add(PV);
        AllFactions.Add(ACS);

        foreach (Faction faction in AllFactions)
            faction.Init();
    }
}
