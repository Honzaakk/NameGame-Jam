using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("Movement speed (units/sec)")]
    public float speed = 20f;

    [Tooltip("Direction the obstacle travels (default: -Z toward player)")]
    public Vector3 direction = Vector3.back;

    [Tooltip("Auto-destroy after this many seconds (0 = never)")]
    public float lifetime = 5f;

    void Start()
    {
        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);
    }
}
