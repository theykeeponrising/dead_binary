using System.Collections.Generic;
using UnityEngine;

public sealed class UnitRig : MonoBehaviour
{
    private Unit _unit;
    private Animator _animator;
    private HumanBodyBones[] _humanBones;
    private Transform[] _boneTransforms;
    private readonly List<Ragdoll> _ragdolls = new();

    private Rigidbody UnitRigidbody => _unit.Rigidbody;
    private Collider[] UnitColliders => _unit.Colliders;
    public Transform[] BoneTransforms=> _boneTransforms;

    private void Awake()
    {
        _unit = GetComponentInParent<Unit>();
        _animator = _unit.GetComponent<Animator>();

        _humanBones = new HumanBodyBones[]
        {
            HumanBodyBones.Chest,
            HumanBodyBones.UpperChest,
            HumanBodyBones.Spine
        };

        // These bones are used for torso rotation
        _boneTransforms = new Transform[_humanBones.Length];
        for (int i = 0; i < _humanBones.Length; i++)
            _boneTransforms[i] = _animator.GetBoneTransform(_humanBones[i]);

        foreach (Rigidbody rigidbody in GetComponentsInChildren<Rigidbody>())
            _ragdolls.Add(new Ragdoll(rigidbody));
    }

    public Transform GetBoneTransform(HumanBodyBones bone)
    {
        if (_animator.GetBoneTransform(bone))
            return _animator.GetBoneTransform(bone);
        return _unit.transform;
    }

    public void SetRagdollActive(bool isActive)
    {
        foreach (Collider collider in UnitColliders)
            collider.enabled = false;

        UnitRigidbody.isKinematic = true;

        foreach (Ragdoll ragdoll in _ragdolls)
            ragdoll.SetActive(isActive);
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
}
