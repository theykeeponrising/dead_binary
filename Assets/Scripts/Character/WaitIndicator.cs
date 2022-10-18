using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitIndicator : MonoBehaviour
{
    Image image;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    public void SetColor(Color32 color)
    {
        if (!image) image = GetComponent<Image>();
        image.color = color;
    }

    public void ShowSprite(bool showSprites)
    {
        image.enabled = showSprites;
    }
}
