using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEffects : MonoBehaviour
{
    Transform effectsContainer;
    public List<GameObject> activeEffects = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        effectsContainer = transform.Find("Effects");
    }

    public GameObject CreateEffect(GameObject efxPrefab, Vector3 efxPosition, Quaternion efxRotation)
    {
        // Used to track effects and remove oldest effects if we are above effects limit

        // Instantiate effect and add it to the list
        GameObject effect = Instantiate(efxPrefab, efxPosition, efxRotation, effectsContainer);
        activeEffects.Add(effect);

        // Check for and remove excess effects
        if (activeEffects.Count > GlobalManager.maxEffects)
        {
            GameObject removeEffect = activeEffects[0];
            activeEffects.Remove(removeEffect);
            Destroy(removeEffect, 5 * Time.deltaTime / GlobalManager.gameSpeed);
        }

        // Return effect object to initial request
        return effect;
    }

    public GameObject CreateTimedEffect(GameObject efxPrefab, Vector3 efxPosition, Quaternion efxRotation, float timer)
    {
        // Used to track effects and remove oldest effects if we are above effects limit

        // Instantiate effect and add it to the list
        GameObject effect = Instantiate(efxPrefab, efxPosition, efxRotation, effectsContainer);
        effect.GetComponent<ParticleSystem>().Play(true);
        Destroy(effect, timer);

        // Return effect object to initial request
        return effect;
    }

    public void AddEffect(Transform efxPrefab)
    {
        efxPrefab.parent = effectsContainer;
    }

}
