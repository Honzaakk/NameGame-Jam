using UnityEngine;
using Unity.Cinemachine;

public class ChangeCamera : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private CinemachineCamera camera1;
    [SerializeField] private CinemachineCamera camera2;
    
    [SerializeField] private GameObject spawnerObject;

    [Header("Priority Settings")]
    [SerializeField] private int highPriority = 10;
    [SerializeField] private int lowPriority = 0;

    private bool isCamera1Active = true;

    void Start()
    {
        // Set initial priorities
        if (camera1 != null)
            camera1.Priority = highPriority;
        
        if (camera2 != null)
            camera2.Priority = lowPriority;
    }

    // Call this method from a UI Button's OnClick event
    public void SwitchCamera()
    {
        if (camera1 == null || camera2 == null)
        {
            Debug.LogWarning("Camera references are not assigned!");
            return;
        }

        if (isCamera1Active)
        {
            camera1.Priority = lowPriority;
            camera2.Priority = highPriority;
        }
        else
        {
            camera1.Priority = highPriority;
            camera2.Priority = lowPriority;
        }

        isCamera1Active = !isCamera1Active;
        if (spawnerObject.TryGetComponent<ObstaclesSpawner>(out var spawner))
        {
            spawner.StartSpawn();
        }
    }

    // Optional: Switch to a specific camera
    public void SwitchToCamera1()
    {
        if (camera1 == null || camera2 == null) return;
        
        camera1.Priority = highPriority;
        camera2.Priority = lowPriority;
        isCamera1Active = true;
    }

    public void SwitchToCamera2()
    {
        if (camera1 == null || camera2 == null) return;
        
        camera1.Priority = lowPriority;
        camera2.Priority = highPriority;
        isCamera1Active = false;
    }
}
