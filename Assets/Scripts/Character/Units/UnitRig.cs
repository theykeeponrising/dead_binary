using System.Collections.Generic;
using UnityEngine;

public sealed class UnitRig : MonoBehaviour
{
    private Unit _unit;
    private Animator _animator;
    private HumanBone[] _humanBones;
    private Transform[] _boneTransforms;
    private readonly List<Ragdoll> _ragdoll = new();

    private Rigidbody UnitRigidbody => _unit.UnitRigidBody;
    private Collider[] UnitColliders => _unit.UnitColliders;

    public Transform[] BoneTransforms { get { return _boneTransforms; } }

    private class HumanBone
    {
        public HumanBodyBones Bone;
        public HumanBone(HumanBodyBones bone) { Bone = bone; }
    }

    private class Ragdoll
    {
        private readonly Rigidbody _rigidbody;
        private readonly Collider[] _colliders;

        public Ragdoll(Rigidbody rigidbody)
        {
            _rigidbody = rigidbody;
            _colliders = rigidbody.GetComponents<Collider>();
        }

        public void SetActive(bool isActive)
        {
            _rigidbody.isKinematic = !isActive;
            foreach (Collider collider in _colliders)
                collider.isTrigger = !isActive;
        }
    }

    private void Awake()
    {
        _unit = GetComponentInParent<Unit>();
        _animator = _unit.GetComponent<Animator>();

        _humanBones = new HumanBone[]
        {
            new HumanBone(HumanBodyBones.Chest),
            new HumanBone(HumanBodyBones.UpperChest),
            new HumanBone(HumanBodyBones.Spine)
        };

        // These bones are used for torso rotation
        _boneTransforms = new Transform[_humanBones.Length];
        for (int i = 0; i < _humanBones.Length; i++)
            _boneTransforms[i] = _animator.GetBoneTransform(_humanBones[i].Bone);


        foreach (Rigidbody rigidbody in GetComponentsInChildren<Rigidbody>())
            _ragdoll.Add(new Ragdoll(rigidbody));
    }

    public Transform GetBoneTransform(HumanBodyBones bone)
    {
        return _animator.GetBoneTransform(bone);
    }

    public void SetRagdoll(bool isActive)
    {
        foreach (Collider collider in UnitColliders)
            collider.enabled = false;

        UnitRigidbody.isKinematic = true;

        foreach (Ragdoll ragdoll in _ragdoll)
            ragdoll.SetActive(isActive);
    }
}
