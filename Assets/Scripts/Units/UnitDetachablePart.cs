using UnityEngine;

public class UnitDetachablePart : MonoBehaviour
{
    private Unit _unit;
    private CharacterJoint _joint;
    [SerializeField] private bool _alwaysDetach;

    private void Start()
    {
        _unit = GetComponentInParent<Unit>();
        _joint = GetComponent<CharacterJoint>();
    }

    private void Update()
    {
        if (_unit.IsDead())
        {
            Detach();
            enabled = false;
        }
    }

    private void Detach()
    {
        // 50% chance of detaching joint by default
        bool detachJoint = (Random.Range(0, 100) % 2 == 0);

        if (detachJoint || _alwaysDetach)
        {
            Map.MapEffects.AddEffect(transform);
            if (_joint) Destroy(_joint);
        }
    }
}
