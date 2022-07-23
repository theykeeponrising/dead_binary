using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cover : MonoBehaviour
{
    public enum CoverSize { half, full }
    public CoverSize coverSize;
    public Vector3 standPoint;

    private void Awake()
    {
        standPoint = GetComponentInChildren<StandPoint>().transform.position;
    }
}
