using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickupComponent : MonoBehaviour
{
    [SerializeField, Min(0.0001f)] private float radius = 2;
    [SerializeField] private InputActionReference pickupActionReference;
    
    private SphereCollider _collider = new();
    private InputAction _pickupAction;
    private bool _isPlayerCanPickup, _isPlayerHolding;
    private GameObject _player;
    private Rigidbody _rigidbody;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _isPlayerCanPickup = true;
        print("Collided");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _isPlayerCanPickup = false;
        print("Uncollided");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        
        _player = GameObject.FindGameObjectWithTag("Player");
        
        _pickupAction = pickupActionReference.action;
        
        _collider = gameObject.AddComponent<SphereCollider>();
        
        _collider.isTrigger = true;
        _collider.center = Vector3.zero;
        _collider.radius = radius;
    }

    // Update is called once per frame
    void Update()
    {
        if (_pickupAction.WasPerformedThisFrame())
        {
            if (_isPlayerHolding)
                Drop();
            else if (_isPlayerCanPickup)
                Pickup();
        }
    }

    private void Pickup()
    {
        transform.SetParent(_player.transform);
        transform.localPosition = new Vector3(transform.localPosition.x, 1, transform.localPosition.z);
        
        _rigidbody.constraints = RigidbodyConstraints.FreezePosition 
                                 | RigidbodyConstraints.FreezeRotation;
        
        _isPlayerHolding =  true;
        print("You picked up The Cube");
    }

    private void Drop()
    {
        transform.SetParent(null);
        
        _rigidbody.constraints =  RigidbodyConstraints.FreezePositionX 
                                  | RigidbodyConstraints.FreezePositionZ 
                                  | RigidbodyConstraints.FreezeRotation;
        
        _isPlayerHolding = false;
        print("You drop The Cube");
    }
}
