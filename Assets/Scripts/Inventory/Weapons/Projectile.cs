using UnityEngine;

public class Projectile : MonoBehaviour
{
    private UnitActionShoot _action;
    private float _speed;

    protected LineRenderer BulletLine;

    protected Vector3 CurrentPosition;
    protected Vector3 PreviousPosition;
    protected Vector3 Destination;
    protected bool Triggered;

    public float Lifetime = 0f;

    protected virtual void Awake()
    {
        BulletLine = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        CurrentPosition = transform.position;
    }

    public void Init(UnitActionShoot action, Vector3 destination, float speed)
    {
        _action = action;
        _speed = speed;
        Destination = destination;
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

    protected virtual void UpdateBulletLine()
    {
        if (!BulletLine)
            return;

        BulletLine.enabled = !Triggered;
        PreviousPosition = CurrentPosition;
        CurrentPosition = transform.position;

        BulletLine.SetPosition(0, PreviousPosition);
        BulletLine.SetPosition(1, CurrentPosition);
    }

    private void MoveTowardsDestination()
    {
        transform.position = Vector3.MoveTowards(transform.position, Destination, _speed);
    }

    private void CheckDestinationReached()
    {
        if (Triggered)
            return;

        if (Vector3.Distance(transform.position, Destination) < 0.1f)
        {
            _action.TriggerAction(this);
            Triggered = true;
        }
    }
}
