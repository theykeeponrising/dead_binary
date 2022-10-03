using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    // Manifest of prefab assets for use in function calls
    public static ActionManager Instance = null;

    void Awake()
    {
        Instance = this;
    }

    [System.Serializable]
    public class UnitActionPrefabs
    {
        public UnitAction move;
        public UnitAction shoot;
        public UnitAction swap;
        public UnitAction reload;
        public UnitAction inventory;
        public UnitAction item_grenade;
        public UnitAction item_medkit;
    }

    public UnitActionPrefabs unitActions;
}
