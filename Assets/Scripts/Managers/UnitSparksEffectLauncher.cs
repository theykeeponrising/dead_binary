using UnityEngine;

public sealed class UnitSparksEffectLauncher: MonoBehaviour
{
    [SerializeField]
    private GameObject _sparksEffect;

    private Unit _observedUnit;

    private void Start()
    {
        _observedUnit = GetComponentInParent<Unit>();
        _observedUnit.OnHealthModified += HealthModifiedHandler;
    }

    private void OnDestroy()
    {
        _observedUnit.OnHealthModified -= HealthModifiedHandler;
    }

    private void HealthModifiedHandler()
    {
        int unitHealth = _observedUnit.stats.healthCurrent;
        int maxHealth = _observedUnit.stats.healthMax;
        bool isEffectEnabled = unitHealth < maxHealth;

        _sparksEffect.SetActive(isEffectEnabled);
    }
}
