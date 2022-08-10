using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actions : MonoBehaviour
{
    public class ActionButton
    {
        // Class used to handle sprites for action buttons
        // Will be used to control mouse-over effects as well

        public Sprite active;
        public Sprite inactive;

        public ActionButton(string spritePath)
        {
            active = Resources.Load<Sprite>(spritePath);
            inactive = Resources.Load<Sprite>(spritePath + "_1");
        }
    }

    public static class ActionButtons
    {
        // Loads action button sprites from resources

        public static ActionButton btn_action_move = new ActionButton("Buttons/btn_move");
        public static ActionButton btn_action_shoot = new ActionButton("Buttons/btn_shoot");
        public static ActionButton btn_action_reload = new ActionButton("Buttons/btn_reload");
        public static ActionButton btn_action_swap = new ActionButton("Buttons/btn_swap");
    }

    // All possible actions in enum form
    public enum ActionsList { MOVE, SHOOT, RELOAD, SWAP, REFRESH }

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
        public ActionButton button;

        public Action(string aName, ActionsList aTag, string aContext, int aCost, int aCooldown, ActionButton aButton)
        {
            name = aName;
            tag = aTag;
            context = aContext;
            cost = aCost;
            cooldown = aCooldown;
            button = aButton;
        }
    }   

    // All possible character actions with values
    public static Action action_move = new Action("Move", ActionsList.MOVE, "move", 1, 0, null);
    public static Action action_shoot = new Action("Shoot", ActionsList.SHOOT, "shoot", 1, 0, ActionButtons.btn_action_shoot);
    public static Action action_reload = new Action("Reload", ActionsList.RELOAD, "reload", 1, 0, ActionButtons.btn_action_reload);
    public static Action action_swap = new Action("Swap Guns", ActionsList.SWAP, "swap", 0, 0, ActionButtons.btn_action_swap);
    public static Action action_refresh = new Action("Refresh AP", ActionsList.REFRESH, "refresh", 0, 0, ActionButtons.btn_action_swap); // TEMP ACTION TO REFRESH AP, REMOVE WHEN END TURN BTN IS ADDED

    // Dictionary used to match enum to actual action class object
    public static Dictionary<ActionsList, Action> ActionsDict = new Dictionary<ActionsList, Action>() { 
        { ActionsList.MOVE, action_move },
        { ActionsList.SHOOT, action_shoot },
        { ActionsList.RELOAD, action_reload },
        { ActionsList.SWAP, action_swap },
        { ActionsList.REFRESH, action_refresh },
    };
}
