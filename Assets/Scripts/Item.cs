using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum ItemType { ATTACHMENT, EQUIPMENT, CONSUMABLE, QUEST }
    // ATTACHMENT - Not usable item, passive effect ... ex scopes, grips, implants
    // EQUIPMENT - Usable unlimited item, immediate effect ... ex jammer, taser, radio
    // CONSUMABLE - Usable limited item, immediate effect ... ex grenades, medkits, stimpacks
    // QUEST - Not usable ... ex supplies, intel, macguffin

    public string itemName;
    public ItemType itemType;
    public int itemCost; // AP Cost
    public string itemContext;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
