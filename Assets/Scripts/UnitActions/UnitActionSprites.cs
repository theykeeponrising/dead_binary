using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitActionSprites
{
    // Used as a quick way to designate sprites to actions

    public static string btn_background = "Buttons/btn_background";
    public static string btn_action_move = "Buttons/btn_move";
    public static string btn_action_shoot = "Buttons/btn_shoot";
    public static string btn_action_reload = "Buttons/btn_reload";
    public static string btn_action_swap = "Buttons/btn_swap";
    public static string btn_action_chooseItem = "Buttons/btn_chooseItem";
    public static string btn_action_useItem = "Buttons/btn_useItem";
    public static string btn_action_medkit = "Buttons/btn_medkit";
    public static string btn_action_grenade = "Buttons/btn_grenade";

    public static Dictionary<UnitActionEnum, string> UnitActionSpritesDict = new Dictionary<UnitActionEnum, string>() {
        { UnitActionEnum.MOVE, btn_action_move },
        { UnitActionEnum.SHOOT, btn_action_shoot },
        { UnitActionEnum.RELOAD, btn_action_reload },
        { UnitActionEnum.SWAP, btn_action_swap },
        { UnitActionEnum.CHOOSE_ITEM, btn_action_chooseItem },
        { UnitActionEnum.MEDKIT, btn_action_medkit },
        { UnitActionEnum.GRENADE, btn_action_grenade },
    };

    public static string GetSprite(UnitActionEnum match)
    {
        // Returns the sprite string

        return UnitActionSpritesDict[match];
    }
}

public enum UnitActionEnum { NONE, MOVE, SHOOT, RELOAD, SWAP, CHOOSE_ITEM, MEDKIT, GRENADE };
