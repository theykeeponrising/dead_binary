using System.Collections.Generic;

public class UnitActionSprites
{
    // Used as a quick way to designate sprites to actions

    private const string btn_background = "Buttons/btn_background";
    private const string btn_action_move = "Buttons/btn_move";
    private const string btn_action_shoot = "Buttons/btn_shoot";
    private const string btn_action_reload = "Buttons/btn_reload";
    private const string btn_action_swap = "Buttons/btn_swap";
    private const string btn_action_chooseItem = "Buttons/btn_chooseItem";
    private const string btn_action_useItem = "Buttons/btn_useItem";
    private const string btn_action_medkit = "Buttons/btn_medkit";
    private const string btn_action_grenade = "Buttons/btn_grenade";
    private const string btn_action_wait = "Buttons/btn_wait";
    private const string btn_action_talk = "Buttons/btn_talk";

    public static Dictionary<UnitActionEnum, string> UnitActionSpritesDict = new() {
        { UnitActionEnum.MOVE, btn_action_move },
        { UnitActionEnum.SHOOT, btn_action_shoot },
        { UnitActionEnum.RELOAD, btn_action_reload },
        { UnitActionEnum.SWAP, btn_action_swap },
        { UnitActionEnum.CHOOSE_ITEM, btn_action_chooseItem },
        { UnitActionEnum.MEDKIT, btn_action_medkit },
        { UnitActionEnum.GRENADE, btn_action_grenade },
        { UnitActionEnum.WAIT, btn_action_wait },
        { UnitActionEnum.TALK, btn_action_talk },
    };

    public static string GetSprite(UnitActionEnum match)
    {
        // Returns the sprite string

        return UnitActionSpritesDict[match];
    }
}

public enum UnitActionEnum { NONE, MOVE, SHOOT, RELOAD, SWAP, CHOOSE_ITEM, MEDKIT, GRENADE, WAIT, TALK };
