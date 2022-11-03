using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private LineRenderer _bulletLine;

    private UnitActionShoot _action;
    private Vector3 _currentPosition;
    private Vector3 _previousPosition;
    private Vector3 _destination;
    private float _speed;
    private bool _triggered;

    private void Awake()
    {
        _bulletLine = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        _currentPosition = transform.position;
    }

    public void Init(UnitActionShoot action, Vector3 destination, float speed)
    {
        _action = action;
        _destination = destination;
        _speed = speed;
    }

    private void Update()
    {
        MoveTowardsDestination();
        CheckDestinationReached();
    }

    private void LateUpdate()
    {
        UpdateBulletLine();
    }

    private void UpdateBulletLine()
    {
        if (!_bulletLine)
            return;

        _bulletLine.enabled = !_triggered;
        _previousPosition = _currentPosition;
        _currentPosition = transform.position;

        _bulletLine.SetPosition(0, _previousPosition);
        _bulletLine.SetPosition(1, _currentPosition);
    }

    private void MoveTowardsDestination()
    {
        transform.position = Vector3.MoveTowards(transform.position, _destination, _speed);
    }

    private void CheckDestinationReached()
    {
        if (_triggered)
            return;

        if (Vector3.Distance(transform.position, _destination) < 0.1f)
        {
            _action.TriggerAction(this);
            _triggered = true;
        }
    }
}
