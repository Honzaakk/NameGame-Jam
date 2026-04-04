using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerBoat : MonoBehaviour
{
    [Header("Lane Settings")]
    public float centreX = 0f;
    public float laneWidth = 3f;
    public float laneSnapSpeed = 10f;

    [Header("Jump Settings")]
    public float jumpForce = 8f;
    public float gravity = -20f;

    [Header("Clamp Settings")]
    public float minX = -6f;
    public float maxX = 6f;

    // -1 = Left, 0 = Centre, 1 = Right
    private int currentLane = 0;
    private float targetX;
    private float velocityY;
    private bool isGrounded;
    private float groundY;

    private Keyboard kb;

    void Start()
    {
        groundY = transform.position.y;
        targetX = LaneToX(currentLane);
        kb = Keyboard.current;
    }

    void Update()
    {
        kb = Keyboard.current;
        if (kb == null) return;

        HandleLaneInput();
        HandleJump();
        MovePlayer();
    }

    void HandleLaneInput()
    {
        if (kb.leftArrowKey.wasPressedThisFrame && currentLane > -1)
            currentLane--;

        if (kb.rightArrowKey.wasPressedThisFrame && currentLane < 1)
            currentLane++;

        targetX = Mathf.Clamp(LaneToX(currentLane), minX, maxX);
    }

    void HandleJump()
    {
        isGrounded = transform.position.y <= groundY + 0.01f;

        if (kb.spaceKey.wasPressedThisFrame && isGrounded)
            velocityY = jumpForce;

        velocityY += gravity * Time.deltaTime;
    }

    void MovePlayer()
    {
        float newX = Mathf.Lerp(transform.position.x, targetX, laneSnapSpeed * Time.deltaTime);
        float newY = Mathf.Max(transform.position.y + velocityY * Time.deltaTime, groundY);

        if (newY <= groundY) velocityY = 0f;

        transform.position = new Vector3(newX, newY, transform.position.z);
    }

    float LaneToX(int lane) => centreX + lane * laneWidth;

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        float y = Application.isPlaying ? groundY : transform.position.y;
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
