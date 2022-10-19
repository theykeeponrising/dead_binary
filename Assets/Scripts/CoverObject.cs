using System.Collections.Generic;
using UnityEngine;

public sealed class CoverObject : MonoBehaviour
{
    // Used by parent Tile script to locate any appplicable cover objects

    AudioSource _audioSource;
    CoverObject[] _childrenCoverObjects;
    Rigidbody[] _rigidbodies;
    readonly List<Tile> _coveredTiles = new();

    [Tooltip("Stand Points should designate a collider via the inspector.")]
    [SerializeField] Collider _mainCollider;

    // Move these to an appropriate dict once difficulty settings are added
    private const int _coverBonusFull = 60;
    private const int _coverBonusHalf = 45;

    public CoverSizes CoverSize;
    public ImpactTypes ImpactType;
    public bool IsVaultable;
    public bool IsDestructible = false;
    public bool IsDestroyed = false;

    readonly Timer _debugTimer = new(10f); // Temp to test destruction physics
   

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _childrenCoverObjects = GetComponentsInChildren<CoverObject>();
        _rigidbodies = GetComponentsInChildren<Rigidbody>();

        InitCollider();
    }

    private void Update()
    {
        // Temp timer to test destruction physics
        // Destruction will normally be called externally and not performed via Update

        if (_debugTimer.CheckTimer() && !IsDestroyed && IsDestructible)
        {
            DestroyObject();
        }
    }

    private void InitCollider()
    {
        // If no collider was provided by the inspector, use first found collider

        if (!_mainCollider) 
            _mainCollider = GetComponentInChildren<Collider>();
    }

    public void RegisterTile(Tile tile)
    {
        // Register tiles for use when cover is destroyed

        if (!_coveredTiles.Contains(tile))
            _coveredTiles.Add(tile);
    }

    public Vector3 GetStandPoint(Tile tile)
    {
        // Calculates a stand position based on the cover object's collider size

        Vector3 tilePosition = tile.transform.position;
        Vector3 coverPosition = transform.position;

        float offset = _mainCollider.bounds.size.z / 2;
        Vector3 direction = tilePosition - coverPosition;
        Vector3 standPoint = (tilePosition + coverPosition) / 2;

        return new Vector3(standPoint.x, 0, standPoint.z) + direction * offset;
    }

    public void PlayImpactSFX()
    {
        // Impact noise that is played when character is protected by cover

        AudioClip audioClip = AudioManager.GetSound(ImpactType);
        _audioSource.PlayOneShot(audioClip);
    }

    public int GetCoverBonus()
    {
        // Returns dodge chance percent bonus provided by cover

        if (CoverSize == CoverSizes.full) 
            return _coverBonusFull;
        else 
            return _coverBonusHalf;
    }

    public void DestroyObject()
    {
        // For each physics child found, enable collider and physics
        IsDestroyed = true;

        foreach (Rigidbody rigidbody in _rigidbodies)
        {
            rigidbody.GetComponent<Collider>().enabled = true;
            rigidbody.isKinematic = false;
        }

        // Remove this object as a valid cover piece for tiles
        foreach (Tile tile in _coveredTiles)
            tile.cover = null;

        foreach (CoverObject cover in _childrenCoverObjects)
        {
            if (cover == this) continue;
            cover.DestroyObject();
        }
    }
}

public enum CoverSizes { half, full }
