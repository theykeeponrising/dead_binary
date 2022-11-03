using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEffects : MonoBehaviour
{
    private Transform _effectsContainer;
    private List<GameObject> _activeEffects = new();

    // Start is called before the first frame update
    void Start()
    {
        _effectsContainer = transform.Find("Effects");
    }

    public GameObject CreateEffect(GameObject efxPrefab, Vector3 efxPosition, Quaternion efxRotation)
    {
        // Used to track effects and remove oldest effects if we are above effects limit

        // Instantiate effect and add it to the _effectsContainer
        GameObject effect = Instantiate(efxPrefab, efxPosition, efxRotation, _effectsContainer);
        _activeEffects.Add(effect);

        // Check for and remove excess effects
        if (_activeEffects.Count > GlobalManager.maxEffects)
        {
            GameObject removeEffect = _activeEffects[0];
            _activeEffects.Remove(removeEffect);
            Destroy(removeEffect, 5 * Time.deltaTime / GlobalManager.gameSpeed);
        }

        // Return effect object to initial request
        return effect;
    }
    public Projectile CreateEffect(Projectile efxPrefab, Vector3 efxPosition, Quaternion efxRotation)
    {
        // Used to track effects and remove oldest effects if we are above effects limit

        // Instantiate effect and add it to the list
        GameObject effect = Instantiate(efxPrefab.gameObject, efxPosition, efxRotation, _effectsContainer);
        _activeEffects.Add(effect);

        // Check for and remove excess effects
        if (_activeEffects.Count > GlobalManager.maxEffects)
        {
            GameObject removeEffect = _activeEffects[0];
            _activeEffects.Remove(removeEffect);
            Destroy(removeEffect, 5 * Time.deltaTime / GlobalManager.gameSpeed);
        }

        // Return effect object to initial request
        return effect.GetComponent<Projectile>();
    }

    public GameObject CreateTimedEffect(GameObject efxPrefab, Vector3 efxPosition, Quaternion efxRotation, float timer)
    {
        // Used to track effects and remove oldest effects if we are above effects limit

        // Instantiate effect and add it to the list
        GameObject effect = Instantiate(efxPrefab, efxPosition, efxRotation, _effectsContainer);
        effect.GetComponent<ParticleSystem>().Play(true);
        Destroy(effect, timer);

        // Return effect object to initial request
        return effect;
    }

    public void DestroyEffect(GameObject removeEffect)
    {

        if (!_activeEffects.Contains(removeEffect))
        {
            Debug.Log("Effect {0} remove was called, but cannot be found!", removeEffect);
            return;
        }

        _activeEffects.Remove(removeEffect);
        Destroy(removeEffect);
    }

    public void DestroyEffect(Projectile efx)
    {
        GameObject removeEffect = efx.gameObject;
        DestroyEffect(removeEffect);
    }

    public void AddEffect(Transform efxPrefab)
    {
        efxPrefab.parent = _effectsContainer;
    }

}
