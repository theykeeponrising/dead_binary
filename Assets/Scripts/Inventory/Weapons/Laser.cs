using UnityEngine;

public class Laser : Projectile
{
    private Material _material;
    [SerializeField] private float _fadeTime = 1f;

    protected override void Awake()
    {
        base.Awake();

        _material = GetComponent<LineRenderer>().material;
    }
    protected override void UpdateBulletLine()
    {
        if (!BulletLine)
            return;

        Color materialColor = _material.color;
        materialColor.a = Mathf.Lerp(materialColor.a, 0f, _fadeTime * Time.deltaTime);
        _material.color = materialColor;

        if (Triggered)
            return;

        BulletLine.SetPosition(0, CurrentPosition);
        BulletLine.SetPosition(1, Destination);
    }
}
