using System.Linq;
using UnityEngine;

//Handles all the animations+animation logic for a particular character

public class CharacterAnimator
{
    private readonly Unit _unit;
    private readonly Animator _animator;
    private readonly UnitRig _unitRig;

    private Quaternion _aimTowards = Quaternion.identity;
    [SerializeField] private bool _useTorsoTwist = true;

    private int AnimationLayer => _unit.EquippedWeapon.GetAnimationLayer();
    private Transform[] BoneTransforms => _unitRig.BoneTransforms;

    public enum AnimationEventContext { SHOOT, TAKE_DAMAGE, AIMING, RELOAD, STOW, DRAW, DODGE, VAULT, FOOTSTEP_LEFT, FOOTSTEP_RIGHT, IDLE, THROW };

    public CharacterAnimator(Unit unit)
    {
        _unit = unit;
        _unitRig = unit.GetComponentInChildren<UnitRig>();
        _animator = unit.GetComponent<Animator>();
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
        _animator.enabled = enabled;
    }

    public bool GetEnabled()
    {
        return _animator.enabled;
    }

    void SetAnimation()
    {
        // Changes movement animation based on flags

        if (_unit.GetActor().FindActionOfType(typeof(UnitActionMove)).Performing())
        {
            _animator.SetFloat("velocityX", _unit.velocityX / GlobalManager.gameSpeed);
            _animator.SetFloat("velocityZ", _unit.velocityZ / GlobalManager.gameSpeed);
        }
        else
        {
            //animator.SetBool("moving", false);
            _animator.SetFloat("velocityX", 0);
            _animator.SetFloat("velocityZ", 0);
        }
    }

    public Transform GetWeaponDefaultPosition()
    {
        return _unitRig.GetBoneTransform(HumanBodyBones.RightHand).Find("AttachPoint");
    }

    public Vector3 GetCharacterChestPosition()
    {
        return _unitRig.GetBoneTransform(HumanBodyBones.Chest).position;
    }

    public Transform GetBoneTransform(HumanBodyBones bone)
    {
        return _animator.GetBoneTransform(bone);
    }

    public bool AnimatorIsPlaying(string animationName)
    {
        // True/False whether an animation is currently playing on the equipped weapon layer.
        // Should always be called during a LateUpdate
        // Note -- lengthy transitions will not work

        if (_animator.GetCurrentAnimatorStateInfo(AnimationLayer).IsName(animationName))
            return _animator.GetCurrentAnimatorStateInfo(AnimationLayer).length > _animator.GetCurrentAnimatorStateInfo(AnimationLayer).normalizedTime;
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
                _unit.AddFlag(FlagType.VAULT);
                _animator.Play("Default");
                _animator.SetTrigger("vaulting");
                break;
            
            case (AnimationEventContext.AIMING):
                _unit.AddFlag(FlagType.AIM);
                _animator.SetBool("aiming", true);
                break;

            // Idle
            case (AnimationEventContext.IDLE):
                _animator.updateMode = AnimatorUpdateMode.Normal;
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
                _unit.RemoveFlag(FlagType.VAULT);
                break;
            
            case (AnimationEventContext.AIMING):
                _animator.SetBool("aiming", false);
                _animator.updateMode = AnimatorUpdateMode.Normal;
                _unit.RemoveFlag(FlagType.AIM);
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
                _unit.GetActor().targetCharacter.GetAnimator().TakeDamageEffect(_unit.EquippedWeapon);
                break;
        }
    }

    public void SetBool(string flag, bool state)
    {
        // Sets animator bool

        _animator.SetBool(flag, state);
    }

    public void SetTrigger(string flag)
    {
        // Sets animator trigger

        _animator.SetTrigger(flag);
    }

    public void SetUpdateMode(AnimatorUpdateMode updateMode = AnimatorUpdateMode.Normal)
    {
        // Sets animator update mode, defaults to normal

        _animator.updateMode = updateMode;
    }

    public void ToggleCrouch(bool instant=false)
    {
        if (instant)
            _animator.Play("Crouch");
        else
            _animator.SetTrigger("toggleCrouch");
    }

    public bool IsCrouching()
    {
        // Returns true if any crouch animation is playing

        bool[] crouchingAnims = {
            _animator.GetCurrentAnimatorStateInfo(AnimationLayer).IsName("Crouch-Down"),
            _animator.GetCurrentAnimatorStateInfo(AnimationLayer).IsName("Crouch-Up"),
            _animator.GetCurrentAnimatorStateInfo(AnimationLayer).IsName("Crouch"),
            _animator.GetCurrentAnimatorStateInfo(AnimationLayer).IsName("Crouch-Dodge"),
            _animator.GetCurrentAnimatorStateInfo(AnimationLayer).IsName("Crouch-Damage")};
        return crouchingAnims.Any(x => x == true);
    }

    public void CoverCrouch()
    {
        // Makes character crouch if they should be crouching behind cover

        if (_unit.currentCover && _unit.currentCover.CoverSize == CoverSizes.half)
            if (!IsCrouching()) ToggleCrouch();
    }

    void AimGetTarget()
    {
        // Twists characters torso to aim gun at target

        // Only continue if we have a valid target
        if (!_unit.GetActor().targetCharacter)
            return;

        // Initial camera position should snap immediately
        Vector3 targetPosition = _unit.GetActor().GetTargetPosition(true);
        Vector3 targetDirection = targetPosition - _unit.inventory.equippedWeapon.transform.position;
        _unit.GetComponentInChildren<CharacterCamera>().AdjustAngle(targetDirection.x, targetPosition);

        // If we are crouching or not using torso twist, then skip the bone rotations
        if (IsCrouching() || !_useTorsoTwist)
            return;

        // If we are not aiming or shooting, then skip the bone rotations
        if (!_animator.GetCurrentAnimatorStateInfo(AnimationLayer).IsName("Aiming") 
            && !_animator.GetCurrentAnimatorStateInfo(AnimationLayer).IsName("Shoot"))
            return;
        
        // Iterations improve accuracy of aim position
        int iterations = 10;

        for (int i = 0; i < iterations; i++)
        {
            targetPosition = _unit.GetActor().GetTargetPosition();
            targetDirection = targetPosition - _unit.inventory.equippedWeapon.transform.position;
            _unit.GetComponentInChildren<CharacterCamera>().AdjustAngle(targetDirection.x, targetPosition);

            for (int b = 0; b < BoneTransforms.Length; b++)
            {
                // Gets the rotation needed to point weapon at enemy
                Transform bone = BoneTransforms[b];
                Vector3 aimDirection = _unit.inventory.equippedWeapon.transform.forward;
                
                // Updates rotation up until the actual shoot animation happens
                if (_unit.GetFlag(FlagType.AIM))
                    _aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);

                // Gets absolute angle
                float dot = Vector3.Dot(targetDirection.normalized, _unit.transform.forward);
                float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

                // Rotates the character to face the target
                if (Mathf.Abs(angle) > 70f) _unit.transform.LookAt(new Vector3(targetPosition.x, 0f, targetPosition.z));
                bone.rotation = (_aimTowards * bone.rotation).normalized;

                // TO DO -- Fix weirdness during shoot animation
            }
        }
    }

    void ThrowItem()
    {
        // Releases item from hand and starts item movement

        ItemProp itemProp = _unitRig.GetBoneTransform(HumanBodyBones.LeftHand).GetComponentInChildren<ItemProp>();
        GlobalManager.ActiveMap.AddEffect(itemProp);
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
        _unit.GetSFX().PlayRandomImpactSound();

        // Effect shown when character is hit
        if (_animator.GetCurrentAnimatorStateInfo(AnimationLayer).IsName("Crouch"))
            _animator.Play("Crouch-Damage");
        else if (_animator.GetCurrentAnimatorStateInfo(AnimationLayer).IsName("Crouch-Damage"))
            _animator.Play("Crouch-Damage", 0, normalizedTime: .1f);
        else if (_animator.GetCurrentAnimatorStateInfo(AnimationLayer).IsName("Damage2"))
            _animator.Play("Damage3", 0, normalizedTime: .1f);
        else if (weapon && weapon.GetImpact() == WeaponImpact.HEAVY)
            _animator.Play("Damage1");
        else if (item && item.itemImpact == WeaponImpact.HEAVY)
            _animator.Play("Damage1");
        else
            _animator.Play("Damage2");

        CoverCrouch();
    }

    public void OnDeath(Vector3 force, ForceMode mode)
    {
        SetEnabled(false);

        // Enable bodypart physics for the ragdoll effect
        _unitRig.SetRagdollActive(true);

        // Apply impact force to center of mass
        _unitRig.GetBoneTransform(HumanBodyBones.Chest).GetComponent<Rigidbody>().AddForce(force, mode);
    }

    public void SetLayerWeight(int layerIndex, float weight)
    {
        _animator.SetLayerWeight(layerIndex, weight);
    }

    public void SetAnimationSpeed(float animSpeed)
    {
        _animator.SetFloat("animSpeed", animSpeed);
    }

    public void Play(string animation)
    {
        // Quick reference to play animation

        _animator.Play(animation);
    }
}
