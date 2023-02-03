using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//State is active during player turn, and inactive during enemy turn
public class PlayerTurnState : GameState
{
    private InCombatPlayerAction playerAction;
    private StateMachine<InCombatPlayerAction> playerActionStateMachine;
    public enum PlayerActionState {
        Idle,
        NoTargetSelected,
        ChoosingMoveDestination,
        Moving,
        ChoosingShootTarget,
        ShootTarget,
        PostShootTarget,
        Reloading,
        RefreshingAP,
        SwapGun,
    };

    public override void Init(GameState parentState, StateHandler stateHandler)
    {
        base.Init(parentState, stateHandler);
        this.stateEnum = StateHandler.State.PlayerTurnState;
        playerAction = new InCombatPlayerAction();
        playerAction.Init(this);
        playerActionStateMachine = new StateMachine<InCombatPlayerAction>();
        playerActionStateMachine.Configure(playerAction, new StateNoSelection(playerActionStateMachine));
        playerAction.SetStateMachine(playerActionStateMachine);
    }

    public override void Start()
    {
        playerAction.Start();
        base.Start();
    }

    public override void Update() {
        base.Update();
        UpdatePlayerTurnState();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        HandleContinuousInput();
    }

    public void EndTurn()
    {
        playerActionStateMachine.ChangeState(new StateNoSelection(playerActionStateMachine));
        UpdatePlayerTurnState();
        FactionManager.ACS.EndTurn();
        this.ChangeState(StateHandler.State.EnemyTurnState);
    }
    
    public InCombatPlayerAction GetPlayerAction()
    {
        return playerAction;
    }

    // Update is called once per frame
    public override bool HandleKeyPress()
    {
        // True/False if any key is pressed while in state
        return (playerAction.playerInput.Controls.AnyKey.ReadValue<float>() > 0.5f);
    }

    public override void SetStateActive()
    {
        base.SetStateActive();
        playerAction.EnablePlayerInput();

        //If coming from Esc/Status Menu, do NOT restart turn.
        if (StatusMenuUI.ISSTATUS == false)
        {
            FactionManager.PV.StartTurn();
            playerAction.StartTurn();
        }
    }

    public override void SetStateInactive()
    {
        base.SetStateInactive();
        playerAction.DisablePlayerInput();
    }

    private void UpdatePlayerTurnState()
    {
        HandleInput();
        playerAction.Update();
        
        if (playerActionStateMachine != null)
        {
            playerActionStateMachine.Update();
            playerAction.GetPlayerActionUI().GetStateText().text = playerActionStateMachine.GetCurrentState().GetType().Name;
        }
    }
}
