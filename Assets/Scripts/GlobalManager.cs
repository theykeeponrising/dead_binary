using UnityEngine;
using System.Collections;

public class GlobalManager : MonoBehaviour
{
    // Used for handling constants and other globals
    
    public static float gameSpeed = 1.0f;
    public static float diffMod = 0.1f;
    static Vector3 gravity;
    public Transform initialSpawn;
    public static bool raining;
    public static bool challenge;

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
}
