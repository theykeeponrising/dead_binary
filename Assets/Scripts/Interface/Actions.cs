using System.Collections.Generic;
using UnityEngine;

public class Actions
{
    // All possible actions in enum form
    public enum ActionList { MOVE, SHOOT, RELOAD, SWAP, REFRESH, CHOOSEITEM, USEITEM, NONE }
    public enum ActionRequirement { NONE, AP, AMMO, RELOAD };

    public class Action
    {
        // Container for various actions
        // aname - The action name displayed
        // atag - Used to match enums, such as Character.availableActions
        // context - Used internally to provide context to action, such as action variations
        // cost - Action point cost to use action
        // cooldown - Number of turns until action is useable again
        // buttonPath - The sprites used with the action button (if applicable)
        // requirements - Requirements to perform action

        // FUTURE ATTR
        // Type - Action type (ability, item, etc)

        public string aname;
        public ActionList atag;
        public string context;
        public int cost;
        public int cooldown;
        public string buttonPath;
        public ActionRequirement[] requirements;

        public Action(string aName, ActionList aTag, string aContext, int aCost, int aCooldown, string aButtonPath, ActionRequirement[] aRequirements)
        {
            aname = aName;
            atag = aTag;
            context = aContext;
            cost = aCost;
            cooldown = aCooldown;
            buttonPath = aButtonPath;
            requirements = aRequirements;
        }

        public Action(string aName, ActionList aTag, string aContext, int aCost, int aCooldown, string aButtonPath, ActionRequirement aRequirements)
        {
            aname = aName;
            atag = aTag;
            context = aContext;
            cost = aCost;
            cooldown = aCooldown;
            buttonPath = aButtonPath;
            requirements = new ActionRequirement[] { aRequirements };
        }

        public bool CheckRequirements(Unit unit)
        {
            // Checks each requirement item in list
            // If any requirement fails, returns false, otherwise return true

            if (requirements[0] != ActionRequirement.NONE)
                foreach (ActionRequirement requirement in requirements)
                {
                    // Check that unit meets AP cost
                    if (requirement == ActionRequirement.AP)
                        if (unit.stats.actionPointsCurrent < cost)
                            return false;

                    // Check that unit meets Ammo cost
                    if (requirement == ActionRequirement.AMMO)
                        if (unit.inventory.equippedWeapon.stats.ammoCurrent <= 0)
                            return false;

                    // Check that unit's ammo isn't full
                    if (requirement == ActionRequirement.RELOAD)
                        if (unit.inventory.equippedWeapon.stats.ammoCurrent >= unit.inventory.equippedWeapon.stats.ammoMax)
                            return false;
                }

            return true;
        }
    }

    // All possible character actions with values
    public static Action action_move = new Action("Move", ActionList.MOVE, "move", 1, 0, null, ActionRequirement.NONE);
    public static Action action_shoot = new Action("Shoot", ActionList.SHOOT, "shoot", 1, 0, ActionButtons.btn_action_shoot, new ActionRequirement[]{ ActionRequirement.AP, ActionRequirement.AMMO});
    public static Action action_reload = new Action("Reload", ActionList.RELOAD, "reload", 1, 0, ActionButtons.btn_action_reload, new ActionRequirement[] { ActionRequirement.AP, ActionRequirement.RELOAD });
    public static Action action_swap = new Action("Swap Guns", ActionList.SWAP, "swap", 0, 0, ActionButtons.btn_action_swap, ActionRequirement.NONE);
    public static Action action_chooseItem = new Action("Choose Item", ActionList.CHOOSEITEM, "chooseItem", 0, 0, ActionButtons.btn_action_chooseItem, ActionRequirement.NONE);
    public static Action action_useItem = new Action("Use Item", ActionList.USEITEM, "useItem", 1, 0, ActionButtons.btn_action_useItem, ActionRequirement.NONE);
    public static Action action_none = new Action("Do nothing.", ActionList.NONE, "none", 1, 0, null, ActionRequirement.NONE);

    // Dictionary used to match enum to actual action class object
    public static Dictionary<ActionList, Action> ActionsDict = new Dictionary<ActionList, Action>() {
        { ActionList.MOVE, action_move },
        { ActionList.SHOOT, action_shoot },
        { ActionList.RELOAD, action_reload },
        { ActionList.SWAP, action_swap },
        { ActionList.CHOOSEITEM, action_chooseItem },
        { ActionList.USEITEM, action_useItem },
        { ActionList.NONE, action_none}
    };
}
