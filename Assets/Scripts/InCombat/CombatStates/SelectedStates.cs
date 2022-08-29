using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SelectedStates
{
    #region Basics
    public class Idle : FiniteState<InCombatPlayerAction>
    {
        public Idle(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        List<Character> _allies;

        public override void Enter(InCombatPlayerAction t)
        {
            base.Enter(t);

           // ChangeState(new ChooseItem(Machine, t.selectedCharacter.inventory.items.ToArray()));

            if (t.selectedCharacter)
            {
                if(t.selectedCharacter.potentialTargets != null)
                    foreach (var v in t.selectedCharacter.potentialTargets)
                    v.IsTargetUX(false, false);

                t.selectedCharacter.potentialTargets = null;

                if (t.selectedCharacter.targetCharacter != null && t.selectedCharacter.targetCharacter.stats.healthCurrent <= 0)
                    t.selectedCharacter.targetCharacter = null;
                // t.selectedCharacter.targetCharacter = null; // Commented out because this breaks dodging
            }
        }
        public override void Execute(InCombatPlayerAction t)
        {
            if (t.selectedCharacter == null)
                ChangeState(new NoTargetSelected(Machine));
        }

        public override void InputPrimary(InCombatPlayerAction t)
        {
            Character c = t.selectedCharacter;

            if (!IsPointerOverUIElement(t))
                t.SelectUnit();

            if (t.selectedCharacter == null) ChangeState(new NoTargetSelected(Machine));
            if (t.selectedCharacter != c) ChangeState(new Idle(Machine));
        }
        public override void InputSecndry(InCombatPlayerAction t)
        {
            if (t.selectedCharacter)
            {
                RaycastHit hit;
                Ray ray;
                ray = Camera.main.ScreenPointToRay(t.playerInput.Controls.InputPosition.ReadValue<Vector2>());
                int layerMask = (1 << LayerMask.NameToLayer("TileMap"));

                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    if (hit.collider.GetComponent<Character>())
                    {
                        var v = hit.collider.GetComponent<Character>();

                        // TODO: Hold RMB = UI Popup with unit information.

                        Debug.Log("Right-cliked on a character. No functionality yet.");
                        //Right Click to shoot. NOT USED
                        {
                            /*
                            // If Right Click on Target, shoot it.
                            if (v.attributes.faction != t.selectedCharacter.attributes.faction)
                            {
                                t.selectedCharacter.targetCharacter = v;
                                ChangeState(new ShootTarget(Machine, v));
                            }
                            else
                            {
                                Debug.Log("Cannot shoot a Character of the same Faction!");
                            }
                            */
                        }
                    }

                    if (hit.collider.GetComponent<Tile>())
                    {
                        var tile = hit.collider.GetComponent<Tile>();

                        t.selectedCharacter.ProcessAction(Actions.action_move, contextTile: tile, contextPath: t.previewPath);

                        //TODO: Clean this up. Need to check that this is a valid move before sending to state machine.
                        if (t.selectedCharacter.CheckTileMove(tile) == true)
                            ChangeState(new Moving(Machine, tile));
                    }
                }
            }
        }
        public override void InputActionBtn(InCombatPlayerAction t, int index)
        {
            Actions.ActionsList action = t.GetBindings(index);

            switch (action)
            {
                case (Actions.ActionsList.RELOAD):
                    {
                        ChangeState(new Reloading(Machine));
                        break;
                    }
                case (Actions.ActionsList.SWAP):
                    {
                        ChangeState(new SwapGun(Machine));
                        break;
                    }
                case (Actions.ActionsList.REFRESH):
                    {
                        ChangeState(new RefreshingAP(Machine));
                        break;
                    }
                case (Actions.ActionsList.CHOOSEITEM):
                    {
                        if (t.selectedCharacter.inventory.GetItem(0))
                            ChangeState
                                //(new UseItem(Machine, t.selectedCharacter.inventory.GetItem(0)));
                                (new ChooseItem(Machine, t.selectedCharacter.inventory.items.ToArray()));
                        
                        break;
                    }
                case (Actions.ActionsList.SHOOT):
                    {
                        ChangeState(new ChoosingShootTarget(Machine));
                        break;
                    }
            }
        }
        public override void InputTab(InCombatPlayerAction t, bool shift)
        {
            
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
    public class ChoosingMoveDestination : FiniteState<InCombatPlayerAction>
    {
        public ChoosingMoveDestination(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        public override void Enter(InCombatPlayerAction t)
        {
            base.Enter(t);
        }

        public override void InputSecndry(InCombatPlayerAction t)
        {
            if (t.selectedCharacter.stats.actionPointsCurrent > 0)
            {
                t.MoveCharacter();
                // ChangeState(new Moving(Machine));
            }
            else
                ChangeState(new Idle(Machine));
        }

        public override void InputActionBtn(InCombatPlayerAction t, int index)
        {
            Actions.ActionsList action = t.GetBindings(index);

            switch (action)
            {
                case (Actions.ActionsList.SHOOT):
                    {
                        ChangeState(new ChoosingShootTarget(Machine));
                        break;
                    }
                case (Actions.ActionsList.RELOAD):
                    {
                        ChangeState(new Reloading(Machine));
                        break;
                    }
                case (Actions.ActionsList.SWAP):
                    {
                        ChangeState(new SwapGun(Machine));
                        break;
                    }
                case (Actions.ActionsList.REFRESH):
                    {
                        ChangeState(new RefreshingAP(Machine));
                        break;
                    }
                case (Actions.ActionsList.CHOOSEITEM):
                    {
                        ChangeState
                            (new UseItem(Machine, t.selectedCharacter.inventory.GetItem(0)));
                        break;
                    }
            }
        }
    }

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
    public class ChoosingShootTarget : FiniteState<InCombatPlayerAction>
    {
        public ChoosingShootTarget(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        List<Character> enemyList = new List<Character>();

        public override void Enter(InCombatPlayerAction t)
        {
            base.Enter(t);

            t.PathPreviewClear();

            //Make a list of Enemies
            Character[] gos = GameObject.FindObjectsOfType<Character>();
            foreach (var v in gos)
            {
                if (v.GetComponent<IFaction>() != null)
                    if (t.selectedCharacter.attributes.faction != v.attributes.faction)
                        if (v.stats.healthCurrent > 0)
                            enemyList.Add(v);
            }

            //Find closest target
            if(enemyList.Count > 0)
            {
                enemyList.Sort(delegate (Character a, Character b)
                {
                    return Vector2.Distance(t.selectedCharacter.transform.position, a.transform.position).CompareTo(Vector2.Distance(t.selectedCharacter.transform.position, b.transform.position));
                });

                t.selectedCharacter.potentialTargets = enemyList;
                t.selectedCharacter.targetCharacter = t.selectedCharacter.targetCharacter != null ? t.selectedCharacter.targetCharacter : enemyList[0];
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

            foreach(var v in enemyList)
            {
                if (v == t.selectedCharacter.targetCharacter)
                    v.IsTargetUX(true, true);
                else
                    v.IsTargetUX(false, true);
            }
        }
        public override void Exit(InCombatPlayerAction t)
        {
            base.Exit(t);
            foreach (var v in enemyList)
            {
                v.IsTargetUX(false, false);
            }
        }
        public override void InputPrimary(InCombatPlayerAction t)
        {
            //if (!IsPointerOverUIElement(t))
                //t.SelectUnit();
                
            // If valid target, make Target
            if (!IsPointerOverUIElement(t))
            {
                RaycastHit hit;
                Ray ray;
                ray = Camera.main.ScreenPointToRay(t.playerInput.Controls.InputPosition.ReadValue<Vector2>());

                int layerMask = (1 << LayerMask.NameToLayer("TileMap"));

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~layerMask))
                {
                    if (hit.collider.GetComponent<Character>())
                    {
                        var c = hit.collider.GetComponent<Character>();

                        if (enemyList.Contains(c))
                            t.selectedCharacter.targetCharacter = c;
                        else if (c.attributes.faction == t.selectedCharacter.attributes.faction)
                        { 
                            t.SelectUnit();
                            ChangeState(new Idle(Machine));
                        }
                        else
                        {
                            Debug.Log("Clicked character but unable to process. Possibly not Ally or Enemy.");
                        }
                    } 
                }
                else
                    ChangeState(new NoTargetSelected(Machine));
            }
        }
        public override void InputSecndry(InCombatPlayerAction t)
        {
            /*
            // Check for Target (Should already have).
            if (t.selectedCharacter.targetCharacter)
            {
                RaycastHit hit;
                Ray ray;
                ray = Camera.main.ScreenPointToRay(t.playerInput.Controls.InputPosition.ReadValue<Vector2>());
                int layerMask = (1 << LayerMask.NameToLayer("TileMap"));

                // What are we hitting?
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {
                    if (hit.collider.GetComponent<Character>())
                    {
                        var v = hit.collider.GetComponent<Character>();

                        // If Right Click on Target, shoot it.
                        if (v == t.selectedCharacter.targetCharacter)
                            ChangeState(new ShootTarget(Machine, v));

                        // If Potential Target, Switch Target
                        else if (t.selectedCharacter.potentialTargets.Contains(v))
                            t.selectedCharacter.targetCharacter = v;

                        // If character is not potential target, throw error
                        else if (!t.selectedCharacter.potentialTargets.Contains(v))
                            Debug.Log("Unit not Targetable.");
                    }

                    // If no character is pressed, revert to Idle.
                    else
                        ChangeState(new Idle(Machine));
                }
            }
            */
        }

        public override void InputActionBtn(InCombatPlayerAction t, int index)
        {
            Actions.ActionsList action = t.GetBindings(index);

            switch (action)
            {
                case (Actions.ActionsList.SHOOT):
                    {
                        /*
                        if (t.selectedCharacter.targetCharacter)
                            ChangeState(new ShootTarget(Machine,t.selectedCharacter.targetCharacter));
                        else
                            Debug.Log("No Target -- But how? Ensure that both characters are set to different factions.");*/

                        ChangeState(new Idle(Machine));
                        break;
                    }
            }
        }

        public override void InputSpacebar(InCombatPlayerAction t)
        {
            if (t.selectedCharacter.targetCharacter)
                ChangeState(new ShootTarget(Machine, t.selectedCharacter.targetCharacter));
            else
                Debug.Log("No Target -- But how? Ensure that both characters are set to different factions. (spacebar)");
        }

        public override void InputTab(InCombatPlayerAction t, bool shift)
        {
            int index = enemyList.IndexOf(t.selectedCharacter.targetCharacter);
            int n = shift ? index - 1 : index + 1;

            if (n < 0) n = enemyList.Count - 1;
            if (n > enemyList.Count - 1) n = 0;

            t.selectedCharacter.targetCharacter = enemyList[n];
        }
    }
    public class ShootTarget : FiniteState<InCombatPlayerAction>
    {
        Character Target;
        public ShootTarget(StateMachine<InCombatPlayerAction> machine, Character target) : base(machine) { Machine = machine; Target = target; }

        float timer;

        public override void Enter(InCombatPlayerAction t)
        {
            t.selectedCharacter.ProcessAction(Actions.action_shoot, contextCharacter: Target, contextString: "attack");

            timer = Time.time + 1;
        }
        public override void Execute(InCombatPlayerAction t)
        {
            if (timer < Time.time)
                ChangeState(new Idle(Machine));
        }
    }
    public class PostShootTarget : FiniteState<InCombatPlayerAction>
    {
        public PostShootTarget(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }
    }
    #endregion

    #region Actions
    public class Reloading : FiniteState<InCombatPlayerAction>
    {
        public Reloading(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

        float timer = 0.25f;
        public override void Enter(InCombatPlayerAction t)
        {
            timer += Time.time;
            t.selectedCharacter.ProcessAction(Actions.action_reload);
        }
        public override void Execute(InCombatPlayerAction t)
        {
            if (Time.time > timer)
            {
                ChangeState(new Idle(Machine));
            }
        }
    }
    public class RefreshingAP : FiniteState<InCombatPlayerAction>
    {
        public RefreshingAP(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }
        float timer = 0.25f;
        public override void Enter(InCombatPlayerAction t)
        {
            t.selectedCharacter.RefreshActionPoints();
            timer += Time.time;
        }

        public override void Execute(InCombatPlayerAction t)
        {
            if (timer < Time.time)
                ChangeState(new Idle(Machine));
        }
    }
    public class SwapGun : FiniteState<InCombatPlayerAction>
    {
        public SwapGun(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }
        float timer = 0.25f;
        public override void Enter(InCombatPlayerAction t)
        {
            t.selectedCharacter.ProcessAction(Actions.action_swap);
            timer += Time.time;
        }

        public override void Execute(InCombatPlayerAction t)
        {
            if (timer < Time.time)
                ChangeState(new Idle(Machine));
        }
    }
    public class UseItem : FiniteState<InCombatPlayerAction>
    {
        Item Item; List<GameObject> Targets; GameObject Target;
        public UseItem(StateMachine<InCombatPlayerAction> machine, Item item) : base(machine) { Machine = machine; Item = item; }

        public override void Enter(InCombatPlayerAction t)
        {
            base.Enter(t);
            t.PathPreviewClear();
            // The item panel
            var iPanel = GameObject.FindGameObjectWithTag("UseItemPanel").GetComponent<UseItemPanelScript>();
            iPanel.SetPanel(true, Item);

            // Show UI

            //Find Targets
            Targets = new List<GameObject>();
            switch (Item.TargetType)
            {
                case TargetType.Character:
                    FindTargets<Character>(t);
                    break;
                default:
                    Debug.Log("Types other than Character are not yet implemented");
                    break;
            }
        }
        
        public void FindTargets<T>(InCombatPlayerAction t)
        {
            var x = typeof(T);

            if (x == typeof(Character))
            {
                Character[] chars = GameObject.FindObjectsOfType<Character>();
                foreach (var v in chars)
                {
                    if(v.GetComponent<IFaction>() != null)
                    {
                        if( Item.CheckAffinity
                            (t.selectedCharacter.attributes.faction, v.attributes.faction) 
                            == true)
                        {
                            Targets.Add(v.gameObject);
                        }
                    }
                }
            }


            //Find closest Target
            if(Targets.Count > 0)
            {
                Targets.Sort(delegate (GameObject a, GameObject b)
                {
                    return Vector2.Distance(t.selectedCharacter.transform.position, a.transform.position).CompareTo(Vector2.Distance(t.selectedCharacter.transform.position, b.transform.position));
                });

                Target = Targets[0];
            }
        }

        public override void Execute(InCombatPlayerAction t)
        {
            Debug.Log("Target: " + Target.name);
            if (t.selectedCharacter == null) ChangeState(new NoTargetSelected(Machine));

            foreach (var v in Targets)
            {
                if (v.GetComponent<Character>() == true)
                {
                    Character c = v.GetComponent<Character>();

                    if (v == Target)
                        c.IsTargetUX(true, true);
                    else
                       c.IsTargetUX(false, true);
                }
            }
        }

        public override void Exit(InCombatPlayerAction t)
        {
            // Disable UI
            var iPanel = GameObject.FindGameObjectWithTag("UseItemPanel").GetComponent<UseItemPanelScript>();

            iPanel.SetPanel(false);

            foreach (var v in Targets)
            {
                v.TryGetComponent(out Character c);
                c.IsTargetUX(false, false);
            }

            base.Exit(t);
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
                    if (Targets.Contains(hit.collider.gameObject))
                    {
                        if (Target != hit.collider.gameObject)
                        {
                            Target = hit.collider.gameObject;
                        }
                    }
                    else if (hit.collider.gameObject.GetComponent<Character>())
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
            if(Target)
            {
                Item.TryUseItem(t.selectedCharacter, Target, out bool success);

                if(success)
                {
                    Item.UseTheUseItem(t.selectedCharacter, Target.GetComponent<Character>());
                    t.selectedCharacter.stats.actionPointsCurrent -= Item.statics.itemCost;

                    if (t.selectedCharacter.inventory.items.Find(x => Item) == true)
                    {
                        var i = t.selectedCharacter.inventory.items.IndexOf(Item);
                        t.selectedCharacter.inventory.RemoveChargeFromItem(i, 1);
                    }
                    else
                    {
                        Debug.Log("Item not found.");
                    }

                    //Insert Animation + Feedback

                    ChangeState(new Idle(Machine));
                }
                else
                {
                    Debug.Log("Unable to use Item. But how? Reverting to idle.");
                    ChangeState(new Idle(Machine));
                } 
            }
            else
            {
                Debug.Log("No Target to Use Item. But how. Reverting to idle.");
                ChangeState(new Idle(Machine));
            }
        }

        public override void InputTab(InCombatPlayerAction t, bool shift)
        {
            int index = Targets.IndexOf(Target);
            int n = shift ? index - 1 : index + 1;

            if (n < 0) n = Targets.Count - 1;
            if (n > Targets.Count - 1) n = 0;

            Target = Targets[n];
        }
        public override void InputActionBtn(InCombatPlayerAction t, int index)
        {
            Actions.ActionsList action = t.GetBindings(index);

            if (action == Actions.ActionsList.CHOOSEITEM)
            {
                ChangeState(new Idle(Machine));
            }
        }
    }
    public class ChooseItem : FiniteState<InCombatPlayerAction>
    {
        Item[] Items;
        InventoryPanelScript iPanel;
        public ChooseItem(StateMachine<InCombatPlayerAction> machine, Item[] items) : base(machine) { Machine = machine; Items = items; }

        public override void Enter(InCombatPlayerAction t)
        {
            base.Enter(t);

            iPanel = GameObject.Find("InventoryPanel").GetComponent<InventoryPanelScript>();
            iPanel.SetPanel(true, t.selectedCharacter.inventory.items.ToArray());
        }
        public override void Exit(InCombatPlayerAction t)
        {
            base.Exit(t);
            iPanel.SetPanel(false);
        }

        public override void InputPrimary(InCombatPlayerAction t)
        {
            Character c = t.selectedCharacter;

            if (!IsPointerOverUIElement(t))
                t.SelectUnit();

            if (t.selectedCharacter == null) ChangeState(new NoTargetSelected(Machine));
            if (t.selectedCharacter != c) ChangeState(new Idle(Machine));
        }
        public override void InputActionBtn(InCombatPlayerAction t, int index)
        {
            if(index < 1 || index > 4)
                base.InputActionBtn(t, index);
            else
            {
                if (Items.Length >= index)
                    ChangeState(new UseItem(Machine, Items[index-1]));
                else
                    Debug.Log("There is no useable item in this slot.");
            }
        }
    }
    #endregion
}

