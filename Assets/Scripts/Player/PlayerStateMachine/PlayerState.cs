using UnityEngine;

public class PlayerState 
{   
    public Player player;
    public PlayerStateMachine stateMachine;
    public string stateName;

    public PlayerState(Player _player, PlayerStateMachine _stateMachine, string _stateName)
    {
        player = _player;
        stateMachine = _stateMachine;
        stateName = _stateName;
    }

    public virtual void Enter()
    {
        Debug.Log("The current stat is " + stateName);
    }

    public virtual void Update()
    {
        
    }

    public virtual void Exit()
    {
        
    }
}
