using UnityEngine;

public class Player : MonoBehaviour
{   
    private PlayerStateMachine stateMachine;
    private PlayersIdleState idleState;
    private PlayersMoveState moveState;
    void Awake()
    {
        stateMachine = new PlayerStateMachine();
        idleState = new PlayersIdleState(this,stateMachine,"Idle");
        moveState = new PlayersMoveState(this,stateMachine,"Move");
        stateMachine.FirstState(idleState);

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.currentState.Update(); // to update the current state
    }
}
