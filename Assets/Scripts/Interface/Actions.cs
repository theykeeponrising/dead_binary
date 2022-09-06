using System.Collections.Generic;
using UnityEngine;

public class Actions : MonoBehaviour
{
    // All possible actions in enum form
    public enum ActionsList { MOVE, SHOOT, RELOAD, SWAP, REFRESH, CHOOSEITEM, USEITEM, NONE }

    public class Action
    {
        // Container for various actions
        // Name - The action name displayed
        // Tag - Used to match enums, such as Character.availableActions
        // Context - Used internally to provide context to action
        // Cost - Action point cost to use action
        // Cooldown - Number of turns until action is useable again
        // Button - The sprites used with the action button (if applicable)

        // FUTURE ATTR
        // Type - Action type (ability, item, etc)

        public string name;
        public ActionsList tag;
        public string context;
        public int cost;
        public int cooldown;
        public string buttonPath;

        public Action(string aName, ActionsList aTag, string aContext, int aCost, int aCooldown, string aButtonPath)
        {
            name = aName;
            tag = aTag;
            context = aContext;
            cost = aCost;
            cooldown = aCooldown;
            buttonPath = aButtonPath;
        }
    }   

    // All possible character actions with values
    public static Action action_move = new Action("Move", ActionsList.MOVE, "move", 1, 0, null);
    public static Action action_shoot = new Action("Shoot", ActionsList.SHOOT, "shoot", 1, 0, ActionButtons.btn_action_shoot);
    public static Action action_reload = new Action("Reload", ActionsList.RELOAD, "reload", 1, 0, ActionButtons.btn_action_reload);
    public static Action action_swap = new Action("Swap Guns", ActionsList.SWAP, "swap", 0, 0, ActionButtons.btn_action_swap);

    public static Action action_refresh = new Action("Refresh AP", ActionsList.REFRESH, "refresh", 0, 0, ActionButtons.btn_action_swap); // TEMP ACTION TO REFRESH AP, REMOVE WHEN END TURN BTN IS ADDED

    public static Action action_chooseItem = new Action("Choose Item", ActionsList.CHOOSEITEM, "chooseItem", 0, 0, ActionButtons.btn_action_chooseItem);

    public static Action action_useItem = new Action("Use Item", ActionsList.USEITEM, "useItem", 1, 0, ActionButtons.btn_action_useItem);
    public static Action action_none = new Action("Do nothing.", ActionsList.NONE, "none", 1, 0, null);

    // Dictionary used to match enum to actual action class object
    public static Dictionary<ActionsList, Action> ActionsDict = new Dictionary<ActionsList, Action>() {
        { ActionsList.MOVE, action_move },
        { ActionsList.SHOOT, action_shoot },
        { ActionsList.RELOAD, action_reload },
        { ActionsList.SWAP, action_swap },
        { ActionsList.REFRESH, action_refresh },
        { ActionsList.CHOOSEITEM, action_chooseItem },
        { ActionsList.USEITEM, action_useItem },
        { ActionsList.NONE, action_none}
    };
}
