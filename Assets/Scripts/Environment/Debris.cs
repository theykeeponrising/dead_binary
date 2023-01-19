using UnityEngine;

public class Debris : MonoBehaviour
{
    private AudioSource _audioSource;
    private bool _hasCollided = false;
    private bool _collisionEnabled = false;
    private Timer _waitTimer;

    [SerializeField] private AudioClip[] _collisionSound;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        CheckTimer();
    }

    private void OnEnable()
    {
        _waitTimer = new(1f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Plays collision sound only on first collision to prevent ear bleeds

        if (_hasCollided || !_collisionEnabled)
            return;

        AudioClip audioClip = _collisionSound[Random.Range(0, _collisionSound.Length)];
        _audioSource.PlayOneShot(audioClip);
        _hasCollided = true;
    }

    private void CheckTimer()
    {
        // This timer will prevent the debris from making collision sounds immediately

        if (_waitTimer == null)
            return;

        if (_waitTimer.CheckTimer())
            _collisionEnabled = true;
    }
}
