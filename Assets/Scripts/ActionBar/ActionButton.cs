using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Class used to handle sprites for action buttons
    // Will be used to control mouse-over effects as well

    Sprite icon_active;
    Sprite icon_inactive;
    Sprite background_active;
    Sprite background_inactive;
    string spritePath;

    public Button button;

    void OnEnable()
    {
        button = GetComponent<Button>();
        // button.onClick.AddListener(ButtonClicked);
    }

    public void LoadResources(string newSpritePath)
    {
        spritePath = newSpritePath;
        icon_active = Resources.Load<Sprite>(spritePath);
        icon_inactive = Resources.Load<Sprite>(spritePath + "_1");
        background_inactive = Resources.Load<Sprite>(ActionButtons.btn_background);
        background_active = Resources.Load<Sprite>(ActionButtons.btn_background + "_1");
        GetComponentsInChildren<Image>()[1].sprite = icon_active;
    }

    public string GetSpritePath()
    {
        return spritePath;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        // Highlights icon on mouse over
        GetComponentsInChildren<Image>()[0].sprite = background_inactive;
        GetComponentsInChildren<Image>()[1].sprite = icon_inactive;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        // Clears unit highlight on mouse leave
        GetComponentsInChildren<Image>()[0].sprite = background_active;
        GetComponentsInChildren<Image>()[1].sprite = icon_active;
    }
}

public static class ActionButtons
{
    // Loads action button sprites from resources

    public static string btn_background = "Buttons/btn_background";
    public static string btn_action_move = "Buttons/btn_move";
    public static string btn_action_shoot = "Buttons/btn_shoot";
    public static string btn_action_reload = "Buttons/btn_reload";
    public static string btn_action_swap = "Buttons/btn_swap";
    public static string btn_action_useItem = "Buttons/btn_useItem";
}
