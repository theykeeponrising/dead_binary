using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class CameraHandler : MonoBehaviour
{
    // Used for Camera controls

    Camera thisCamera;
    [SerializeField] List<Camera> sceneCameras = new List<Camera>();
    CameraInput cameraInput;
    PhysicsRaycaster raycaster;
    AudioListener audioListener;
    InputAction movement;
    public Transform parent;

    // horizontal motion
    [SerializeField]
    [Range(5f, 50f)]
    float maxSpeed = 10f;
    float speed;
    [SerializeField]
    float acceleration = 10f;

    // vertical motion - zooming
    [SerializeField]
    [Range(0.5f, 3f)]
    float stepSize = 2f;
    [SerializeField]
    float zoomDampening = 7.5f;
    [SerializeField]
    float minHeight = 5f;
    [SerializeField]
    float maxHeight = 50f;
    [SerializeField]
    [Range(0.5f, 2f)]
    float zoomSpeed = 2f;

    // rotation
    [SerializeField]
    [Range(0.1f, 1f)]
    float maxRotationSpeed = 0.25f;

    // screen edge motion
    // [SerializeField]
    // [Range(0f, 0.1f)]
    // float edgeTolerance = 0.05f; // TO DO -- For use with camera boundaries
    // [SerializeField]
    // bool useScreenEdge = true; // TO DO -- For use with camera boundaries

    // value set in various functions

    Vector3 targetPosition;

    float zoomHeight;

    Vector3 horizontalVelocity;
    Vector3 lastPosition;

    // Vector3 startDrag;


    void Awake()
    {
        thisCamera = GetComponent<Camera>();
        cameraInput = new CameraInput();
        raycaster = GetComponent<PhysicsRaycaster>();
        audioListener = GetComponent<AudioListener>();
        zoomHeight = transform.position.y;
        parent = transform.parent;
    }

    void OnEnable()
    {
        // Add bindings when enabled and begin tracking position

        lastPosition = transform.position;
        movement = cameraInput.Controls.Movement;
        cameraInput.Controls.RotateCamera.performed += RotateCamera;
        cameraInput.Controls.ZoomCamera.performed += ZoomCamera;
        cameraInput.Enable();
    }

    void OnDisable()
    {
        // Remove bindings when disabled

        cameraInput.Controls.RotateCamera.performed -= RotateCamera;
        cameraInput.Controls.ZoomCamera.performed -= ZoomCamera;
        cameraInput.Disable();
    }

    void Update()
    {
        GetKeyboardMovement();
        UpdateVelocity();
        UpdateCameraPosition();
        UpdateBasePosition();
        CheckActiveCamera();
    }

    public void AddSceneCamera(Camera addCamera)
    {
        // Adds camera from the current scene to be tracked

        if (!sceneCameras.Contains(addCamera))
            sceneCameras.Add(addCamera);
    }

    void CheckActiveCamera()
    {
        // Ensures physics raycaster is only active if no other camera is in use

        // If there are any scene cameras currently active, disable raycaster on this camera
        if (sceneCameras.Any() && sceneCameras.Any(checkCamera => checkCamera.enabled))
        {
            raycaster.enabled = false;
            audioListener.enabled = false;
            return;
        }

        // If this is the only camera active, ensure raycaster and audio listener are turned back on
        if (!raycaster.enabled || !audioListener.enabled)
        {
            raycaster.enabled = true;
            audioListener.enabled = true;
        }
    }


    void UpdateVelocity()
    {
        // Tracks velocity of camera movement

        horizontalVelocity = (parent.transform.position - lastPosition) / Time.deltaTime;
        horizontalVelocity.y = 0;
        lastPosition = parent.transform.position;
    }

    void GetKeyboardMovement()
    {
        // Translates keyboard input into camera movement values

        Vector3 inputValue = movement.ReadValue<Vector2>().x * GetCameraRight()
            + movement.ReadValue<Vector2>().y * GetCameraForward();

        inputValue = inputValue.normalized;

        if (inputValue.sqrMagnitude > 0.1f)
            targetPosition += inputValue;
    }

    Vector3 GetCameraRight()
    {
        // Returns camera right with a flattened y axis

        Vector3 right = transform.right;
        right.y = 0;
        return right;
    }

    Vector3 GetCameraForward()
    {
        // Returns camera forward with a flattened y axis

        Vector3 forward = transform.forward;
        forward.y = 0;
        return forward;
    }

    void UpdateBasePosition()
    {
        // Moves camera parent (Player) to new position

        if (targetPosition.sqrMagnitude > 0.1f)
        {
            speed = Mathf.Lerp(speed, maxSpeed, Time.deltaTime * acceleration);
            parent.transform.position += targetPosition * speed * Time.deltaTime;
        }

        targetPosition = Vector3.zero;
    }

    void RotateCamera(InputAction.CallbackContext inputValue)
    {
        // Rotate camera parent (Player) while holding middle mouse button
        // TO DO -- Add keyboard rotation

        if (!Mouse.current.middleButton.isPressed)
            return;

        float value = inputValue.ReadValue<Vector2>().x;
        parent.rotation = Quaternion.Euler(parent.rotation.eulerAngles.x, value * maxRotationSpeed + parent.rotation.eulerAngles.y, parent.rotation.eulerAngles.z);
    }

    void ZoomCamera(InputAction.CallbackContext inputValue)
    {
        // Sets the expected zoom height based on input

        float value = -inputValue.ReadValue<Vector2>().y / 100f; // divided for tuning

        if (Mathf.Abs(value) > 0.1f)
        {
            zoomHeight = transform.localPosition.y + value * stepSize;
            if (zoomHeight < minHeight)
                zoomHeight = minHeight;
            else if (zoomHeight > maxHeight)
                zoomHeight = maxHeight;
        }
    }

    void UpdateCameraPosition()
    {
        // Pulls camera backwards, giving a "zooming out" feel

        Vector3 zoomTarget = new Vector3(transform.localPosition.x, zoomHeight, transform.localPosition.z);
        Vector3 moveVector = new Vector3(-0.5f, 0f, 0.5f); // Keep x and z inverted to center camera
        zoomTarget -= zoomSpeed * (zoomHeight - transform.localPosition.y) * moveVector;

        transform.localPosition = Vector3.Lerp(transform.localPosition, zoomTarget, Time.deltaTime * zoomDampening);
    }

}
