using UnityEngine;

public class UnitHover : MonoBehaviour
{
    private Unit _unit;
    private Transform _transform;
    private Vector3 _startPosition;
    private bool _hoverUpwards = true;
    private bool _hoverRightwards = true;
    private bool _hoverForwards = true;
    [SerializeField] private float _hoverDistance = 0.25f;
    [SerializeField] private float _hoverDampening = 0.5f;

    private float CurrentX { get { return _transform.localPosition.x; } }
    private float CurrentY { get { return _transform.localPosition.y; } }
    private float CurrentZ { get { return _transform.localPosition.z; } }
    private float MaxX { get { return _startPosition.x + _hoverDistance; } }
    private float MinX { get { return _startPosition.x - _hoverDistance; } }
    private float MaxY { get { return _startPosition.y + _hoverDistance; } }
    private float MinY { get { return _startPosition.y - _hoverDistance; } }
    private float MaxZ { get { return _startPosition.z + _hoverDistance; } }
    private float MinZ { get { return _startPosition.z - _hoverDistance; } }

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
        Vector3 floatVector = new();
        System.Random rand = new();

        floatVector.x = (_hoverRightwards ? Time.deltaTime : -Time.deltaTime) * Random.Range(0.5f, 1);
        floatVector.y = (_hoverUpwards ? Time.deltaTime : -Time.deltaTime) * Random.Range(0.5f, 1);
        floatVector.z = (_hoverForwards ? Time.deltaTime : -Time.deltaTime) * Random.Range(0.5f, 1);

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

        _transform.Translate(floatVector / _hoverDampening);
    }
}
