using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SelectedStates
{
    #region Generic States
    public class CancelState : FiniteState<InCombatPlayerAction>
    {
        public CancelState(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        public InfoPanelScript infoPanel = UIManager.Instance.infoPanel;
        public InventoryPanelScript inventoryPanel = UIManager.Instance.inventoryPanel;

        public override void InputSecndry(InCombatPlayerAction t)
        {
            // Right mouse button will exit targeting

            t.selectedCharacter.GetActor().ClearTarget();
            ChangeState(new Idle(Machine));
        }

        public override void InputCancel(InCombatPlayerAction t)
        {
            t.selectedCharacter.GetActor().ClearTarget();
            ChangeState(new Idle(Machine));
        }
    }

    public class TargetState : CancelState
    {
        public TargetState(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        public List<Unit> targets = new List<Unit>();
        public Unit target;

        public override void Exit(InCombatPlayerAction t)
        {
            // Disable UI
            inventoryPanel.gameObject.SetActive(false);
            infoPanel.gameObject.SetActive(false);

            foreach (var v in targets)
            {
                v.TryGetComponent(out Unit c);
                c.GetActor().IsTargetUX(false, false);
            }

            base.Exit(t);
        }

        public override void InputPrimary(InCombatPlayerAction t)
        {
            // If valid target, make Target
            if (!IsPointerOverUIElement(t))
            {
                Camera raycastCamera = Camera.main;
                RaycastHit hit;
                Ray ray;

                if (t.selectedCharacter.GetComponentInChildren<Camera>())
                    raycastCamera = t.selectedCharacter.GetComponentInChildren<Camera>();

                ray = raycastCamera.ScreenPointToRay(t.playerInput.Controls.InputPosition.ReadValue<Vector2>());

                int layerMask = (1 << LayerMask.NameToLayer("TileMap"));

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~layerMask))
                {
                    if (hit.collider.GetComponent<Unit>())
                    {
                        var c = hit.collider.GetComponent<Unit>();

                        if (targets.Contains(c))
                            t.selectedCharacter.GetActor().targetCharacter = c;
                    }
                }
            }
        }

        public override void InputTab(InCombatPlayerAction t, bool shift)
        {
            int index = targets.IndexOf(target);
            int n = shift ? index - 1 : index + 1;

            if (n < 0) n = targets.Count - 1;
            if (n > targets.Count - 1) n = 0;

            target = targets[n];
            t.selectedCharacter.GetActor().targetCharacter = target;
        }
    }

    public class TimedActionState : FiniteState<InCombatPlayerAction>
    {
        Action action;
        public TimedActionState(StateMachine<InCombatPlayerAction> machine, Action performAction) : base(machine) { Machine = machine; action = performAction; }

        float timer = 0.25f;
        public override void Enter(InCombatPlayerAction t)
        {
            t.selectedCharacter.GetActor().ProcessAction(action);
            timer += Time.time;
        }

        public override void Execute(InCombatPlayerAction t)
        {
            if (timer < Time.time)
                ChangeState(new Idle(Machine));
        }
    }
    #endregion

    #region Basics
    public class Idle : FiniteState<InCombatPlayerAction>
    {
        public Idle(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        public override void Enter(InCombatPlayerAction t)
        {
            base.Enter(t);

            if (t.selectedCharacter)
            {
                if(t.selectedCharacter.GetActor().potentialTargets != null)
                    foreach (var v in t.selectedCharacter.GetActor().potentialTargets)
                    v.GetActor().IsTargetUX(false, false);

                t.selectedCharacter.GetActor().potentialTargets = null;

                if (t.selectedCharacter.GetActor().targetCharacter != null && t.selectedCharacter.GetActor().targetCharacter.stats.healthCurrent <= 0)
                    t.selectedCharacter.GetActor().targetCharacter = null;
            }
        }

        public override void Execute(InCombatPlayerAction t)
        {
            if (t.selectedCharacter == null)
                ChangeState(new NoTargetSelected(Machine));
        }

        public override void InputPrimary(InCombatPlayerAction t)
        {
            Unit c = t.selectedCharacter;

            if (!IsPointerOverUIElement(t))
                t.SelectUnit();

            if (t.selectedCharacter == null) ChangeState(new NoTargetSelected(Machine));
            if (t.selectedCharacter != c) ChangeState(new Idle(Machine));
        }

        public override void InputSecndry(InCombatPlayerAction t)
        {
            // Orders selected character to move to target tile

            // Check that we have a selected character and they meet the AP cost
            if (t.selectedCharacter && t.selectedCharacter.stats.actionPointsCurrent >= Action.action_move.cost)
            {
                RaycastHit hit;
                Ray ray;
                ray = Camera.main.ScreenPointToRay(t.playerInput.Controls.InputPosition.ReadValue<Vector2>());
                int layerMask = (1 << LayerMask.NameToLayer("TileMap"));

                // Find tile from right-click action
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {
                    if (hit.collider.GetComponent<Tile>())
                    {
                        var tile = hit.collider.GetComponent<Tile>();

                        t.selectedCharacter.GetActor().ProcessAction(Action.action_move, contextTile: tile, contextPath: t.previewPath);

                        //TODO: Clean this up. Need to check that this is a valid move before sending to state machine.
                        if (t.selectedCharacter.GetActor().CheckTileMove(tile) == true)
                            ChangeState(new Moving(Machine, tile));
                    }
                }
            }

            // If we can't perform move, inform player
            else if (t.selectedCharacter && t.selectedCharacter.stats.actionPointsCurrent < Action.action_move.cost)
            {
                Debug.Log("Out of AP, cannot move!"); // TODO: Display this to player in UI
            }

            // Something went wrong
            else
                Debug.Log("WARNING - Invalid move order detected");
        }

        public override void InputActionBtn(InCombatPlayerAction t, int index)
        {
            ActionList action = t.GetBindings(index);

            // If requirements aren't met, ignore button press
            bool requirementsMet = Action.CheckRequirements(action, t.selectedCharacter);
            if (!requirementsMet) return;

            ButtonPress(index);

            switch (action)
            {
                case (ActionList.RELOAD):
                    {
                        ChangeState(new TimedActionState(Machine, Action.action_reload));
                        break;
                    }
                case (ActionList.SWAP):
                    {
                        ChangeState(new TimedActionState(Machine, Action.action_swap));
                        break;
                    }
                case (ActionList.CHOOSEITEM):
                    {
                        //if (t.selectedCharacter.inventory.GetItem(0))
                            ChangeState
                                //(new UseItem(Machine, t.selectedCharacter.inventory.GetItem(0)));
                                (new ChooseItem(Machine, t.selectedCharacter.inventory.items.ToArray()));
                        
                        break;
                    }
                case (ActionList.SHOOT):
                    {
                        ChangeState(new ChoosingShootTarget(Machine));
                        break;
                    }
            }
        }

        public override void InputTab(InCombatPlayerAction t, bool shift)
        {
            // Iterate through player characters
        }
    }

    public class NoTargetSelected : FiniteState<InCombatPlayerAction>
    {
        public NoTargetSelected(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        /*
        public override void Enter(InCombatPlayerAction t)
        {
            base.Enter(t);
        }
        */
        public override void Execute(InCombatPlayerAction t)
        {
            if (t.selectedCharacter)
                ChangeState(new Idle(Machine));
        }

        public override void InputPrimary(InCombatPlayerAction t)
        {
            if (!IsPointerOverUIElement(t))
                t.SelectUnit();
        }
    }
    #endregion

    #region Movement
    public class Moving : FiniteState<InCombatPlayerAction>
    {
        public Moving(StateMachine<InCombatPlayerAction> machine, Tile destination) : base(machine) { Machine = machine; _destination = destination; }

        Tile _destination;
        float timer = 5; //Failsafe

        public override void Enter(InCombatPlayerAction t)
        {
            timer += Time.time;
        }

        public override void Execute(InCombatPlayerAction t)
        {
            if (t.selectedCharacter)
                if (t.selectedCharacter.currentTile == _destination)
                    ChangeState(new Idle(Machine));

            if (Time.time > timer)
            {
                Debug.LogWarning("Character timed out of Moving. Reverting to Idle state.");
                ChangeState(new Idle(Machine));
            }
        }
    }
    #endregion

    #region Combat
    public class ChoosingShootTarget : TargetState
    {
        public ChoosingShootTarget(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        public override void Enter(InCombatPlayerAction t)
        {
            base.Enter(t);

            t.PathPreviewClear();
            infoPanel.UpdateAction(ActionList.SHOOT);

            //Make a list of Enemies
            Unit[] gos = GameObject.FindObjectsOfType<Unit>();
            foreach (var v in gos)
            {
                if (v.GetComponent<IFaction>() != null)
                    if (t.selectedCharacter.attributes.faction != v.attributes.faction)
                        if (v.stats.healthCurrent > 0)
                            targets.Add(v);
            }

            //Find closest target
            if(targets.Count > 0)
            {
                targets.Sort(delegate (Unit a, Unit b)
                {
                    return Vector2.Distance(t.selectedCharacter.transform.position, a.transform.position).CompareTo(Vector2.Distance(t.selectedCharacter.transform.position, b.transform.position));
                });

                t.selectedCharacter.GetActor().potentialTargets = targets;
                t.selectedCharacter.GetActor().targetCharacter = t.selectedCharacter.GetActor().targetCharacter != null ? t.selectedCharacter.GetActor().targetCharacter : targets[0];
                t.selectedCharacter.GetActor().GetTarget();
                target = targets[0];
            }
            else
            {
                Debug.LogWarning("There are no nearby enemies. If there should be, check to see that their Faction is not the same as the Selected Character. Reverting to Idle");
                ChangeState(new Idle(Machine));
            }
        }

        public override void Execute(InCombatPlayerAction t)
        {
            if (t.selectedCharacter == null) ChangeState(new NoTargetSelected(Machine));

            foreach(var v in targets)
            {
                if (v == t.selectedCharacter.GetActor().targetCharacter)
                    v.GetActor().IsTargetUX(true, true);
                else
                    v.GetActor().IsTargetUX(false, true);
            }
        }

        public override void InputPrimary(InCombatPlayerAction t)
        {             
            // If valid target, make Target
            if (!IsPointerOverUIElement(t))
            {
                Camera raycastCamera = Camera.main;
                RaycastHit hit;
                Ray ray;

                if (t.selectedCharacter.GetComponentInChildren<Camera>())
                    raycastCamera = t.selectedCharacter.GetComponentInChildren<Camera>();

                ray = raycastCamera.ScreenPointToRay(t.playerInput.Controls.InputPosition.ReadValue<Vector2>());

                int layerMask = (1 << LayerMask.NameToLayer("TileMap"));

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~layerMask))
                {
                    if (hit.collider.GetComponent<Unit>())
                    {
                        var c = hit.collider.GetComponent<Unit>();

                        if (targets.Contains(c))
                            t.selectedCharacter.GetActor().targetCharacter = c;
                    } 
                }
            }
        }

        public override void InputActionBtn(InCombatPlayerAction t, int index)
        {
            // Perform action based on which button was selected

            ActionList action = t.GetBindings(index);

            // Button press effect
            ButtonPress(index);

            switch (action)
            {
                case (ActionList.SHOOT):
                    InputSpacebar(t);
                    break;
                case (ActionList.RELOAD):
                    t.selectedCharacter.GetActor().ClearTarget();
                    ChangeState(new TimedActionState(Machine, Action.action_reload));
                    break;
                case (ActionList.SWAP):
                    ChangeState(new TimedActionState(Machine, Action.action_swap));
                    ChangeState(new ChoosingShootTarget(Machine));
                    break;
                case (ActionList.CHOOSEITEM):
                    t.selectedCharacter.GetActor().ClearTarget();
                    ChangeState(new ChooseItem(Machine, t.selectedCharacter.inventory.items.ToArray()));
                    break;
            }
        }

        public override void InputSpacebar(InCombatPlayerAction t)
        {
            // Spacebar and shoot action will execute shoot while in targeting

            infoPanel.gameObject.SetActive(false);
            if (t.selectedCharacter.GetActor().targetCharacter)
                ChangeState(new ShootTarget(Machine, t.selectedCharacter.GetActor().targetCharacter));
            else
                Debug.Log("No Target -- But how? Ensure that both characters are set to different factions. (spacebar)");
        }
    }

    public class ShootTarget : FiniteState<InCombatPlayerAction>
    {
        Unit Target;
        public ShootTarget(StateMachine<InCombatPlayerAction> machine, Unit target) : base(machine) { Machine = machine; Target = target; }

        float timer;

        public override void Enter(InCombatPlayerAction t)
        {
            t.selectedCharacter.GetActor().ProcessAction(Action.action_shoot, contextCharacter: Target);

            timer = Time.time + 1;
        }

        public override void Execute(InCombatPlayerAction t)
        {
            if (timer < Time.time - 1.5f)
            {
                t.selectedCharacter.GetActor().ClearTarget();
                ChangeState(new Idle(Machine));
            }
        }
    }

    public class PostShootTarget : FiniteState<InCombatPlayerAction>
    {
        public PostShootTarget(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }
    }
    #endregion

    #region Actions
    public class UseItem : TargetState
    {
        Item item;

        public UseItem(StateMachine<InCombatPlayerAction> machine, Item useItem) : base(machine) { Machine = machine; item = useItem; }

        public override void Enter(InCombatPlayerAction t)
        {
            base.Enter(t);
            t.PathPreviewClear();

            // Display info panel
            infoPanel.gameObject.SetActive(true);
            infoPanel.UpdateAction(item.itemAction);
            infoPanel.UpdateHit(-1);

            if (item.GetType().BaseType == typeof(DamageItem))
            {
                var itemType = (DamageItem)item;
                infoPanel.UpdateDamage(itemType.hpAmount);
            }

            //Find Targets
            switch (item.targetType)
            {
                case TargetType.CHARACTER:
                    FindTargets<Unit>(t);
                    break;
                default:
                    Debug.Log("Types other than Character are not yet implemented");
                    break;
            }
        }

        public void FindTargets<T>(InCombatPlayerAction t)
        {
            var x = typeof(T);

            if (x == typeof(Unit))
            {
                Unit[] chars = GameObject.FindObjectsOfType<Unit>();
                foreach (var v in chars)
                {
                    if (v.GetComponent<IFaction>() != null && v.stats.healthCurrent > 0)
                    {
                        if (item.CheckAffinity (t.selectedCharacter, v) == true)
                        {
                            targets.Add(v);
                        }
                    }
                }
            }

            //Find closest Target
            if (targets.Count > 0)
            {
                targets.Sort(delegate (Unit a, Unit b)
                {
                    return Vector2.Distance(t.selectedCharacter.transform.position, a.transform.position).CompareTo(Vector2.Distance(t.selectedCharacter.transform.position, b.transform.position));
                });

                target = targets[0];
            }
        }

        public override void Execute(InCombatPlayerAction t)
        {
            if (t.selectedCharacter == null) ChangeState(new NoTargetSelected(Machine));

            foreach (var v in targets)
            {
                if (v.GetComponent<Unit>() == true)
                {
                    Unit c = v.GetComponent<Unit>();

                    if (v == target)
                        c.GetActor().IsTargetUX(true, true);
                    else
                        c.GetActor().IsTargetUX(false, true);
                }
            }
        }

        public override void InputPrimary(InCombatPlayerAction t)
        {
            if (!IsPointerOverUIElement(t))
            {
                RaycastHit hit;
                Ray ray;
                ray = Camera.main.ScreenPointToRay(t.playerInput.Controls.InputPosition.ReadValue<Vector2>());

                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    if (targets.Contains(hit.collider.GetComponent<Unit>()))
                    {
                        if (target != hit.collider.GetComponent<Unit>())
                        {
                            target = hit.collider.GetComponent<Unit>();
                        }
                    }
                    else if (hit.collider.gameObject.GetComponent<Unit>())
                    {
                        Debug.Log("Not a target but don't want to revert to idle. Do nothing.");
                    }
                    else
                        ChangeState(new Idle(Machine));
                }
            }
        }

        public override void InputSpacebar(InCombatPlayerAction t)
        {
            if (target)
            {
                t.selectedCharacter.GetActor().ProcessAction(Action.ActionsDict[item.itemAction], contextCharacter: target, contextItem: item);
            }
            else
                Debug.Log("No Target to Use Item. But how. Reverting to idle.");
            ChangeState(new Idle(Machine));
        }

        public override void InputActionBtn(InCombatPlayerAction t, int index)
        {
            ActionList action = t.GetBindings(index);

            if (action == ActionList.CHOOSEITEM)
            {
                ChangeState(new Idle(Machine));
            }
            {

            // If requirements aren't met, ignore button press
            bool requirementsMet = Action.CheckRequirements(action, t.selectedCharacter);
            if (!requirementsMet) return;

            ButtonPress(index);

            switch (action)
            {
                case (ActionList.RELOAD):
                    {
                        ChangeState(new TimedActionState(Machine, Action.action_reload));
                        break;
                    }
                case (ActionList.SWAP):
                    {
                        ChangeState(new TimedActionState(Machine, Action.action_swap));
                        break;
                    }
                case (ActionList.SHOOT):
                    {
                        ChangeState(new ChoosingShootTarget(Machine));
                        break;
                    }
                }
            }
        }
    }

    public class ChooseItem : CancelState
    {
        Item[] Items;

        public ChooseItem(StateMachine<InCombatPlayerAction> machine, Item[] items) : base(machine) { Machine = machine; Items = items; }

        public override void Enter(InCombatPlayerAction t)
        {
            base.Enter(t);
            inventoryPanel.gameObject.SetActive(true);
            inventoryPanel.GetComponent<InventoryPanelScript>().BindButtons();
        }
        public override void Exit(InCombatPlayerAction t)
        {
            base.Exit(t);
            inventoryPanel.gameObject.SetActive(false);
        }

        public override void InputPrimary(InCombatPlayerAction t)
        {
            Unit c = t.selectedCharacter;

            if (!IsPointerOverUIElement(t))
                t.SelectUnit();

            if (t.selectedCharacter == null) ChangeState(new NoTargetSelected(Machine));
            if (t.selectedCharacter != c) ChangeState(new Idle(Machine));
        }

        public override void InputActionBtn(InCombatPlayerAction t, int index)
        {
            ActionPanelScript actionPanel = UIManager.Instance.actionPanel;
            int offset = actionPanel.GetButtons().Count;
            ActionList action = t.GetBindings(index);

            // If the Choose Item button was selected again, close the panel and return to Idle
            if (action == ActionList.CHOOSEITEM)
            {
                ButtonPress(index);
                InputCancel(t);
                return;
            }

            // If a normal action button was selected, perform that action
            else if (index <= offset)
            {
                new Idle(Machine).InputActionBtn(t, index);
                return;
            }

            // Valid item was selected, check requirements and then proceed to use
            else if (Items.Length >= index - offset && Action.CheckRequirements(Items[index - 1 - offset].itemAction, t.selectedCharacter, Items[index - 1 - offset]))
            {
                ButtonPress(index);
                ChangeState(new UseItem(Machine, Items[index - 1 - offset]));
            }

            else
                Debug.Log("Invalid item selection.");
        }
    }
    #endregion
}

