using UnityEngine;
using UnityEngine.EventSystems;

public class UnitCamera : MonoBehaviour
{
    private Unit _unit;
    private Camera _unitCamera;
    private PhysicsRaycaster _raycaster;

    // Start is called before the first frame update
    void Awake()
    {
        _unit = GetComponentInParent<Unit>();
        _unitCamera = GetComponent<Camera>();
        _raycaster = GetComponent<PhysicsRaycaster>();

        name = string.Format("{0} (Camera)", _unit.Attributes.Name);

        Camera.main.gameObject.GetComponent<CameraHandler>().AddSceneCamera(_unitCamera);
    }

    private void OnEnable()
    {
        (GetComponent<AudioListener>().enabled, Camera.main.GetComponent<AudioListener>().enabled) = (true, false);
        _unitCamera.enabled = true;
        _raycaster.enabled = true;
        _unit.Healthbar.Hide();
        Map.ClearTileHighlights();
    }

    private void OnDisable()
    {
        if (Camera.main) Camera.main.GetComponent<AudioListener>().enabled = true;
        GetComponent<AudioListener>().enabled = false;
        _unitCamera.enabled = false;
        _raycaster.enabled = false;
        _unit.Healthbar.Show();
    }

    public void AdjustAngle(float angle, Vector3 targetPosition)
    {
        // Changes camera angle to look at the character's target

        Vector3 newPosition;
        Vector3 lookPosition = (_unit.GetBoneTransform(HumanBodyBones.Chest).position + targetPosition) / 2;

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
