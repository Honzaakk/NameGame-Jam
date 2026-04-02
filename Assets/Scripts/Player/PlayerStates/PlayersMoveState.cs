using UnityEngine;

public class PlayersMoveState : PlayerState
{
    public PlayersMoveState(Player _player, PlayerStateMachine _stateMachine, string _stateName) : base(_player, _stateMachine, _stateName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();
        
    }
    public override void Exit()
    {
        base.Exit();
    }
}

