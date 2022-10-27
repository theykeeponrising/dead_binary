using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    Unit _unit;
    Color32 _factionColor => _unit.attributes.faction.FactionColor;

    ///////////////////
    // Health points //
    ///////////////////

    List<Transform> pointsContainer = new List<Transform>();
    List<Transform> pointsBackground = new List<Transform>();
    int containerLength = 5;

    int healthMax => _unit.stats.healthMax;
    int healthCurrent => _unit.stats.healthCurrent;
    List<Image> healthPoints = new List<Image>();
    public GameObject HealthPointPrefab;
    enum HealthState { HEALTHY, INJURED };
    Dictionary<HealthState, Color32> HealthPointColors;

    //////////////////////////
    // Wait indicator //
    //////////////////////////

    WaitIndicator waitIndicator;

    /////////////////////
    // Cover indicator //
    /////////////////////

    Image coverIndicator;
    Image coverHalf;
    Image coverFull;
    enum CoverState { ACTIVE, INACTIVE };
    Dictionary<CoverState, Color32> CoverIndicatorColors;

    void Awake()
    {
        _unit = GetComponentInParent<Unit>();

        HealthPointColors = new Dictionary<HealthState, Color32>()
        {
            { HealthState.HEALTHY, _factionColor },
            { HealthState.INJURED, new Color32(150, 150, 150, 255) },
        };
        CoverIndicatorColors = new Dictionary<CoverState, Color32>()
        {
            { CoverState.ACTIVE, _factionColor },
            { CoverState.INACTIVE, new Color32(150, 150, 150, 0) },
        };

        pointsContainer.Add(transform.Find("Points"));
        pointsBackground.Add(pointsContainer[0].Find("Background"));

        waitIndicator = GetComponentInChildren<WaitIndicator>(); ;
        waitIndicator.SetColor(CoverIndicatorColors[CoverState.ACTIVE]);
        waitIndicator.ShowSprite(false);

        coverIndicator = transform.Find("CoverIndicator").GetComponent<Image>();
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
            (float zoomHeight, float zoomDampening) = Camera.main.GetComponent<CameraHandler>().Zoom;
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
            Image newPoint = Instantiate(HealthPointPrefab, pointsContainer[pointsRow]).GetComponent<Image>();
            healthPoints.Add(newPoint);
            int index = healthPoints.IndexOf(newPoint);

            // Set the color to the correct faction color
            newPoint.color = HealthPointColors[HealthState.HEALTHY];

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
                healthPoints[i].color = HealthPointColors[HealthState.HEALTHY];
            else
                healthPoints[i].color = HealthPointColors[HealthState.INJURED];
            i++;
        }
    }


    void UpdateCover()
    {
        // If not behind cover, disable the frame and make the cover icons transparent
        if (!_unit.currentCover)
        {
            coverIndicator.enabled = false;
            coverHalf.color = CoverIndicatorColors[CoverState.INACTIVE];
            coverFull.color = CoverIndicatorColors[CoverState.INACTIVE];
        }

        // If behind cover, enable the frame and set the colors
        else if (_unit.currentCover.CoverSize == CoverSizes.full)
        {
            coverIndicator.enabled = true;
            coverHalf.color = CoverIndicatorColors[CoverState.INACTIVE];
            coverFull.color = CoverIndicatorColors[CoverState.ACTIVE];
        }

        else
        {
            coverIndicator.enabled = true;
            coverHalf.color = CoverIndicatorColors[CoverState.ACTIVE];
            coverFull.color = CoverIndicatorColors[CoverState.INACTIVE];
        }
    }

    public void WaitingIndicator(bool showSprites = true)
    {
        // Toggle to show/hide the "Waiting" sprites

        waitIndicator?.ShowSprite(showSprites);
    }
}
