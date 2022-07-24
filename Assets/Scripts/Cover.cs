using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cover : MonoBehaviour
{
    // Used by parent Tile script to locate any appplicable cover objects

    public enum CoverSize { half, full }
    public CoverSize coverSize;
    public Vector3 standPoint;

    private void Awake()
    {
        // Finds the stand point for character currently using this cover object
        
        standPoint = GetComponentInChildren<StandPoint>().transform.position;
    }
}
