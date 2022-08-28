using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Item Data", menuName = "New Item")]
public partial class ItemData : ScriptableObject
{
    [SerializeField] private string itemName;
    [SerializeField] private ItemType itemType;
    [SerializeField] private int itemCost;
    [SerializeField] private bool isReusable;
    [SerializeField] private int cooldown;
    [SerializeField] private TargetType targetType;
    [SerializeField] private Affinity affinity;
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject prefab;

    public string ItemName => itemName;
    public ItemType ItemType => itemType;
    public int ItemCost => itemCost;
    public bool IsReusable => isReusable;
    public int Cooldown => cooldown;
    public TargetType TargetType => targetType;
    public Affinity Affinity => affinity;
    public Sprite Icon => icon;
    public GameObject Prefab => prefab;
}

/*
[Flags]
public enum Affinity
{
    Ally = 1 << 0,
    Enemy = 1 << 1,
    Neutral = 1 << 2
}
public enum ItemType { ATTACHMENT, EQUIPMENT, CONSUMABLE, QUEST }
public enum TargetType { Character, Cover, Weapon }
// ATTACHMENT - Not usable item, passive effect ... ex scopes, grips, implants
// EQUIPMENT - Usable unlimited item, immediate effect ... ex jammer, taser, radio
// CONSUMABLE - Usable limited item, immediate effect ... ex grenades, medkits, stimpacks
// QUEST - Not usable ... ex supplies, intel, macguffin


public partial class ItemData
{
    [CustomEditor(typeof(ItemData))]
    public class ItemDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ItemData data = (ItemData)target;

            data.itemName = EditorGUILayout.TextField("Name", data.itemName);
            data.itemType = (ItemType)EditorGUILayout.EnumPopup("Item Type", data.itemType);
            data.itemCost = EditorGUILayout.IntField("Cost", data.itemCost);

            data.isReusable = EditorGUILayout.Toggle("Is Reusable", data.isReusable);
            if (data.isReusable)
                data.cooldown = EditorGUILayout.IntField("Cooldown", data.cooldown);

            data.targetType = (TargetType)EditorGUILayout.EnumPopup("Target Type", data.targetType);
            if (data.targetType == TargetType.Character)
                data.affinity = (Affinity)EditorGUILayout.EnumFlagsField("Affinity", data.affinity);
            
            data.icon = (Sprite)EditorGUILayout.ObjectField("Icon", data.icon, typeof(Sprite),true);
        }
    }
}
*/