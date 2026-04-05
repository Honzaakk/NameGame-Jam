using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObstaclesSpawner : MonoBehaviour
{
    [Header("Lane Settings")]
    public float centreX = 0f;
    public float laneWidth = 3f;
    public float laneSnapSpeed = 10f;

    [Header("Clamp Settings")]
    public float minX = -6f;
    public float maxX = 6f;
    
    [Header("Obstacle Settings")]
    [SerializeField] private GameObject[] prefabs;

    // -1 = Left, 0 = Centre, 1 = Right
    private int currentLane = 0;
    private float targetX;
    private float velocityY;
    private float groundY;
    
    private Coroutine spawnRoutine, Move;
    

    void Start()
    {
        groundY = transform.position.y;
        targetX = LaneToX(currentLane);
        StartCoroutine(HandleLaneRandomMove());
    }
    
    IEnumerator HandleLaneRandomMove()
    {
        while (true)
        {
            currentLane = Random.Range(-1, 2);
            targetX = Mathf.Clamp(LaneToX(currentLane), minX, maxX);
            MovePlayer();
            
            yield return new WaitForSeconds(1f);
        }
    }

    public void StartSpawn()
    {
        spawnRoutine = StartCoroutine(SpawnObstacles());
    }

    public void StopSpawn()
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
    }

    IEnumerator SpawnObstacles()
    {
        while(true)
        {
            if (Random.Range(0f, 1f) <= .5f)
            {
                var cube = Instantiate(prefabs[Random.Range(0, prefabs.Length)], transform.position, Quaternion.identity);
                if (cube.TryGetComponent<MoveObstacle>(out var obstacle))
                {
                    obstacle.speed = Random.Range(10, 51);
                }
                
                yield return new WaitForSeconds(1f);
            }
            yield return new WaitForSeconds(Random.Range(0f, 3f));
        }
    }

    void MovePlayer()
    {
        float newX = Mathf.Lerp(transform.position.x, targetX, laneSnapSpeed);
        float newY = Mathf.Max(transform.position.y + velocityY, groundY);

        if (newY <= groundY) velocityY = 0f;

        transform.position = new Vector3(newX, newY, transform.position.z);
    }

    float LaneToX(int lane) => centreX + lane * laneWidth;

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        float y = transform.position.y;
        float z = transform.position.z;
        float lineHeight = 4f;

        int[] lanes = { -1, 0, 1 };
        string[] labels = { "L", "C", "R" };
        Color[] colors = { Color.red, Color.green, Color.blue };

        for (int i = 0; i < lanes.Length; i++)
        {
            float x = LaneToX(lanes[i]);
            bool isActive = Application.isPlaying && lanes[i] == currentLane;

            Gizmos.color = isActive ? Color.yellow : colors[i];
            Gizmos.DrawLine(new Vector3(x, y, z), new Vector3(x, y + lineHeight, z));
            Gizmos.DrawWireSphere(new Vector3(x, y, z), 0.2f);

            UnityEditor.Handles.color = Gizmos.color;
            UnityEditor.Handles.Label(new Vector3(x, y + lineHeight + 0.3f, z), labels[i]);
        }

        // Draw target X marker at runtime
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(new Vector3(targetX, y + 0.5f, z), new Vector3(0.4f, 0.4f, 0.4f));
        }
    }
#endif
}
