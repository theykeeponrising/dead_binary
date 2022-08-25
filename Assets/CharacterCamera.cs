using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterCamera : MonoBehaviour
{
    Character character;
    Camera characterCamera;
    PhysicsRaycaster raycaster;

    // Start is called before the first frame update
    void Start()
    {
        character = GetComponentInParent<Character>();
        characterCamera = GetComponent<Camera>();
        raycaster = GetComponent<PhysicsRaycaster>();

    }

    // Update is called once per frame
    void Update()
    {
        EnableCamera();
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

        characterCamera.enabled = useCharacterCam;
        raycaster.enabled = useCharacterCam;
    }

    public void AdjustAngle(float angle, Vector3 targetPosition)
    {
        //Debug.Log(angle);
        Vector3 newPosition;
        Vector3 lookPosition = (character.transform.position + targetPosition) / 2;

        if (angle <= 0)
            newPosition = new Vector3(0.75f, 2f, -1f);
        else
            newPosition = new Vector3(-0.75f, 2f, -1f);

        transform.localPosition = Vector3.Lerp(transform.localPosition, newPosition, 2f);
        transform.LookAt(lookPosition);
    }
}
