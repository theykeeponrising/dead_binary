using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateTarget : StateCancel
{
    public List<System.Type> CompatibleActions = new List<System.Type>() { typeof(UnitActionSwap) };
    public StateTarget(StateMachine<InCombatPlayerAction> machine) : base(machine) { Machine = machine; }

    public List<Unit> targets = new List<Unit>();
    public Unit target;
    public Tile targetedTile;

    public GameObject tileSelectionCircle;

    public override void Enter(InCombatPlayerAction t)
    {
        base.Enter(t);

        // Instantiate tile selection circle
        tileSelectionCircle = t.selectedCharacter.grid.InstantiateTileSelectionCircle(Vector3.zero);
    }

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

        t.selectedCharacter.grid.DestroyTileSelectionCircle(tileSelectionCircle);

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
