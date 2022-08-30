using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterCamera : MonoBehaviour
{
    Character character;
    Camera characterCamera;
    PhysicsRaycaster raycaster;
    AudioListener audioListener;

    // Start is called before the first frame update
    void Awake()
    {
        character = GetComponentInParent<Character>();
        characterCamera = GetComponent<Camera>();
        raycaster = GetComponent<PhysicsRaycaster>();
        audioListener = GetComponent<AudioListener>();

        name = string.Format("{0} (Camera)", character.attributes.name);

        Camera.main.gameObject.GetComponent<CameraHandler>().AddSceneCamera(characterCamera);
    }

    private void OnEnable()
    {
        (audioListener.enabled, Camera.main.GetComponent<AudioListener>().enabled) = (true, false);
        characterCamera.enabled = true;
        raycaster.enabled = true;
    }

    private void OnDisable()
    {
        (audioListener.enabled, Camera.main.GetComponent<AudioListener>().enabled) = (false, true);
        characterCamera.enabled = false;
        raycaster.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //EnableCamera();
    }

    void EnableCamera()
    {
        // If this character isn't selected, disalbe the character camera
        if (character.playerAction.selectedCharacter != character)
        {
            characterCamera.enabled = false;
            return;
        }

        // State machine is "Choosing Target"
        bool targeting = (character.playerAction.stateMachine.GetCurrentState().GetType() == typeof(SelectedStates.ChoosingShootTarget));

        // State machine is "Shooting Target"
        bool shooting = (character.playerAction.stateMachine.GetCurrentState().GetType() == typeof(SelectedStates.ShootTarget));

        // There is a target character
        bool target = character.targetCharacter != null;

        // If we are in "choosing" or "shooting" target states, and there is a valid target, we use the character camera
        bool useCharacterCam = ((targeting || shooting) && target) ? true : false;

        if (useCharacterCam) 
        characterCamera.enabled = useCharacterCam;
        raycaster.enabled = useCharacterCam;
        audioListener.enabled = useCharacterCam;
    }

    public void AdjustAngle(float angle, Vector3 targetPosition)
    {
        // Changes camera angle to look at the character's target

        Vector3 newPosition;
        Vector3 lookPosition = (character.body.chest.transform.position + targetPosition) / 2;

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
