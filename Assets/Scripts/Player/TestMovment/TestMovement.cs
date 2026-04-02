using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class TestMovement : MonoBehaviour
{
    private InputAction _movementAction;
    private Vector2 _movementVector;
    
    public InputActionReference movementActionReference;
    public new Rigidbody rigidbody;
    public float moveSpeed;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _movementAction = movementActionReference.action;
    }

    // Update is called once per frame
    void Update()
    {
        _movementVector = _movementAction.ReadValue<Vector2>();
        
        rigidbody.linearVelocity = new Vector3(_movementVector.x * moveSpeed, 0, _movementVector.y * moveSpeed) * Time.deltaTime;
    }
}
