using UnityEngine;

public class UnitHover : MonoBehaviour
{
    private Unit _unit;
    private Transform _transform;
    private Vector3 _startPosition;
    private bool _hoverUpwards = true;
    private bool _hoverRightwards = true;
    private bool _hoverForwards = true;
    [SerializeField] private Vector3 _hoverDistance;
    [SerializeField] private Vector3 _hoverDampening;

    private float CurrentX { get { return _transform.localPosition.x; } }
    private float CurrentY { get { return _transform.localPosition.y; } }
    private float CurrentZ { get { return _transform.localPosition.z; } }
    private float MaxX { get { return _startPosition.x + _hoverDistance.x; } }
    private float MinX { get { return _startPosition.x - _hoverDistance.x; } }
    private float MaxY { get { return _startPosition.y + _hoverDistance.y; } }
    private float MinY { get { return _startPosition.y - _hoverDistance.y; } }
    private float MaxZ { get { return _startPosition.z + _hoverDistance.z; } }
    private float MinZ { get { return _startPosition.z - _hoverDistance.z; } }

    private void Start()
    {
        _unit = GetComponent<Unit>();
        _transform = _unit.GetBoneTransform(HumanBodyBones.Chest);
        _startPosition = _transform.localPosition;
    }

    private void FixedUpdate()
    {
        Hover();
    }

    private void Hover()
    {
        if (_unit.IsDead())
        {
            enabled = false;
            return;
        }

        Vector3 floatVector = new();

        floatVector.x = (_hoverRightwards ? Time.deltaTime : -Time.deltaTime) * Random.Range(0.5f, 1) / _hoverDampening.x;
        floatVector.y = (_hoverUpwards ? Time.deltaTime : -Time.deltaTime) * Random.Range(0.5f, 1) / _hoverDampening.y;
        floatVector.z = (_hoverForwards ? Time.deltaTime : -Time.deltaTime) * Random.Range(0.5f, 1) / _hoverDampening.z;

        if (CurrentY > MaxY)
            _hoverUpwards = false;
        else if (CurrentY < MinY)
            _hoverUpwards = true;

        if (CurrentX > MaxX)
            _hoverRightwards = false;
        else if (CurrentX < MinX)
            _hoverRightwards = true;

        if (CurrentZ > MaxZ)
            _hoverForwards = false;
        else if (CurrentZ < MinZ)
            _hoverForwards = true;

        _transform.localPosition = Vector3.Lerp(_transform.localPosition, _transform.localPosition + floatVector, Time.deltaTime);
    }
}
