using UnityEngine;
using System.Collections;

public class GlobalManager : MonoBehaviour
{
    // Used for handling constants and other globals
    public static GlobalManager Instance = null;

    public static float gameSpeed = 1.0f;
    static Vector3 gravity;
    public StateHandler stateHandler;

    // Distance between neighboring tiles
    public static float tileSpacing = 2.0f;

    // Combat related
    public static float globalHit = 100.0f;

    [Range(10f, 100f)] public static float maxEffects = 20f;

    public static Map ActiveMap;
    [SerializeField] Map _activeMap;

    private void Awake() 
    {
        Instance = this;
        ActiveMap = _activeMap;
    }

    private void Update()
    {
        //Physics.gravity = gravity * gameSpeed;
        Time.timeScale = gameSpeed;
        //Time.maximumDeltaTime = gameSpeed;
        Time.fixedDeltaTime = gameSpeed * 0.02f;
    }
}
