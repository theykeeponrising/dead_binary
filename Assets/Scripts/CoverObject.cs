using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class CoverObject : MonoBehaviour
{
    // Used by parent Tile script to locate any appplicable cover objects

    private AudioSource _audioSource;
    private CoverObject[] _childrenCoverObjects;
    private Rigidbody[] _rigidbodies;
    private readonly List<Tile> _coveredTiles = new();

    [Tooltip("Stand Points should designate a collider via the inspector.")]
    [SerializeField] private Collider _mainCollider;
    [SerializeField] private AudioClip _destructionSound;
    [SerializeField] private float _debugDestructionTimer;

    // Move these to an appropriate dict once difficulty settings are added
    private const int _coverBonusFull = 60;
    private const int _coverBonusHalf = 45;

    public CoverSizes CoverSize;
    public ImpactTypes ImpactType;
    public bool IsVaultable;
    public bool IsDestructible = false;
    public bool IsDestroyed = false;

    private readonly Timer _debugTimer = new(2f); // Temp to test destruction physics
   

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _childrenCoverObjects = GetComponentsInChildren<CoverObject>();
        _rigidbodies = GetComponentsInChildren<Rigidbody>();
        _debugTimer.SetTimer(_debugDestructionTimer);

        InitCollider();
    }

    //private void Update()
    //{
    //    // Temp timer to test destruction physics
    //    // Destruction will normally be called externally and not performed via Update

    //    if (_debugTimer.CheckTimer() && !IsDestroyed && IsDestructible)
    //    {
    //        DestroyObject();
    //    }
    //}

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

    public bool IsCoverInUse(CoverObject coverObject)
    {
        List<CoverObject> coverObjects = _childrenCoverObjects.ToList();
        coverObjects.Add(this);

        return coverObjects.Contains(coverObject);
    }

    public void DestroyObject()
    {
        // For each physics child found, enable collider and physics
        if (IsDestroyed)
            return;

        IsDestroyed = true;
        _mainCollider.enabled = false;

        if (_destructionSound)
            _audioSource.PlayOneShot(_destructionSound);

        foreach (Rigidbody rigidbody in _rigidbodies)
        {
            rigidbody.GetComponent<Collider>().enabled = true;
            rigidbody.isKinematic = false;
            Destroy(rigidbody.gameObject, 6);
        }

        // Remove this object as a valid cover piece for tiles
        foreach (Tile tile in _coveredTiles)
            tile.Cover = null;

        foreach (CoverObject cover in _childrenCoverObjects)
        {
            if (cover == this) continue;
            cover.DestroyObject();
        }

        foreach (Debris debris in GetComponentsInChildren<Debris>())
        {
            debris.enabled = true;
        }
    }
}

public enum CoverSizes { half, full }
