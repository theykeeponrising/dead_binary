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

    //TODO: Maybe handle these in unit class
    public List<string> flags = new List<string>();

    [HideInInspector] Quaternion aimTowards = Quaternion.identity;

    class HumanBone
    {
        public HumanBodyBones bone;
        public HumanBone(HumanBodyBones setBone)
        {
            bone = setBone;
        }
    }
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

    public enum AnimationEventContext { SHOOT, TAKE_DAMAGE, AIMING, RELOAD, STOW, DRAW, DODGE, VAULT, CROUCH_DOWN, CROUCH_UP, FOOTSTEP_LEFT, FOOTSTEP_RIGHT, IDLE, MOVE };

    public CharacterAnimator(Unit unit)
    {
        this.unit = unit;

        // Animator, bones, and body transforms
        animator = unit.GetComponent<Animator>();
        ragdoll = unit.GetComponentsInChildren<Rigidbody>();
        humanBones = new HumanBone[]
        {
            new HumanBone(HumanBodyBones.Chest),
            new HumanBone(HumanBodyBones.UpperChest),
            new HumanBone(HumanBodyBones.Spine)
        };
        boneTransforms = new Transform[humanBones.Length];
        for (int i = 0; i < humanBones.Length; i++)
            boneTransforms[i] = animator.GetBoneTransform(humanBones[i].bone);

        body.chest = animator.GetBoneTransform(HumanBodyBones.Chest);
        body.head = animator.GetBoneTransform(HumanBodyBones.Head);
        body.handRight = animator.GetBoneTransform(HumanBodyBones.RightHand);
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

    public void SetAnimation()
    {
        // Changes movement animation based on flags

        if (flags.Contains("moving"))
        {
            animator.SetFloat("velocityX", unit.velocityX / GlobalManager.gameSpeed);
            animator.SetFloat("velocityZ", unit.velocityZ / GlobalManager.gameSpeed);
        }
        else
        {
            animator.SetBool("moving", false);
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

    public bool AnimatorIsPlaying()
    {
        // True/False whether an animation is currently playing on the equipped weapon layer.
        // Note -- lengthy transitions will not work

        return animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).length > animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).normalizedTime;
    }

    // void AnimationTransition(AnimationEventContext context)
    // {
    //     // Used to better control when circumstantial animations are played

    //     if (AnimationPause())
    //         return;

    //     switch (context)
    //     {
    //         case (AnimationEventContext.CROUCH_DOWN):
    //             ToggleCrouch(true);
    //             break;
    //         case (AnimationEventContext.CROUCH_UP):
    //             ToggleCrouch(false);
    //             break;
    //     }
    // }

    public bool AnimationPause()
    {
        // True/False if any of the listed animations are playing

        bool[] uninterruptibleAnims = { 
            animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).IsName("Aiming"), 
            animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).IsName("Shoot"), 
            animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).IsName("Crouch-Up"),
            animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).IsName("Crouch")};
        return uninterruptibleAnims.Any(x => x == true);
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
                AddFlag("vaulting");
                animator.Play("Default");
                animator.SetTrigger("vaulting");
                break;

            // Fire weapon effect
            case (AnimationEventContext.SHOOT):
                AddFlag("shooting");
                animator.Play("Shoot");
                break;
            
            case (AnimationEventContext.RELOAD):
                AddFlag("reload");
                animator.Play("Reload");
                break;
            
            case (AnimationEventContext.AIMING):
                animator.SetBool("aiming", true);
                animator.updateMode = AnimatorUpdateMode.AnimatePhysics;

                //Non-monobehaviour, so coroutine called through Character
                unit.StartCoroutine(WaitForAiming());
                break;
            
            case (AnimationEventContext.DODGE):
                AddFlag("dodging");
                break;

            // Stow weapon animation is completed
            case (AnimationEventContext.STOW):
                AddFlag("stowing");
                animator.Play("Stow");
                break;

            // Draw weapon animation is completed
            case (AnimationEventContext.DRAW):
                AddFlag("drawing");
                //animator.Play("Draw", inventory.equippedWeapon.weaponLayer);
                animator.Play("Draw");
                break;

            // Crouch-down
            case (AnimationEventContext.CROUCH_DOWN):
                AddFlag("crouching");
                break;

            //Move
            case (AnimationEventContext.MOVE):
                AddFlag("moving");
                SetBool("moving", true);
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
                RemoveFlag("vaulting");
                break;

            // Fire weapon effect
            case (AnimationEventContext.SHOOT):
                RemoveFlag("shooting");
                break;
            
            case (AnimationEventContext.RELOAD):
                // Reload weapon animation is completed
                RemoveFlag("reload");
                unit.inventory.equippedWeapon.stats.ammoCurrent = unit.inventory.equippedWeapon.stats.ammoMax;
                break;
            
            case (AnimationEventContext.AIMING):
                animator.SetBool("aiming", false);
                animator.updateMode = AnimatorUpdateMode.Normal;
                RemoveFlag("aiming");
                break;
            
            case (AnimationEventContext.DODGE):
                RemoveFlag("dodging");
                break;

            // Stow weapon animation is completed
            case (AnimationEventContext.STOW):
                RemoveFlag("stowing");
                unit.inventory.equippedWeapon.gameObject.SetActive(false);
                animator.SetLayerWeight(unit.inventory.equippedWeapon.weaponLayer, 0);
                break;

            // Draw weapon animation is completed
            case (AnimationEventContext.DRAW):
                RemoveFlag("drawing");
                break;

            // Crouch-down
            case (AnimationEventContext.CROUCH_DOWN):
                RemoveFlag("crouching");
                break;

            //Move
            case (AnimationEventContext.MOVE):
                RemoveFlag("moving");
                SetBool("moving", false);
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
                unit.GetActor().targetCharacter.GetAnimator().TakeDamageEffect(unit.inventory.equippedWeapon);
                break;
        }
    }

    public void SetBool(string flag, bool state)
    {
        animator.SetBool(flag, state);
    }

    public void AddFlag(string flag)
    {
        // Handler for adding new flags
        // Used to prevent duplicate flags

        if (!flags.Contains(flag))
            flags.Add(flag);
    }

    public void RemoveFlag(string flag)
    {
        // Handler for adding new flags
        // Used for consistency with AddFlag function

        if (flags.Contains(flag))
            flags.Remove(flag);
    }

    IEnumerator WaitForAiming()
    {
        // Small buffer to prevent janky torso twist

        while (flags.Contains("crouching")) yield return new WaitForSeconds(0.01f);
        yield return new WaitForSeconds(0.25f);
        AddFlag("aiming");
    }

    public bool GetFlag(string flag)
    {
        return flags.Contains(flag);
    }

    public void ToggleCrouch(bool crouching, bool instant)
    {
        if (!crouching)
        {
            animator.SetBool("crouching", false);
            if (instant) RemoveFlag("crouching");
        }
        else
        {
            animator.SetBool("crouching", true);
            if (instant) AddFlag("crouching");
        }
    }

    IEnumerator SetAiming()
    {
        animator.SetBool("aiming", true);
        animator.updateMode = AnimatorUpdateMode.AnimatePhysics;

        yield return new WaitForSeconds(0.5f);
        AddFlag("aiming");
        yield return new WaitForSeconds(0.25f);
    }

    void AimGetTarget()
    {
        // Twists characters torso to aim gun at target

        if (!GetFlag("aiming") || !useTorsoTwist)
            return;
        
        // Iterations improve accuracy of aim position
        int iterations = 10;

        for (int i = 0; i < iterations; i++)
        {
            Vector3 targetPosition = unit.GetActor().GetTargetPosition();
            Vector3 targetDirection = targetPosition - unit.inventory.equippedWeapon.transform.position;
            unit.GetComponentInChildren<CharacterCamera>().AdjustAngle(targetDirection.x, targetPosition);

            for (int b = 0; b < boneTransforms.Length; b++)
            {
                // Gets the rotation needed to point weapon at enemy
                Transform bone = boneTransforms[b];
                Vector3 aimDirection = unit.inventory.equippedWeapon.transform.forward;
                

                // Updates rotation up until the actual shoot animation happens
                if (GetFlag("aiming"))
                    aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);

                // Gets absolute angle
                float dot = Vector3.Dot(targetDirection.normalized, unit.transform.forward);
                float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

                // Rotates the character to face the target
                if (Mathf.Abs(angle) > 70f) unit.transform.LookAt(new Vector3(targetPosition.x, 0f, targetPosition.z));
                bone.rotation = (aimTowards * bone.rotation).normalized;

                // TO DO -- Fix weirdness during shoot animation
            }
        }
    }

    public Transform GetBoneTransform(HumanBodyBones bone) 
    {
        return animator.GetBoneTransform(bone);
    }

    public void TakeDamageEffect(Weapon weapon=null)
    {
        // Animation that plays when character is taking damage
        // Damage is not actually applied in this function

        if (GetFlag("dodging"))
        {
            // TO DO -- Play dodging animation instead
            return;
        }

        // Play impact sound
        unit.GetSFX().PlayRandomImpactSound();

        // Effect shown when character is hit
        if (animator.GetCurrentAnimatorStateInfo(unit.inventory.equippedWeapon.weaponLayer).IsName("Damage2"))
            animator.Play("Damage3", 0, normalizedTime: .1f);
        else if (weapon.weaponImpact == Weapon.WeaponImpact.HEAVY)
            animator.Play("Damage1");
        else
            animator.Play("Damage2");
    }

    public void OnDeath(Vector3 force, ForceMode mode)
    {
        SetEnabled(false);
        GameObject.Destroy(ragdoll[0]);
        
        // Enable bodypart physics for the ragdoll effect
        foreach (Rigidbody rag in ragdoll)
        {
            rag.isKinematic = false;
            rag.GetComponent<Collider>().isTrigger = false;
        }

        // Apply impact force to center of mass
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
}