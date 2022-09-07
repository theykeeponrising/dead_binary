using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterCamera : MonoBehaviour
{
    Unit unit;
    Camera characterCamera;
    PhysicsRaycaster raycaster;

    // Start is called before the first frame update
    void Awake()
    {
        unit = GetComponentInParent<Unit>();
        characterCamera = GetComponent<Camera>();
        raycaster = GetComponent<PhysicsRaycaster>();

        name = string.Format("{0} (Camera)", unit.attributes.name);

        Camera.main.gameObject.GetComponent<CameraHandler>().AddSceneCamera(characterCamera);
    }

    private void OnEnable()
    {
        (GetComponent<AudioListener>().enabled, Camera.main.GetComponent<AudioListener>().enabled) = (true, false);
        characterCamera.enabled = true;
        raycaster.enabled = true;
    }

    private void OnDisable()
    {
        (GetComponent<AudioListener>().enabled, Camera.main.GetComponent<AudioListener>().enabled) = (false, true);
        characterCamera.enabled = false;
        raycaster.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //EnableCamera();
    }

    public void AdjustAngle(float angle, Vector3 targetPosition)
    {
        // Changes camera angle to look at the character's target

        Vector3 newPosition;
        Vector3 lookPosition = (unit.GetAnimator().GetCharacterChestPosition() + targetPosition) / 2;

        // Move camera to the appropriate side of the character depending on angle
        if (angle <= 0)
            newPosition = new Vector3(0.75f, 2f, -1f);
        else
            newPosition = new Vector3(-0.75f, 2f, -1f);

        // Point camera so that both character and target position are visible in same shot
        transform.localPosition = Vector3.Lerp(transform.localPosition, newPosition, 2f);
        transform.LookAt(lookPosition);
    }
}
