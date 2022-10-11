using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionManager : MonoBehaviour
{
    public static FactionManager Instance = null;

    public Faction PV; //= new Faction("Primogen Volition", new Color32(37, 232, 232, 255));
    public Faction ACS; //= new Faction("Anonymous Civil Service", new Color32(255, 0, 0, 255));

    [HideInInspector] public static List<Faction> AllFactions = new List<Faction>();

    private void Awake()
    {
        Instance = this;

        AllFactions.Add(PV);
        AllFactions.Add(ACS);

        foreach (Faction faction in AllFactions)
            faction.Init();
    }
}
