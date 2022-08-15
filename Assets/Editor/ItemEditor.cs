using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Item), true)]
public class ItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Item data = (Item)target;

        EditorGUILayout.LabelField("Private Stats", EditorStyles.boldLabel);

        data.statics.itemName = EditorGUILayout.TextField("Name", data.statics.itemName);
        data.statics.itemType = (ItemType)EditorGUILayout.EnumPopup("Item Type", data.statics.itemType);
        data.statics.itemCost = EditorGUILayout.IntField("Cost", data.statics.itemCost);

        data.statics.isReusable = EditorGUILayout.Toggle("Is Reusable", data.statics.isReusable);
        data.statics.cooldownMax = EditorGUILayout.IntField("Cooldown", data.statics.cooldownMax);

        data.statics.targetType = (TargetType)EditorGUILayout.EnumPopup("Target Type", data.statics.targetType);
        if (data.statics.targetType == TargetType.Character)
            data.statics.affinity = (Affinity)EditorGUILayout.EnumFlagsField("Affinity", data.statics.affinity);

        data.statics.icon = (Sprite)EditorGUILayout.ObjectField("Icon", data.statics.icon, typeof(Sprite), true);

        EditorGUILayout.LabelField("Public Variables", EditorStyles.boldLabel);
        data.CurrentCooldown = EditorGUILayout.IntField("Current Cooldown", data.CurrentCooldown);
    }
}