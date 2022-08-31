using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanelScript : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject[] slots;


    private void Start()
    {
        panel.SetActive(false);
    }
    //Sets the buttons to reflect the character's inventory.
    public void SetPanel(bool b, Item[] items = null)
    {
        // Activate panel?
        if (b == true)
        {
            panel.SetActive(true);

            //For each slot (4), if there is an associated item, activate a button.
            for (int i = 0; i < slots.Length; i++)
            {
                var v = slots[i].transform.GetChild(0);
                if (i < items.Length)
                {
                    v.gameObject.SetActive(true);

                    //Set the icon to the associated item sprite.
                    if (items[i])
                        v.GetChild(0).GetComponent<Image>().sprite = items[i].statics.icon;
                }

                //If i surpasses the amount of items, deactivate button.
                else
                    v.gameObject.SetActive(false);
            }
        }
        else
        {
            panel.SetActive(false);
        }
    }
}
