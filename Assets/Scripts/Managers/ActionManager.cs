using UnityEngine;

public class ActionManager : MonoBehaviour
{
    // Manifest of prefab assets for use in function calls
    public static ActionManager Instance = null;

    void Awake()
    {
        Instance = this;
        UnitActions = _unitActions;
    }

    [System.Serializable]
    public class UnitActionPrefabs
    {
        public UnitAction Move;
        public UnitAction Shoot;
        public UnitAction Swap;
        public UnitAction Reload;
        public UnitAction Inventory;
        public UnitAction ItemGrenade;
        public UnitAction ItemMedkit;
        public UnitAction Wait;
        public UnitAction Talk;
    }

    [SerializeField] private UnitActionPrefabs _unitActions;
    public static UnitActionPrefabs UnitActions;
}
