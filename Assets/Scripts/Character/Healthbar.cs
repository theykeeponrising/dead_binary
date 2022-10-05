using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    Unit unit;

    ///////////////////
    // Health points //
    ///////////////////

    List<Transform> pointsContainer = new List<Transform>();
    List<Transform> pointsBackground = new List<Transform>();
    int containerLength = 5;

    int healthMax => unit.stats.healthMax;
    int healthCurrent => unit.stats.healthCurrent;
    List<GameObject> healthPoints = new List<GameObject>();
    public GameObject healthPointPrefab;
    enum HealthState { HEALTHY, INJURED };

    // Colors for the health bar sprites
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

    //////////////////////////
    // Do Nothing Indicator //
    //////////////////////////

    public Image waitingIndicator;

    /////////////////////
    // Cover indicator //
    /////////////////////

    Transform coverIndicator;
    Image coverHalf;
    Image coverFull;
    enum CoverState { ACTIVE, INACTIVE };

    // Colors for the cover indicator sprites
    Dictionary<Faction, Dictionary<CoverState, Color32>> CoverIndicatorColors = new Dictionary<Faction, Dictionary<CoverState, Color32>>() {
        { Faction.Good, new Dictionary<CoverState, Color32>() {
        { CoverState.ACTIVE, new Color32(37, 232, 232, 255) },
        { CoverState.INACTIVE, new Color32(150, 150, 150, 0) },
        }},
        { Faction.Bad, new Dictionary<CoverState, Color32>() {
        { CoverState.ACTIVE, new Color32(232, 37, 37, 255) },
        { CoverState.INACTIVE, new Color32(150, 150, 150, 0) },
        }},
    };

    void Awake()
    {
        unit = GetComponentInParent<Unit>();
        pointsContainer.Add(transform.Find("Points"));
        pointsBackground.Add(pointsContainer[0].Find("Background"));

        waitingIndicator = transform.Find("WaitIndicator").GetComponent<Image>();
        waitingIndicator.color = CoverIndicatorColors[unit.attributes.faction][CoverState.ACTIVE];
        waitingIndicator.enabled = false;

        coverIndicator = transform.Find("CoverIndicator");
        coverFull = coverIndicator.GetComponentsInChildren<Image>()[1];
        coverHalf = coverIndicator.GetComponentsInChildren<Image>()[2];
    }

    private void Start()
    {
        Show();
        CreateHealthPoints();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCover();
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

        // Scales the healthbar UI so that it is visible regardless of camera zoom
        if (activeCamera == Camera.main)
        {
            float zoomHeight = Camera.main.GetComponent<CameraHandler>().GetCameraZoomHeight();
            float zoomDampening = Camera.main.GetComponent<CameraHandler>().GetCameraZoomDampening();
            float scaleDampening = 0.3f;

            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * zoomHeight * scaleDampening, Time.deltaTime * zoomDampening);
        }
        else
        {
            float distance = Vector3.Distance(transform.position, activeCamera.transform.position);
            float scaleDampening = 0.2f;
            transform.localScale = Vector3.one * distance * scaleDampening;
        }
    }

    void CreateHealthPoints()
    {
        // Create health point objects based on unit's max health

        int i = 0;
        while (i < healthMax)
        {
            int pointsRow = (i / containerLength);
            i++;

            // If we need more rows, create the "Points" and "Background" objects
            if (pointsContainer.Count <= pointsRow)
            {
                Transform newContainer = new GameObject("Points").transform;
                newContainer.parent = transform;
                newContainer.rotation = transform.rotation;
                newContainer.position = transform.position;
                pointsContainer.Add(newContainer);
                pointsBackground.Add(Instantiate(pointsBackground[0], newContainer).transform);
            }

            // Create the health point object and assign to the correct container
            GameObject newPoint = Instantiate(healthPointPrefab, pointsContainer[pointsRow]);
            healthPoints.Add(newPoint);
            int index = healthPoints.IndexOf(newPoint);

            // Set the color to the correct faction color
            newPoint.GetComponent<Image>().color = HealthPointColors[unit.attributes.faction][HealthState.HEALTHY];

            // Ensure we are treating the first health point of reach row the same way
            if (index >= containerLength) index = index - (containerLength * (pointsContainer.Count - 1));

            // Set the position based on the index and row
            newPoint.transform.localPosition = new Vector3(0.09f * index, 0.06f * pointsRow, 0f);
        }

        foreach (Transform background in pointsBackground)
        {
            int index = pointsBackground.IndexOf(background);
            Math.DivRem(healthPoints.Count, containerLength, out int count_remainder);

            // If we are on a row with less than the container length, fit the background to the specific number of points
            if (index == pointsBackground.Count - 1 && count_remainder > 0)
            {
                float height = 0.075f;
                float width = 0.1f * count_remainder - (0.01f * (count_remainder - 1));

                // Since a single health point is perfectly centered, we start counting at 2 HP onward
                float posX = 0.045f * (count_remainder - 1);

                background.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
                background.localPosition = new Vector3(posX, 0.06f * index, 0f);
            }

            // Otherwise fit the background to the container length
            else
            {
                float height = 0.075f;
                float width = 0.092f * containerLength;

                // Since a single health point is perfectly centered, we start counting at 2 HP onward
                float posX = 0.045f * (containerLength - 1);

                background.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
                background.localPosition = new Vector3(posX, 0.06f * index, 0f);
            }
        }
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


    void UpdateCover()
    {
        // If not behind cover, disable the frame and make the cover icons transparent
        if (!unit.currentCover)
        {
            coverIndicator.GetComponent<Image>().enabled = false;
            coverHalf.color = CoverIndicatorColors[unit.attributes.faction][CoverState.INACTIVE];
            coverFull.color = CoverIndicatorColors[unit.attributes.faction][CoverState.INACTIVE];
        }

        // If behind cover, enable the frame and set the colors
        else if (unit.currentCover.coverSize == CoverObject.CoverSize.full)
        {
            coverIndicator.GetComponent<Image>().enabled = true;
            coverHalf.color = CoverIndicatorColors[unit.attributes.faction][CoverState.INACTIVE];
            coverFull.color = CoverIndicatorColors[unit.attributes.faction][CoverState.ACTIVE];
        }

        else
        {
            coverIndicator.GetComponent<Image>().enabled = true;
            coverHalf.color = CoverIndicatorColors[unit.attributes.faction][CoverState.ACTIVE];
            coverFull.color = CoverIndicatorColors[unit.attributes.faction][CoverState.INACTIVE];
        }
    }

    public void WaitingIndicator(bool showSprites = true)
    {
        // Toggle to show/hide the "Waiting" sprites

        waitingIndicator.enabled = showSprites;
    }
}
