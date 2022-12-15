using UnityEngine;

public class BarrelLiquid : MonoBehaviour
{
    private Material _material;
    private readonly float _glowMin = 0.3f;
    private readonly float _glowMax = 0.7f;
    private bool _glowFade = false;

    private Vector3 _rotateAxis = new(0f, 1f, 0f);
    [SerializeField][Range(0f, 2f)] private float _rotateSpeed = 0.75f;

    private void Awake()
    {
        _material = GetComponent<Renderer>().material;
    }

    private void Update()
    {
        RotateLiquid();
        ChangeGlow();
    }

    private void RotateLiquid()
    {
        transform.Rotate(_rotateAxis * _rotateSpeed / 10);
    }

    private void ChangeGlow()
    {
        float targetGlow = (_glowFade) ? _glowMin : _glowMax;
        Color materialColor = _material.color;

        materialColor.a = Mathf.Lerp(materialColor.a, targetGlow, Time.deltaTime);
        _material.color = materialColor;
        if (SwitchGlow()) _glowFade = !_glowFade;
    }

    private bool SwitchGlow()
    {
        if (_glowFade && _material.color.a <= _glowMin + 0.02f)
            return true;

        if (!_glowFade && _material.color.a >= _glowMax - 0.02f)
            return true;

        return false;
    }
}
