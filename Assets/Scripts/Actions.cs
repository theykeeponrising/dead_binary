using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actions : MonoBehaviour
{
    // All possible actions in enum form
    public enum ActionsList { MOVE, SHOOT, RELOAD }

    public class Action
    {
        // Container for various actions
        // Name - The action name displayed
        // Context - Used internally to direct action
        // Cost - Action point cost to use action
        // Cooldown - Number of turns until action is useable again

        // FUTURE ATTR
        // Type - Action type (ability, item, etc)

        public ActionsList name;
        public string context;
        public int cost;
        public int cooldown;

        public Action(ActionsList aName, string aContext, int aCost, int aCooldown)
        {
            name = aName;
            context = aContext;
            cost = aCost;
            cooldown = aCooldown;
        }
    }   

    // All possible character actions with values
    public static Action action_move = new Action(ActionsList.MOVE, "move", 1, 0);
    public static Action action_shoot = new Action(ActionsList.SHOOT, "shoot", 1, 0);
    public static Action action_reload = new Action(ActionsList.RELOAD, "reload", 1, 0);
}
