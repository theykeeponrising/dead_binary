using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : DamageItem
{
    ItemProp grenade;
    Timer flashTimer;
    [SerializeField] float flashTime = 05;
    bool flash;

    Transform lightContainer;
    Material lightMaterial;
    Light lightComponent;

    private void Update()
    {
        FlashLight();
    }

    void FlashLight()
    {
        // Flashes the prop light at the flash time interval

        if (grenade == null || lightContainer == null)
            return;

        if (flashTimer.CheckTimer())
        {
            flash = !flash;

            if (flash) lightMaterial.EnableKeyword("_EMISSION");
            else lightMaterial.DisableKeyword("_EMISSION");

            lightComponent.enabled = flash;
            flashTimer.SetTimer(flashTime);
        }
    }

    void SpawnGrenade()
    {
        // Instantiates grenade prop and sets up the callback

        flashTimer = new Timer(flashTime);
        grenade = Instantiate(itemProp, sourceUnit.GetBoneTransform(HumanBodyBones.LeftHand));
        grenade.SetItemEffect(this);
        grenade.SetItemDestination(targetPosition);

        lightContainer = grenade.transform.Find("GrenadeLight");
        if (lightContainer)
        {
            lightMaterial = lightContainer.GetComponent<MeshRenderer>().material;
            lightComponent = lightContainer.GetComponentInChildren<Light>();
        }
    }

    public override void UseItem(Unit setSourceUnit, Vector3 setTargetPosition)
    {
        UseItemOnTargetPosition(setSourceUnit, setTargetPosition);
    }

    public override void UseItem(Unit setSourceUnit, Unit setTargetedUnit)
    {
        UseItemOnTargetPosition(setSourceUnit, setTargetedUnit.transform.position);
    }

    public void UseItemOnTargetPosition(Unit setSourceUnit, Vector3 setTargetPosition)
    {
        // Gets unit information, creates grenade prop, and plays throwing animation

        itemAction.StartPerformance();
        sourceUnit = setSourceUnit;
        targetPosition = setTargetPosition;

        float distance = (sourceUnit.transform.position - targetPosition).magnitude;

        if (distance > MapGrid.TileSpacing * 3)
            sourceUnit.GetComponent<Animator>().Play("Throw-Long");
        else
            sourceUnit.GetComponent<Animator>().Play("Throw-Short");

        SpawnGrenade();
        sourceUnit.ClearTarget();
    }

    public override void TriggerItem()
    {
        // Callback function for props, initiates the item effect

        base.TriggerItem();
    }

    public override void TriggerItem(Vector3 setTriggerPosition)
    {
        // Callback function for props, initiates the item effect

        base.TriggerItem(setTriggerPosition);
    }
}
