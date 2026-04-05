using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
public class PlayerController : MonoBehaviour
{

    public Rigidbody rb;
    public float moveSpeed,jump;
    private Vector2 moveInput;
    public LayerMask groundLayer;
    private bool isGrounded;
    public Transform groundCheck;
    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        anim.speed =1;  
    }
    void Start()
    {
        
    }

    void Update()
    {
        moveInput.x = Keyboard.current.aKey.isPressed ? -1f : (Keyboard.current.dKey.isPressed ? 1f : 0f);
        moveInput.y = Keyboard.current.sKey.isPressed ? -1f : (Keyboard.current.wKey.isPressed ? 1f : 0f);
        moveInput.Normalize();
        rb.linearVelocity = new Vector3(moveInput.x * moveSpeed, rb.linearVelocity.y, moveInput.y * moveSpeed);
        RaycastHit hit;
        if(Physics.Raycast(groundCheck.position, Vector3.down, out hit, 0.3f, groundLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
        if(Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity += (Vector3.up * jump);
        }
    }
}
