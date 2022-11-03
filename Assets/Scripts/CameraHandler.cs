using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class CameraHandler : MonoBehaviour
{
    // Used for Camera controls
    private readonly List<Camera> _sceneCameras = new();
    private CameraInput _cameraInput;
    private PhysicsRaycaster _raycaster;
    private AudioListener _audioListener;
    private Player _player;

    // Horizontal Motion
    [SerializeField] [Range(5f, 50f)] private float _panSpeedMax = 10f;
    [SerializeField] private float _panAcceleration = 10f;
    private float _panSpeed;
    private Vector3 _panNextPosition;
    private Vector3 _panLastPosition;
    private Vector3 _panSnapPosition;
    private Vector3 _panVelocity;

    // Vertical Motion
    [SerializeField] [Range(0.5f, 3f)] private float _zoomStepSize = 2f;
    [SerializeField] [Range(0.5f, 2f)] private float _zoomZoomSpeed = 2f;
    [SerializeField] private float _zoomDampening = 7.5f;
    [SerializeField] private float _zoomHeightMin = 5f;
    [SerializeField] private float _zoomHeightMax = 50f;
    private float _zoomHeight;

    // Rotation
    [SerializeField] [Range(0.1f, 1f)] private float _rotationSpeedMax = 0.25f;

    // screen edge motion -- NOT IMPLEMENTED
    // [SerializeField] [Range(0f, 0.1f)] float edgeTolerance = 0.05f; // TO DO -- For use with camera boundaries
    // [SerializeField] bool useScreenEdge = true; // TO DO -- For use with camera boundaries

    public static Camera ActiveCamera { get { return Camera.main.GetComponent<CameraHandler>().GetActiveCamera(); } }
    public Tuple<float, float> Zoom { get { return new Tuple<float, float>(_zoomHeight, _zoomDampening); } }
    private Vector3 PlayerPosition { get { return _player.transform.position; } set { _player.transform.position = value; } }
    private Quaternion PlayerRotation { get { return _player.transform.rotation; } set { _player.transform.rotation = value; } }
    private Vector3 LocalPosition { get { return transform.localPosition; } set { transform.localPosition = value; } }
    private bool InputPan { get { return _cameraInput.Controls.InputPan.IsPressed(); } }
    private Vector2 InputMovement { get { return _cameraInput.Controls.Movement.ReadValue<Vector2>(); } }
    private Vector2 InputRotation { get { return _cameraInput.Controls.Rotation.ReadValue<Vector2>(); } }
    private Vector2 MousePosition { get { return _cameraInput.Controls.Position.ReadValue<Vector2>(); } }

    private void Awake()
    {
        _cameraInput = new CameraInput();
        _raycaster = GetComponent<PhysicsRaycaster>();
        _audioListener = GetComponent<AudioListener>();
        _zoomHeight = transform.position.y;
        _player = GetComponentInParent<Player>();
    }

    private void OnEnable()
    {
        // Add bindings when enabled and begin tracking position

        _panLastPosition = transform.position;
        _cameraInput.Controls.ZoomCamera.performed += ZoomCamera;
        _cameraInput.Enable();
    }

    private void OnDisable()
    {
        // Remove bindings when disabled

        _cameraInput.Controls.ZoomCamera.performed -= ZoomCamera;
        _cameraInput.Disable();
    }

    private void Update()
    {
        UpdateKeyboardMovement();
        UpdateVelocity();
        UpdateCameraPosition();
        UpdateBasePosition();
        UpdateCameraRotation();
        UpdateCameraSnap();
        CheckActiveCamera();
    }

    public void AddSceneCamera(Camera addCamera)
    {
        // Adds camera from the current scene to be tracked

        if (!_sceneCameras.Contains(addCamera))
            _sceneCameras.Add(addCamera);
    }

    private void CheckActiveCamera()
    {
        // Ensures physics _raycaster is only active if no other camera is in use

        // If there are any scene cameras currently active, disable _raycaster on this camera
        if (_sceneCameras.Any() && _sceneCameras.Any(checkCamera => checkCamera.enabled))
        {
            _raycaster.enabled = false;
            _audioListener.enabled = false;
            return;
        }

        // If this is the only camera active, ensure _raycaster and audio listener are turned back on
        if (!_raycaster.enabled || !_audioListener.enabled)
        {
            _raycaster.enabled = true;
            _audioListener.enabled = true;
        }
    }

    private Camera GetActiveCamera()
    {
        // Returns currently active camera

        foreach (Camera camera in _sceneCameras)
            if (camera.enabled)
                return camera;

        return Camera.main;
    }

    private void UpdateVelocity()
    {
        // Tracks velocity of camera _inputMovement

        _panVelocity = (PlayerPosition - _panLastPosition) / Time.deltaTime;
        _panVelocity.y = 0;
        _panLastPosition = PlayerPosition;
    }

    private void UpdateKeyboardMovement()
    {
        // Translates keyboard input or mouse position into camera position values

        Vector2 readValue = InputPan ? Camera.main.ScreenToViewportPoint(MousePosition) - new Vector3(0.5f, 0.5f, 0f) : InputMovement;
        Vector3 inputValue = (readValue.x * GetCameraRight() + readValue.y * GetCameraForward()).normalized;

        if (inputValue.sqrMagnitude > 0.1f)
        {
            _panNextPosition += inputValue;
            _panSnapPosition = Vector3.zero;
        }
    }

    private Vector3 GetCameraRight()
    {
        // Returns camera right with a flattened y axis

        Vector3 right = transform.right;
        right.y = 0;
        return right;
    }

    private Vector3 GetCameraForward()
    {
        // Returns camera forward with a flattened y axis

        Vector3 forward = transform.forward;
        forward.y = 0;
        return forward;
    }

    private void UpdateBasePosition()
    {
        // Moves camera parent (Player) to new position

        if (_panNextPosition.sqrMagnitude > 0.1f)
        {
            _panSpeed = Mathf.Lerp(_panSpeed, _panSpeedMax, Time.deltaTime * _panAcceleration);
            PlayerPosition += _panSpeed * Time.deltaTime * _panNextPosition;
        }

        _panNextPosition = Vector3.zero;
    }

    private void UpdateCameraRotation()
    {
        // Rotate camera parent (Player) while holding middle mouse button or pressing Q/E

        float value = InputRotation.x;
        PlayerRotation = Quaternion.Euler(PlayerRotation.eulerAngles.x, value * _rotationSpeedMax + PlayerRotation.eulerAngles.y, PlayerRotation.eulerAngles.z);
    }

    private void ZoomCamera(InputAction.CallbackContext inputValue)
    {
        // Sets the expected zoom height based on input

        float value = -inputValue.ReadValue<Vector2>().y / 100f; // divided for tuning

        if (Mathf.Abs(value) > 0.1f)
        {
            _zoomHeight = LocalPosition.y + value * _zoomStepSize;
            if (_zoomHeight < _zoomHeightMin)
                _zoomHeight = _zoomHeightMin;
            else if (_zoomHeight > _zoomHeightMax)
                _zoomHeight = _zoomHeightMax;
        }
    }

    private void UpdateCameraPosition()
    {
        // Pulls camera backwards, giving a "zooming out" feel

        Vector3 zoomTarget = new(LocalPosition.x, _zoomHeight, LocalPosition.z);
        Vector3 moveVector = new(-0.5f, 0f, 0.5f); // Keep x and z inverted to center camera
        zoomTarget -= _zoomZoomSpeed * (_zoomHeight - LocalPosition.y) * moveVector;

        LocalPosition = Vector3.Lerp(LocalPosition, zoomTarget, Time.deltaTime * _zoomDampening);
    }

    public void SetCameraSnap(Unit unit)
    {
        if (!unit)
            return;

        _panSnapPosition = unit.transform.position;
    }

    public void SetCameraSnap(Vector3 position)
    {
        if (position == Vector3.zero)
            return;

        _panSnapPosition = position;
    }

    private void UpdateCameraSnap()
    {
        // If no snap target is set, exit
        if (_panSnapPosition == Vector3.zero)
            return;

        // Set new target position and pan speed
        Vector3 newPosition = new(_panSnapPosition.x, PlayerPosition.y, _panSnapPosition.z);
        _panSpeed = Mathf.Lerp(_panSpeed, _panSpeedMax, Time.deltaTime * _panAcceleration);

        // Move the camera focus target
        PlayerPosition = Vector3.Lerp(PlayerPosition, newPosition, 0.01f * _panSpeed);

        // Once position is reached, clear snap target
        if (Vector3.Distance(PlayerPosition, newPosition) <= 0.25f) 
            _panSnapPosition = Vector3.zero;
    }
}
