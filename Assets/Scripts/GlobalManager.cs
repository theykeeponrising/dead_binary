using UnityEngine;
using System.Collections;

public class GlobalManager : MonoBehaviour
{
    // Used for handling constants and other globals
    
    public static float gameSpeed = 1.0f;
    static Vector3 gravity;
    public StateHandler stateHandler;

    // Combat related
    public static float globalHit = 85f;

    private void Awake() 
    {
        stateHandler.Initialize(this);    
    }
    private void Start()
    {
        // gravity = Physics.gravity;
    }

    private void Update()
    {
        //Physics.gravity = gravity * gameSpeed;
        Time.timeScale = gameSpeed;
        //Time.maximumDeltaTime = gameSpeed;
        Time.fixedDeltaTime = gameSpeed * 0.02f;
    }

    public void SetStateHandler(StateHandler stateHandler)
    {
        this.stateHandler = stateHandler;
    }
}
