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
}
