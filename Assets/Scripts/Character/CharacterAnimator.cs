using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;




//Handles all the animations+animation logic for a particular character
//Currently includes flags, though maybe that should be moved elsewhere
public class CharacterAnimator
{
    [Header("--Animation")]
    Transform[] boneTransforms;
    Unit unit;

    [HideInInspector] Quaternion aimTowards = Quaternion.identity;

    class HumanBone
    {
        public HumanBodyBones bone;
        public HumanBone(HumanBodyBones setBone)
        {
            bone = setBone;
        }
    }

    public enum AnimationEventContext { SHOOT, TAKE_DAMAGE, AIMING, RELOAD, STOW, DRAW, DODGE, VAULT, FOOTSTEP_LEFT, FOOTSTEP_RIGHT, IDLE, THROW };
    HumanBone[] humanBones;
    Animator animator;
    public class Body
    {
        public Transform handLeft;
        public Transform handRight;
        public Transform chest;
        public Transform head;
        public Transform shoulder;
        public Transform arm;
        public Transform hand;
        public Transform leg;
        public Transform shin;
        public Transform foot;
        public Transform mask;
    }
    public Body body = new Body();
    Rigidbody[] ragdoll;
    [SerializeField] bool useTorsoTwist = true;

    public CharacterAnimator(Unit unit)
    {
        this.unit = unit;

        // Animator, bones, and body transforms
        animator = unit.GetComponent<Animator>();
        ragdoll = unit.GetComponentsInChildren<Rigidbody>();

        // These bones are used for torso rotation
        humanBones = new HumanBone[]
        {
            new HumanBone(HumanBodyBones.Chest),
            new HumanBone(HumanBodyBones.UpperChest),
            new HumanBone(HumanBodyBones.Spine)
        };

        boneTransforms = new Transform[humanBones.Length];
        for (int i = 0; i < humanBones.Length; i++)
            boneTransforms[i] = animator.GetBoneTransform(humanBones[i].bone);

        // These are used for attach points
        body.chest = animator.GetBoneTransform(HumanBodyBones.Chest);
        body.head = animator.GetBoneTransform(HumanBodyBones.Head);
        body.handRight = animator.GetBoneTransform(HumanBodyBones.RightHand);
        body.handLeft = animator.GetBoneTransform(HumanBodyBones.LeftHand);
    }

    public void Update()
    {
        SetAnimation();
    }

    public void LateUpdate()
    {
        AimGetTarget();
    }

    public void SetEnabled(bool enabled)
    {
        animator.enabled = enabled;
    }

    public bool GetEnabled()
    {
        return animator.enabled;
    }

    void SetAnimation()
    {
        // Changes movement animation based on flags

        if (unit.GetActor().FindActionOfType(typeof(UnitActionMove)).Performing())
        {
            animator.SetFloat("velocityX", unit.velocityX / GlobalManager.gameSpeed);
            animator.SetFloat("velocityZ", unit.velocityZ / GlobalManager.gameSpeed);
        }
        else
        {
            //animator.SetBool("moving", false);
            animator.SetFloat("velocityX", 0);
            animator.SetFloat("velocityZ", 0);
        }
    }

    public void SetRagdoll(Rigidbody[] ragdoll)
    {
        this.ragdoll = ragdoll;
    }

    public Transform GetWeaponDefaultPosition()
    {
        return body.handRight.Find("AttachPoint");
    }

    public Vector3 GetCharacterChestPosition()
    {
        return body.chest.transform.position;
    }

    public bool AnimatorIsPlaying(string animationName)
    {
        // True/False whether an animation is currently playing on the equipped weapon layer.
        // Should always be called during a LateUpdate
        // Note -- lengthy transitions will not work

        if (animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).IsName(animationName))
            return animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).length > animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).normalizedTime;
        return false;
    }

    public void ProcessAnimationEvent(AnimationEventContext context, bool state)
    {
        // State == True corresponds to entering an animation, False for exiting one
        if (state) Event_OnAnimationStart(context);
        else Event_OnAnimationEnd(context);
    }

    public void Event_OnAnimationStart(AnimationEventContext context)
    {
        switch (context)
        {
            //TODO: Vault sound in middle of animation?
            case (AnimationEventContext.VAULT):
                unit.AddFlag(FlagType.VAULT);
                animator.Play("Default");
                animator.SetTrigger("vaulting");
                break;
            
            case (AnimationEventContext.AIMING):
                animator.SetBool("aiming", true);

                //Non-monobehaviour, so coroutine called through Character
                unit.StartCoroutine(WaitForAiming());
                break;

            // Idle
            case (AnimationEventContext.IDLE):
                animator.updateMode = AnimatorUpdateMode.Normal;
                break;
            default:
                break;
        }
    }

    public void Event_OnAnimationEnd(AnimationEventContext context)
    {
        // Handler for animation events
        // Evaluate context and perform appropriate actions

        // Weapon shooting effect and sound
        switch (context)
        {
            case (AnimationEventContext.VAULT):
                unit.RemoveFlag(FlagType.VAULT);
                break;
            
            case (AnimationEventContext.AIMING):
                animator.SetBool("aiming", false);
                animator.updateMode = AnimatorUpdateMode.Normal;
                unit.RemoveFlag(FlagType.AIM);
                break;

            // Throw
            case (AnimationEventContext.THROW):
                ThrowItem();
                break;

            default:
                break;
        }
    }

    public void Event_PlayAnimation(AnimationEventContext context)
    {
        // Weapon impact effect on target
        switch (context)
        {
            case (AnimationEventContext.TAKE_DAMAGE):
                unit.GetActor().targetCharacter.GetAnimator().TakeDamageEffect(unit.GetEquippedWeapon());
                break;
        }
    }

    public void SetBool(string flag, bool state)
    {
        // Sets animator bool

        animator.SetBool(flag, state);
    }

    public void SetTrigger(string flag)
    {
        // Sets animator trigger

        animator.SetTrigger(flag);
    }

    public void SetUpdateMode(AnimatorUpdateMode updateMode = AnimatorUpdateMode.Normal)
    {
        // Sets animator update mode, defaults to normal

        animator.updateMode = updateMode;
    }

    IEnumerator WaitForAiming()
    {
        // Small buffer to prevent janky torso twist

        while (IsCrouching()) yield return new WaitForSeconds(0.01f);
        yield return new WaitForSeconds(0.25f);
        unit.AddFlag(FlagType.AIM);
    }

    public void ToggleCrouch(bool instant=false)
    {
        if (instant)
            animator.Play("Crouch");
        else
            animator.SetTrigger("toggleCrouch");
    }

    public bool IsCrouching()
    {
        // Returns true if any crouch animation is playing

        bool[] crouchingAnims = {
            animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).IsName("Crouch-Down"),
            animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).IsName("Crouch-Up"),
            animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).IsName("Crouch"),
            animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).IsName("Crouch-Dodge"),
            animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).IsName("Crouch-Damage")};
        return crouchingAnims.Any(x => x == true);
    }

    public void CoverCrouch()
    {
        // Makes character crouch if they should be crouching behind cover

        if (unit.currentCover && unit.currentCover.coverSize == CoverObject.CoverSize.half)
            if (!IsCrouching()) ToggleCrouch();
    }

    void AimGetTarget()
    {
        // Twists characters torso to aim gun at target

        // Only continue if we have a valid target
        if (!unit.GetActor().targetCharacter)
            return;

        // Initial camera position should snap immediately
        Vector3 targetPosition = unit.GetActor().GetTargetPosition(true);
        Vector3 targetDirection = targetPosition - unit.inventory.equippedWeapon.transform.position;
        unit.GetComponentInChildren<CharacterCamera>().AdjustAngle(targetDirection.x, targetPosition);

        // If we are crouching or not using torso twist, then skip the bone rotations
        if (IsCrouching() || !useTorsoTwist)
            return;

        // If we are not aiming or shooting, then skip the bone rotations
        if (!animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).IsName("Aiming") 
            && !animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).IsName("Shoot"))
            return;
        
        // Iterations improve accuracy of aim position
        int iterations = 10;

        for (int i = 0; i < iterations; i++)
        {
            targetPosition = unit.GetActor().GetTargetPosition();
            targetDirection = targetPosition - unit.inventory.equippedWeapon.transform.position;
            unit.GetComponentInChildren<CharacterCamera>().AdjustAngle(targetDirection.x, targetPosition);

            for (int b = 0; b < boneTransforms.Length; b++)
            {
                // Gets the rotation needed to point weapon at enemy
                Transform bone = boneTransforms[b];
                Vector3 aimDirection = unit.inventory.equippedWeapon.transform.forward;
                
                // Updates rotation up until the actual shoot animation happens
                if (unit.GetFlag(FlagType.AIM))
                    aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);

                // Gets absolute angle
                float dot = Vector3.Dot(targetDirection.normalized, unit.transform.forward);
                float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

                // Rotates the character to face the target
                //if (Mathf.Abs(angle) > 70f)
                    unit.transform.LookAt(new Vector3(targetPosition.x, 0f, targetPosition.z));
                bone.rotation = (aimTowards * bone.rotation).normalized;

                // TO DO -- Fix weirdness during shoot animation
            }
        }
    }

    public Transform GetBoneTransform(HumanBodyBones bone) 
    {
        return animator.GetBoneTransform(bone);
    }

    void ThrowItem()
    {
        // Releases item from hand and starts item movement

        ItemProp itemProp = body.handLeft.GetComponentInChildren<ItemProp>();
        GlobalManager.Instance.activeMap.AddEffect(itemProp);
        itemProp.SetItemMovement(true);
        CoverCrouch();
    }

    public void TakeDamageEffect(Weapon weapon = null, DamageItem item = null)
    {
        // Animation that plays when character is taking damage
        // Damage is not actually applied in this function

        // If unit is dodging, skip damage effect
        if (AnimatorIsPlaying("Dodge") || AnimatorIsPlaying("Crouch-Dodge")) return;

        // Play impact sound
        unit.GetSFX().PlayRandomImpactSound();

        // Effect shown when character is hit
        if (animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).IsName("Crouch"))
            animator.Play("Crouch-Damage");
        else if (animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).IsName("Crouch-Damage"))
            animator.Play("Crouch-Damage", 0, normalizedTime: .1f);
        else if (animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).IsName("Damage2"))
            animator.Play("Damage3", 0, normalizedTime: .1f);
        else if (weapon && weapon.weaponImpact == Weapon.WeaponImpact.HEAVY)
            animator.Play("Damage1");
        else if (item && item.itemImpact == Weapon.WeaponImpact.HEAVY)
            animator.Play("Damage1");
        else
            animator.Play("Damage2");

        CoverCrouch();
    }

    public void OnDeath(Vector3 force, ForceMode mode)
    {
        SetEnabled(false);
        
        // Enable bodypart physics for the ragdoll effect
        foreach (Rigidbody rag in ragdoll)
        {
            if (!rag) continue;
            rag.isKinematic = false;
            rag.GetComponent<Collider>().isTrigger = false;
        }

        // Apply impact force to center of mass
        GameObject.Destroy(ragdoll[0]);
        body.chest.GetComponent<Rigidbody>().AddForce(force, mode);
    }

    public void SetLayerWeight(int layerIndex, float weight)
    {
        animator.SetLayerWeight(layerIndex, weight);
    }

    public void SetAnimationSpeed(float animSpeed)
    {
        animator.SetFloat("animSpeed", animSpeed);
    }

    public void Play(string animation)
    {
        // Quick reference to play animation

        animator.Play(animation);
    }
}