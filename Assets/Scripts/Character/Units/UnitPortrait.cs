using UnityEngine;
using UnityEngine.UI;

public class UnitPortrait : MonoBehaviour
{
    private Image _portrait;
    private Image _outline;

    public Image[] GetSprites()
    {
        _portrait = transform.Find("Portrait").GetComponent<Image>();
        _outline = transform.Find("Outline").GetComponent<Image>();
        return new Image[]{ _portrait, _outline };
    }
}
