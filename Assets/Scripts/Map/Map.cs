using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    MapEffects mapEffects;

    void Start()
    {
        mapEffects = GetComponent<MapEffects>();
    }


    public GameObject CreateEffect(GameObject efxPrefab, Vector3 efxPosition, Quaternion efxRotation)
    {
        // Used to track effects and remove oldest effects if we are above effects limit

        return mapEffects.CreateEffect(efxPrefab, efxPosition, efxRotation);
    }

    public GameObject CreateTimedEffect(GameObject efxPrefab, Vector3 efxPosition, Quaternion efxRotation, float timer)
    {
        // Used to track effects and remove oldest effects if we are above effects limit

        return mapEffects.CreateTimedEffect(efxPrefab, efxPosition, efxRotation, timer);
    }

    public void AddEffect(GameObject efxPrefab)
    {
        // Add an existing effect to the effects list

        mapEffects.AddEffect(efxPrefab.transform);
    }

    public void AddEffect(Transform efxPrefab)
    {
        // Add an existing effect to the effects list

        mapEffects.AddEffect(efxPrefab.transform);
    }

    public void AddEffect(ItemProp efxPrefab)
    {
        // Add an existing effect to the effects list

        mapEffects.AddEffect(efxPrefab.transform);
    }
}
