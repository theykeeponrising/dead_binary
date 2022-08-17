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
        playerAction.Init();
        playerActionStateMachine = new StateMachine<InCombatPlayerAction>();
        playerActionStateMachine.Configure(playerAction, new SelectedStates.NoTargetSelected(playerActionStateMachine));   
    }

    public override void Start()
    {
        playerAction.Start();
        base.Start();
    }

    public override void Update() {
        base.Update();
        HandleInput();
        
        if (playerActionStateMachine != null)
        {
            playerActionStateMachine.Update();
            playerAction.GetPlayerActionUI().GetStateText().text = playerActionStateMachine.GetCurrentState().GetType().Name;
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        HandleContinuousInput();
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
        playerAction.EnablePlayerInput();
        playerAction.playerInput.Controls.InputMenu.performed += _ => stateHandler.ChangeState(StateHandler.State.StatusMenuState);
    }

    public override void SetStateInactive()
    {
        playerAction.DisablePlayerInput();
    }
}
