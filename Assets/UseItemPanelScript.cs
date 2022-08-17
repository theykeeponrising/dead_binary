using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UseItemPanelScript : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI nameText;

    public void SetPanel(bool b, Item item = null)
    {
        if (b == true)
        {
            panel.SetActive(true);
            image.sprite = item.Icon;
            nameText.text = item.Name;
        }
        else
        {
            panel.SetActive(false);
            image.sprite = null;
            nameText.text = "No Item";
        }
    }
}
