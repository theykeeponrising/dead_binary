using System.Collections.Generic;
using UnityEngine;

public class VendingMachine : MonoBehaviour
{
    private Renderer[] _renderers;
    private Light[] _lights;
    private readonly List<Material> _materials = new();
    private readonly Timer _flickerTimer = new(1f);

    [SerializeField] private State _state;
    private bool _flickerOn = false;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _lights = GetComponentsInChildren<Light>();

        foreach (Renderer renderer in _renderers)
            _materials.Add(renderer.material);
    }

    private void Start()
    {
        CheckState();
    }

    private void Update()
    {
        CheckFlickerTimer();
        SetFlickerLights();
        SetFlickerMaterials();
    }

    private void CheckState()
    {
        // Used to ensure script does not update if machine is set to a consistent power state
        // POWERED and OFF states will force lights on and then disable this script

        if (_state == State.POWERED)
        {
            SetFlickerLights(poweredOn: true);
            SetFlickerMaterials(poweredOn: true);
            this.enabled = false;
        }

        else if (_state == State.OFF)
        {
            SetFlickerLights(poweredOn: false);
            SetFlickerMaterials(poweredOn: false);
            this.enabled = false;
        }
    }

    private void CheckFlickerTimer()
    {
        // If timer has expired, toggle bool and restart timer

        if (_flickerTimer.CheckTimer())
        {
            _flickerOn = !_flickerOn;

            System.Random rand = new();
            float flickerTime = (float)rand.NextDouble();
            _flickerTimer.SetTimer(flickerTime);
        }
    }

    private void SetFlickerLights()
    {
        // Set light components enabled/disabled based on flicker bool

        foreach (Light light in _lights)
            light.enabled = _flickerOn;
    }

    private void SetFlickerLights(bool poweredOn)
    {
        // Set light components enabled/disabled based on provided bool override

        foreach (Light light in _lights)
            light.enabled = poweredOn;
    }


    private void SetFlickerMaterials()
    {
        // Changes emissive material based on flicker bool

        float colorValue = _flickerOn ? 6f : 0f;

        foreach (Material material in _materials)
        {
            Color color = new(colorValue, colorValue, colorValue);
            material.SetColor("_Emissive", color);
        }
    }

    private void SetFlickerMaterials(bool poweredOn)
    {
        // Changes emissive material based on provided bool override

        float colorValue = poweredOn ? 6f : 0f;

        foreach (Material material in _materials)
        {
            Color color = new(colorValue, colorValue, colorValue);
            material.SetColor("_Emissive", color);
        }
    }

    private enum State { POWERED, FLICKERING, OFF }
}
