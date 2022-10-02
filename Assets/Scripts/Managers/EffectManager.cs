using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    Unit unit;
    [SerializeField] List<GameObject> effects = new List<GameObject>();
    bool hasSparks = false;
    bool displaySparks = false;
    GameObject sparks;
    // Start is called before the first frame update
    void Start()
    {
        unit = GetComponentInParent<Unit>();
        foreach (var effect in effects)
        {
            if (effect.name == "Sparks")
            {
                hasSparks = true;
                sparks = effect;
                var sparksLight = sparks.GetComponent<ParticleSystem>().lights;
                sparksLight.light = GameObject.Find("Spark Light Template").GetComponent<Light>();
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (unit.stats.healthCurrent >= unit.stats.healthMax)
        {
            if (hasSparks && displaySparks)
            {
                sparks.SetActive(false);
                displaySparks = false;
                Debug.Log("deactivate sparks");
            }
        }
        else if (hasSparks && !displaySparks)
        {
            Debug.Log("activate sparks");
            sparks.SetActive(true);
            displaySparks = true;
        }
    }
    
}
