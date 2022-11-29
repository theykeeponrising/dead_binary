using UnityEngine;

public sealed class UnitSparksEffectController: MonoBehaviour
{

    [SerializeField] private ParticleSystem _sparksEffectSystem;
    [SerializeField] private Transform _snappingTarget;

    private Transform _sparksEffectTransform;
    private Unit _observedUnit;
    private (float min, float max) _emissionRange = (5, 25);
    private bool _requireSnapPositionToTarget = false;

    private void Start()
    {
        _observedUnit = GetComponentInParent<Unit>();
        _sparksEffectTransform = _sparksEffectSystem.transform;
        _observedUnit.OnHealthModified += HealthModifiedHandler;
        _observedUnit.OnUnitDied += UnitDiedHandler;
    }

    private void OnDestroy()
    {
        _observedUnit.OnHealthModified -= HealthModifiedHandler;
        _observedUnit.OnUnitDied -= UnitDiedHandler;
    }

    private void LateUpdate()
    {
        if(_requireSnapPositionToTarget)
        {
            _sparksEffectTransform.position = _snappingTarget.position;
        }
    }

    private void HealthModifiedHandler()
    {
        int unitHealth = _observedUnit.Stats.HealthCurrent;
        int maxHealth = _observedUnit.Stats.HealthMax;
        float effectInensity = CalculateIntensity(unitHealth, maxHealth);
        bool isEffectEnabled = effectInensity > 0;
        _requireSnapPositionToTarget = isEffectEnabled;

        SetEffectEmission(effectInensity);
        _sparksEffectTransform.gameObject.SetActive(isEffectEnabled);
    }

    private void UnitDiedHandler()
    {
        _sparksEffectSystem.Stop();
        Destroy(this.gameObject, t: _sparksEffectSystem.main.duration);
    }

    /// <summary>
    /// estimates the missing part.
    /// returns intensity of the effect in [0 < 0.1f] range.
    ///     <example>
    ///     when "current" is 15, "max" is 20, "threshold" is 0.5
    ///     returns 5/10 = 0.5
    ///     </example>
    /// </summary>
    private float CalculateIntensity(float currentHealth, float maxHealth, float threshold = 0.3f)
    {
        if(threshold >= 1.0f || threshold < 0)
            throw new System.ArgumentException("threshold must be in range of [0 < 1.0f]", nameof(threshold));

        float missing = maxHealth - currentHealth;
        float workingZone = maxHealth * (1 - threshold);

        return Mathf.Clamp(missing / workingZone, min: 0, max: 1.0f);
    }

    private void SetEffectEmission(float intensity)
    {
        var effectEmission = _sparksEffectSystem.emission;
        effectEmission.rateOverTime = Mathf.Lerp(_emissionRange.min, _emissionRange.max, intensity);
    }
}
