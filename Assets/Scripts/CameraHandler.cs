using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraHandler : MonoBehaviour
{
    // Used for Camera controls

    CameraInput cameraInput;
    InputAction movement;

    // horizontal motion
    [SerializeField]
    float maxSpeed = 10f; //5f
    float speed;
    [SerializeField]
    float acceleration = 10f;
    [SerializeField]
    float damping = 15f;

    // vertical motion - zooming
    [SerializeField]
    float stepSize = 2f;
    [SerializeField]
    float zoomDampening = 7.5f;
    [SerializeField]
    float minHeight = 5f; //5f
    [SerializeField]
    float maxHeight = 50f; //50f
    // [SerializeField]
    // float zoomSpeed = 2f; // TO DO -- Fix issue with Z position during zoom

    // rotation
    [SerializeField]
    float maxRotationSpeed = 0.25f; //1f

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
        cameraInput = new CameraInput();
        zoomHeight = transform.position.y;
    }

    void OnEnable()
    {
        lastPosition = transform.position;
        movement = cameraInput.Controls.Movement;
        cameraInput.Controls.RotateCamera.performed += RotateCamera;
        cameraInput.Controls.ZoomCamera.performed += ZoomCamera;
        cameraInput.Enable();
    }

    void OnDisable()
    {
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
    }


    void UpdateVelocity()
    {
        horizontalVelocity = (transform.position - lastPosition) / Time.deltaTime;
        horizontalVelocity.y = 0;
        lastPosition = transform.position;
    }

    void GetKeyboardMovement()
    {
        Vector3 inputValue = movement.ReadValue<Vector2>().x * GetCameraRight()
            + movement.ReadValue<Vector2>().y * GetCameraForward();

        inputValue = inputValue.normalized;

        if (inputValue.sqrMagnitude > 0.1f)
            targetPosition += inputValue;
    }

    Vector3 GetCameraRight()
    {
        Vector3 right = transform.right;
        right.y = 0;
        return right;
    }

    Vector3 GetCameraForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        return forward;
    }

    void UpdateBasePosition()
    {
        if (targetPosition.sqrMagnitude > 0.1f)
        {
            speed = Mathf.Lerp(speed, maxSpeed, Time.deltaTime * acceleration);
            transform.position += targetPosition * speed * Time.deltaTime;
        }
        else
        {
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, Time.deltaTime * damping);
            transform.position += horizontalVelocity * Time.deltaTime;
        }

        targetPosition = Vector3.zero;
    }

    void RotateCamera(InputAction.CallbackContext inputValue)
    {
        if (!Mouse.current.middleButton.isPressed)
            return;

        float value = inputValue.ReadValue<Vector2>().x;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, value * maxRotationSpeed + transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }

    void ZoomCamera(InputAction.CallbackContext inputValue)
    {
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
        Vector3 zoomTarget = new Vector3(transform.localPosition.x, zoomHeight, transform.localPosition.z);
        //zoomTarget -= zoomSpeed * (zoomHeight - transform.localPosition.y) * Vector3.forward; // TO DO - Figure out why this is making Z go nuts.

        transform.localPosition = Vector3.Lerp(transform.localPosition, zoomTarget, Time.deltaTime * zoomDampening);
    }

}
