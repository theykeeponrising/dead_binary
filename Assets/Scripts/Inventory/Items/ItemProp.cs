using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemProp : MonoBehaviour
{
    public enum TriggerType { DESTINATION, TIMED, SET }
    [Tooltip("How and when the prop will trigger the item effect.")]
    public TriggerType triggerType;
    [Tooltip("Slows the prop's trajectory if thrown. Higher values will slow the prop more.")]
    [SerializeField] [Range(100f, 200f)] float dampening = 150f;
    [Tooltip("Whether or not the prop should rotate while in motion.")]
    public bool propRotates;

    DamageItem itemEffect;
    Vector3 arcHeight;
    Vector3 destination;
    float distance;
    bool allowMovement = false;

    private void LateUpdate()
    {
        MoveItem();
        CheckDestination();
    }

    public void SetItemEffect(DamageItem setItem)
    {
        // Callback for when prop triggers

        itemEffect = setItem;
    }

    public void SetItemDestination(Vector3 setDestination)
    {
        // Destination for movement

        destination = setDestination;
        distance = (transform.position - destination).magnitude;
    }

    public void SetItemMovement(bool canMove)
    {
        // Allow prop to begin movement

        allowMovement = canMove;
        arcHeight = (Vector3.up * distance / 3);
    }

    public void TriggerItem()
    {
        // Trigger item effect and remove this prop

        itemEffect.TriggerItem(transform.position);
        Destroy(gameObject);
    }

    void MoveItem()
    {
        // Prop moves towards target

        if (!allowMovement || destination == null)
            return;

        // Add arc to the trajectory
        Vector3 destinationArc = destination + arcHeight;

        // Degrade arc height to create arcing effect
        arcHeight = arcHeight - (Vector3.up * 0.3f) * (distance / dampening);

        // Perform move over time
        transform.position = Vector3.MoveTowards(transform.position, destinationArc, distance / dampening);
        RotateItem();

    }

    void RotateItem()
    {
        // Basic rotation for a mid-air prop

        if (!propRotates) return;

        transform.Rotate(new Vector3(-50f, -50f, -50f) * (distance / dampening));
    }

    void CheckDestination()
    {
        // Check if prop has reached destination, and trigger if so

        if (triggerType != TriggerType.DESTINATION)
                return;

        if ((transform.position - destination).magnitude <= 0.25f)
            TriggerItem();
    }
}
