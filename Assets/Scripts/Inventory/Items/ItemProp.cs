using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemProp : MonoBehaviour
{
    public enum TriggerType { DESTINATION, TIMED, SET }
    public TriggerType triggerType;
    DamageItem itemEffect;

    Vector3 arcHeight;
    Vector3 destination;
    float distance;
    bool allowMovement = false;

    private void Update()
    {
        MoveItem();
        CheckDestination();
    }

    public void SetItemEffect(DamageItem setItem)
    {
        // Callback for when item triggers

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
        // Allow item to begin movement

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
        // Item moves towards target

        if (!allowMovement || destination == null)
            return;

        // Add arc to the trajectory
        Vector3 destinationArc = destination + arcHeight;

        // Degrade arc height to create arcing effect
        arcHeight = arcHeight - (Vector3.up * 0.03f);

        // Perform move over time
        transform.position = Vector3.MoveTowards(transform.position, destinationArc, 0.1f);
        RotateItem();

    }

    void RotateItem()
    {
        transform.Rotate(new Vector3(-5f, -5f, -5f));
    }

    void CheckDestination()
    {
        // Check if item has reached destination, and trigger if so

        if (triggerType != TriggerType.DESTINATION)
                return;

        if ((transform.position - destination).magnitude <= 0.25f)
            TriggerItem();
    }
}
