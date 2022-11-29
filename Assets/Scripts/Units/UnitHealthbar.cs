using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitHealthbar : MonoBehaviour
{
    private const int _containerLength = 5;
    private const float _coverFillHalf = 0.5f;
    private const float _coverFillFull = 1.0f;

    private Unit _unit;
    private Canvas _canvas;
    private readonly List<Transform> _pointsContainer = new();
    private readonly List<Transform> _pointsBackground = new();
    private readonly List<Image> _healthPoints = new();

    private Image _coverIndicatorBackground;
    private Image _coverIndicatorForeground;
    private Image _waitIndicator;

    private Color32 _healthColorHealthy = new (255, 255, 255, 255);
    private Color32 _healthColorInjured = new (150, 150, 150, 255);

    private Color32 FactionColor { get { return _unit.Attributes.Faction.FactionColor; } }
    private int HealthCurrent { get { return _unit.Stats.HealthCurrent; } }
    private int HealthMax { get { return _unit.Stats.HealthMax; } }
    private bool ShowCover { get { return _unit.CurrentCover != null && !_unit.IsMoving();  } }

    public GameObject HealthPointPrefab;

    private void Awake()
    {
        _unit = GetComponentInParent<Unit>();
        _canvas = GetComponent<Canvas>();
        _healthColorHealthy = FactionColor;

        _pointsContainer.Add(transform.Find("Points"));
        _pointsBackground.Add(_pointsContainer[0].Find("Background"));

        _waitIndicator = transform.Find("WaitIndicator").GetComponent<Image>();
        _waitIndicator.color = FactionColor;
        _waitIndicator.enabled = false;

        _coverIndicatorBackground = transform.Find("CoverIndicator").GetComponent<Image>();
        _coverIndicatorForeground = _coverIndicatorBackground.GetComponentsInChildren<Image>()[1];
        _coverIndicatorForeground.color = FactionColor;
    }

    private void Start()
    {
        Show();
        CreateHealthPoints();
    }

    private void Update()
    {
        UpdateCover();

    }

    private void LateUpdate()
    {
        LookAtCamera();
    }

    public void Show()
    {
        _canvas.enabled = true;
    }

    public void Hide()
    {
        _canvas.enabled = false;
    }

    private void LookAtCamera()
    {
        // Points healthbar UI towards the active Camera

        Camera activeCamera = CameraHandler.ActiveCamera;

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

            transform.localScale = Vector3.Lerp(transform.localScale, scaleDampening * zoomHeight * Vector3.one, Time.deltaTime * zoomDampening);
        }
        else
        {
            float distance = Vector3.Distance(transform.position, activeCamera.transform.position);
            float scaleDampening = 0.2f;
            transform.localScale = distance * scaleDampening * Vector3.one;
        }
    }

    private void CreateHealthPoints()
    {
        // Create health point objects based on unit's max health

        int i = 0;
        while (i < HealthMax)
        {
            int pointsRow = (i / _containerLength);
            i++;

            // If we need more rows, create the "Points" and "Background" objects
            if (_pointsContainer.Count <= pointsRow)
            {
                Transform newContainer = new GameObject("Points").transform;
                newContainer.parent = transform;
                newContainer.SetPositionAndRotation(transform.position, transform.rotation);
                _pointsContainer.Add(newContainer);
                _pointsBackground.Add(Instantiate(_pointsBackground[0], newContainer).transform);
            }

            // Create the health point object and assign to the correct container
            Image newPoint = Instantiate(HealthPointPrefab, _pointsContainer[pointsRow]).GetComponent<Image>();
            _healthPoints.Add(newPoint);
            int index = _healthPoints.IndexOf(newPoint);

            // Set the color to the correct faction color
            newPoint.color = _healthColorHealthy;

            // Ensure we are treating the first health point of reach row the same way
            if (index >= _containerLength) index -= (_containerLength * (_pointsContainer.Count - 1));

            // Set the position based on the index and row
            newPoint.transform.localPosition = new Vector3(0.09f * index, 0.06f * pointsRow, 0f);
        }

        foreach (Transform background in _pointsBackground)
        {
            int index = _pointsBackground.IndexOf(background);
            Math.DivRem(_healthPoints.Count, _containerLength, out int count_remainder);

            // If we are on a row with less than the container length, fit the background to the specific number of points
            if (index == _pointsBackground.Count - 1 && count_remainder > 0)
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
                float width = 0.092f * _containerLength;

                // Since a single health point is perfectly centered, we start counting at 2 HP onward
                float posX = 0.045f * (_containerLength - 1);

                background.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
                background.localPosition = new Vector3(posX, 0.06f * index, 0f);
            }
        }
    }

    public void UpdateHealthPoints()
    {
        // Changes the color on the hit point sprites to reflect current HP

        int i = 0;

        while (i < HealthMax)
        {
            _healthPoints[i].color = (i < HealthCurrent) ? _healthColorHealthy : _healthColorInjured;
            i++;
        }
    }

    private void UpdateCover()
    {
        // If not behind cover, disable the frame and make the cover icons transparent

        _coverIndicatorBackground.enabled = ShowCover;
        _coverIndicatorForeground.enabled = ShowCover;

        // If behind cover, enable the frame and set the colors
        if (_unit.CurrentCover?.CoverSize == CoverSizes.full)
            _coverIndicatorForeground.fillAmount = _coverFillFull;

        else
            _coverIndicatorForeground.fillAmount = _coverFillHalf;
    }

    public void ShowWaitIndicator(bool showSprites = true)
    {
        // Toggle to show/hide the "Waiting" sprites

        _waitIndicator.enabled = showSprites;
    }
}

enum HealthState { HEALTHY, INJURED };

enum CoverState { ACTIVE, INACTIVE };
