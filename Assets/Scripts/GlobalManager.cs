using UnityEngine;
using System.Collections;

public class GlobalManager : MonoBehaviour
{
    // Used for handling constants and other globals
    
    public static float gameSpeed = 1.0f;
    static Vector3 gravity;
    public StateHandler stateHandler;

    // Distance between neighboring tiles
    public static float tileSpacing = 2.0f;

    // Combat related
    public static float globalHit = 85f;

    private void Awake() 
    {
        stateHandler.Init(this);    
    }

    private void Update()
    {
        //Physics.gravity = gravity * gameSpeed;
        Time.timeScale = gameSpeed;
        //Time.maximumDeltaTime = gameSpeed;
        Time.fixedDeltaTime = gameSpeed * 0.02f;
    }
}
