using System.Collections.Generic;
using UnityEngine;


public class Action
{
    // Container for various actions
    // aname - The action name displayed
    // context - Used internally to provide context to action, such as action variations
    // cost - Action point cost to use action
    // cooldown - Number of turns until action is useable again
    // buttonPath - The sprites used with the action button (if applicable)
    // requirements - Requirements to perform action

    // FUTURE ATTR
    // Type - Action type (ability, item, etc)

    public string aname;
    public ActionList context;
    public string description;
    public int cost;
    public int cooldown;
    public string buttonPath;
    public ActionRequirement[] requirements;

    //TODO: Create classes for each action, and add a delegate as a callback OnActionComplete()

    public Action(string aName, ActionList aContext, string aDescription, int aCost, int aCooldown, string aButtonPath, ActionRequirement[] aRequirements)
    {
        aname = aName;
        context = aContext;
        description = aDescription;
        cost = aCost;
        cooldown = aCooldown;
        buttonPath = aButtonPath;
        requirements = aRequirements;
    }

    public Action(string aName, ActionList aContext, string aDescription, int aCost, int aCooldown, string aButtonPath, ActionRequirement aRequirements)
    {
        aname = aName;
        context = aContext;
        description = aDescription;
        cost = aCost;
        cooldown = aCooldown;
        buttonPath = aButtonPath;
        requirements = new ActionRequirement[] { aRequirements };
    }

    public static bool CheckRequirements(ActionList actionList, Unit unit = null, Item item = null)
    {
        // Checks each requirement item in list
        // If any requirement fails, returns false, otherwise return true

        Action action = ActionsDict[actionList];

        if (action.requirements[0] != ActionRequirement.NONE)
            foreach (ActionRequirement requirement in action.requirements)
            {
                // Check that unit meets AP cost
                if (requirement == ActionRequirement.AP)
                    if (!unit || unit.stats.actionPointsCurrent < action.cost)
                        return false;

                // Check that unit meets Ammo cost
                if (requirement == ActionRequirement.AMMO)
                    if (!unit || unit.inventory.equippedWeapon.stats.ammoCurrent <= 0)
                        return false;

                // Check that unit's ammo isn't full
                if (requirement == ActionRequirement.RELOAD)
                    if (!unit || unit.inventory.equippedWeapon.stats.ammoCurrent >= unit.inventory.equippedWeapon.stats.ammoMax)
                        return false;

                if (requirement == ActionRequirement.QUANTITY)
                    if (item.itemUsesCurrent <= 0)
                        return false;
            }

        return true;
    }

    // All possible character actions with values
    public static Action action_move = new Action("Move", ActionList.MOVE, null, 1, 0, null, ActionRequirement.NONE);
    public static Action action_shoot = new Action("Shoot", ActionList.SHOOT, "Shoot at the selected target", 1, 0, ActionButtons.btn_action_shoot, new ActionRequirement[] { ActionRequirement.AP, ActionRequirement.AMMO });
    public static Action action_reload = new Action("Reload", ActionList.RELOAD, "Reloads currently equipped weapon", 1, 0, ActionButtons.btn_action_reload, new ActionRequirement[] { ActionRequirement.AP, ActionRequirement.RELOAD });
    public static Action action_swap = new Action("Swap Guns", ActionList.SWAP, "Swap to next weapon", 0, 0, ActionButtons.btn_action_swap, ActionRequirement.NONE);
    public static Action action_chooseItem = new Action("Choose Item", ActionList.CHOOSEITEM, null, 0, 0, ActionButtons.btn_action_chooseItem, ActionRequirement.NONE);
    public static Action action_useItem = new Action("Use Item", ActionList.USEITEM, null, 1, 0, ActionButtons.btn_action_useItem, ActionRequirement.NONE);
    public static Action action_none = new Action("Do nothing.", ActionList.NONE, null, 1, 0, null, ActionRequirement.NONE);
    public static Action action_item_medkit = new Action("Medkit", ActionList.MEDKIT, "Heals a target friendly unit", 1, 0, ActionButtons.btn_action_medkit, new ActionRequirement[] { ActionRequirement.AP, ActionRequirement.QUANTITY });
    public static Action action_item_grenade = new Action("Grenade", ActionList.USEITEM, "Damages a target enemy unit", 1, 0, ActionButtons.btn_action_grenade, new ActionRequirement[] { ActionRequirement.AP, ActionRequirement.QUANTITY });
    public static Action action_item_grenade_heal = new Action("Healnade", ActionList.USEITEM, "A grenade that heals friendly units", 1, 0, ActionButtons.btn_action_grenade, new ActionRequirement[] { ActionRequirement.AP, ActionRequirement.QUANTITY });
    public static Action action_item_grenade_win = new Action("\"Win\" Grenade", ActionList.USEITEM, "For use when frustrated with the robots", 1, 0, ActionButtons.btn_action_grenade, new ActionRequirement[] { ActionRequirement.AP, ActionRequirement.QUANTITY });
    public static Action action_item_grenade_dud = new Action("Dud Grenade", ActionList.USEITEM, "A dud grenade that can still be thrown to inflict concussive damage", 1, 0, ActionButtons.btn_action_grenade, new ActionRequirement[] { ActionRequirement.AP, ActionRequirement.QUANTITY });

    // Dictionary used to match enum to actual action class object
    public static Dictionary<ActionList, Action> ActionsDict = new Dictionary<ActionList, Action>() {
        { ActionList.MOVE, action_move },
        { ActionList.SHOOT, action_shoot },
        { ActionList.RELOAD, action_reload },
        { ActionList.SWAP, action_swap },
        { ActionList.CHOOSEITEM, action_chooseItem },
        { ActionList.USEITEM, action_useItem },
        { ActionList.NONE, action_none},
        { ActionList.MEDKIT, action_item_medkit},
        { ActionList.GRENADE, action_item_grenade},
        { ActionList.GRENADE_HEAL, action_item_grenade_heal},
        { ActionList.GRENADE_WIN, action_item_grenade_win},
        { ActionList.GRENADE_DUD, action_item_grenade_dud},
    };
}

// Actions enum used for lookup
public enum ActionList { MOVE, SHOOT, RELOAD, SWAP, REFRESH, CHOOSEITEM, USEITEM, NONE, MEDKIT, GRENADE, GRENADE_HEAL, GRENADE_WIN, GRENADE_DUD }
public enum ActionRequirement { NONE, AP, AMMO, RELOAD, QUANTITY };