using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    Unit unit;
    Faction faction;
    Transform pointsContainer;
    Transform pointsBackground;
    int healthMax => unit.stats.healthMax;
    int healthCurrent => unit.stats.healthCurrent;
    List<GameObject> healthPoints = new List<GameObject>();
    public GameObject healthPointPrefab;

    enum HealthState { HEALTHY, INJURED };
    //ButtonState currentButtonState = ButtonState.PASSIVE;

    // Colors for the background
    Dictionary<Faction, Dictionary<HealthState, Color32>> HealthPointColors = new Dictionary<Faction, Dictionary<HealthState, Color32>>() {
        { Faction.Good, new Dictionary<HealthState, Color32>() {
        { HealthState.HEALTHY, new Color32(37, 232, 232, 255) },
        { HealthState.INJURED, new Color32(150, 150, 150, 255) },
        }},
        { Faction.Bad, new Dictionary<HealthState, Color32>() {
        { HealthState.HEALTHY, new Color32(232, 37, 37, 255) },
        { HealthState.INJURED, new Color32(150, 150, 150, 255) },
        }},
    };


    void Start()
    {
        unit = GetComponentInParent<Unit>();
        pointsContainer = transform.Find("Points");
        pointsBackground = pointsContainer.Find("Background");

        Show();
        CreateHealthPoints();
    }

    // Update is called once per frame
    void Update()
    {
        LookAtCamera();
    }

    public void Show()
    {
        GetComponent<Canvas>().enabled = true;
    }

    public void Hide()
    {
        GetComponent<Canvas>().enabled = false;
    }

    void LookAtCamera()
    {
        // Points healthbar UI towards the active Camera

        Camera activeCamera = Camera.main.GetComponent<CameraHandler>().GetActiveCamera();

        // Main camera uses the Player game object as a proxy for rotation
        if (activeCamera == Camera.main)
        {
            Transform player = Camera.main.GetComponentInParent<Player>().transform;
            transform.rotation = Quaternion.Euler(45f, player.rotation.eulerAngles.y - 45f, 0f);
        }
        else
        {
            transform.LookAt(activeCamera.transform);
        }
    }

    void CreateHealthPoints()
    {
        // Create health point objects based on unit's max health

        int i = 0;
        while (i < healthMax)
        {
            i++;
            GameObject newPoint = Instantiate(healthPointPrefab, pointsContainer);
            healthPoints.Add(newPoint);
            int index = healthPoints.IndexOf(newPoint);
            newPoint.GetComponent<Image>().color = HealthPointColors[unit.attributes.faction][HealthState.HEALTHY];
            if (index > 0) newPoint.transform.localPosition = new Vector3(0.09f * index, 0f, 0f);
        }

        // Resize background to fit number of points
        float height = 0.075f;
        float width = 0.092f * healthPoints.Count;

        // Since a single health point is perfectly centered, we start counting at 2 HP onward
        float posX = 0.045f * (healthPoints.Count - 1);

        pointsBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        pointsBackground.localPosition = new Vector3(posX, 0f, 0f);
    }

    public void UpdateHealthPoints()
    {
        // Changes the color on the hit point sprites to reflect current HP

        int i = 0;

        while (i < healthMax)
        {
            if (i < healthCurrent)
                healthPoints[i].GetComponent<Image>().color = HealthPointColors[unit.attributes.faction][HealthState.HEALTHY];
            else
                healthPoints[i].GetComponent<Image>().color = HealthPointColors[unit.attributes.faction][HealthState.INJURED];
            i++;
        }
    }
}
