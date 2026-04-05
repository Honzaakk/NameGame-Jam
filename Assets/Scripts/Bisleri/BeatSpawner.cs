using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// ─── Data Classes ───────────────────────────────────────────────────────────

[Serializable]
public class BeatEntry
{
    [Tooltip("Which beat number to spawn on (1-based). At 60 BPM, beat 1 = 0s, beat 2 = 1s, etc.")]
    public int beat = 1;

    [Tooltip("-1 = Left, 0 = Centre, 1 = Right")]
    [Range(-1, 1)]
    public int lane;

    [Tooltip("Override prefab for this entry (uses default if empty)")]
    public GameObject prefab;
}

[Serializable]
public class QTEEvent
{
    [Tooltip("Time in seconds when this QTE triggers (e.g. 15, 30, 45)")]
    public float triggerTime = 15f;

    [Tooltip("WASD pattern the player must press (e.g. wwad, wsws, dada)")]
    public string pattern = "wwad";

    [Tooltip("Seconds the player has to complete the pattern")]
    public float timeLimit = 3f;

    [Tooltip("Time.timeScale during QTE (lower = slower)")]
    [Range(0.05f, 1f)]
    public float slowMotionScale = 0.3f;

    [Tooltip("Override prefab spawned in all lanes during QTE (uses default if empty)")]
    public GameObject prefab;

    [HideInInspector] public bool triggered;
}

// ─── Main Spawner ───────────────────────────────────────────────────────────

public class BeatSpawner : MonoBehaviour
{
    [Header("Music")]
    public AudioSource musicSource;
    [Tooltip("Beats per minute of the chosen track")]
    public float bpm = 60f;
    [Tooltip("Total duration of the game round in seconds")]
    public float gameDuration = 60f;

    [Header("Lanes (must match PlayerControllerBoat)")]
    public float centreX = 60f;
    public float laneWidth = 12f;

    [Header("Spawn Settings")]
    [Tooltip("World-space Z offset ahead of this transform where obstacles appear")]
    public float spawnZOffset = 50f;
    public float spawnY = 0f;

    [Header("Default Obstacle")]
    public GameObject defaultObstaclePrefab;

    [Header("Beat Sheet — configure every spawn")]
    public List<BeatEntry> beatSheet = new List<BeatEntry>();

    [Header("QTE Events (every ~15 s)")]
    public List<QTEEvent> qteEvents = new List<QTEEvent>();

    // ── Events other scripts can subscribe to ──
    public event Action<QTEEvent> OnQTEStarted;
    public event Action<QTEEvent, bool> OnQTECompleted;       // (event, success)
    public event Action<int, int> OnQTEKeyPressed;             // (keysPressed, totalKeys)
    public event Action OnGameStarted;
    public event Action OnGameEnded;

    // ── Runtime state ──
    float songTime;
    float secondsPerBeat;
    HashSet<int> spawnedIndices = new HashSet<int>();
    bool gameRunning;

    // QTE
    QTEEvent activeQTE;
    int qtePatternIndex;
    float qteTimer;
    float savedTimeScale;
    Keyboard kb;

    // ── Public API ──
    public bool IsGameRunning => gameRunning;
    public bool IsQTEActive => activeQTE != null;
    public QTEEvent ActiveQTE => activeQTE;
    public float QTETimeRemaining => qteTimer;
    public int QTECurrentIndex => qtePatternIndex;
    public float SongTime => songTime;
    public int CurrentBeat => Mathf.FloorToInt(songTime / secondsPerBeat) + 1;
    public int TotalBeats => Mathf.CeilToInt(gameDuration / secondsPerBeat);

    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        secondsPerBeat = 60f / bpm;
        kb = Keyboard.current;

        foreach (var qte in qteEvents)
            qte.triggered = false;

        StartGame();
    }

    public void StartGame()
    {
        gameRunning = true;
        songTime = 0f;
        spawnedIndices.Clear();

        foreach (var qte in qteEvents)
            qte.triggered = false;

        if (musicSource != null)
            musicSource.Play();

        OnGameStarted?.Invoke();
    }

    void Update()
    {
        if (!gameRunning) return;

        kb = Keyboard.current;
        if (kb == null) return;

        // Prefer audio time for tight sync; fall back to unscaled delta
        if (musicSource != null && musicSource.isPlaying)
            songTime = musicSource.time;
        else
            songTime += Time.unscaledDeltaTime;

        if (songTime >= gameDuration)
        {
            EndGame();
            return;
        }

        if (activeQTE != null)
        {
            UpdateQTE();
            return;
        }

        CheckQTETriggers();
        ProcessBeatSheet();
    }

    // ─── Beat Sheet Spawning ─────────────────────────────────────────────────

    void ProcessBeatSheet()
    {
        int currentBeat = CurrentBeat;

        for (int i = 0; i < beatSheet.Count; i++)
        {
            if (spawnedIndices.Contains(i)) continue;

            BeatEntry entry = beatSheet[i];
            if (entry.beat <= currentBeat)
            {
                spawnedIndices.Add(i);
                SpawnObstacle(entry.lane, entry.prefab);
            }
        }
    }

    void SpawnObstacle(int lane, GameObject overridePrefab = null)
    {
        GameObject toSpawn = overridePrefab != null ? overridePrefab : defaultObstaclePrefab;
        if (toSpawn == null) return;

        float x = centreX + lane * laneWidth;
        Vector3 pos = new Vector3(x, spawnY, transform.position.z + spawnZOffset);
        Instantiate(toSpawn, pos, Quaternion.identity);
    }

    // ─── QTE System ──────────────────────────────────────────────────────────

    void CheckQTETriggers()
    {
        foreach (var qte in qteEvents)
        {
            if (!qte.triggered && songTime >= qte.triggerTime)
            {
                qte.triggered = true;
                BeginQTE(qte);
                return;
            }
        }
    }

    void BeginQTE(QTEEvent qte)
    {
        activeQTE = qte;
        qtePatternIndex = 0;
        qteTimer = qte.timeLimit;

        savedTimeScale = Time.timeScale;
        Time.timeScale = qte.slowMotionScale;

        // Spawn in all 3 lanes
        for (int lane = -1; lane <= 1; lane++)
            SpawnObstacle(lane, qte.prefab);

        OnQTEStarted?.Invoke(qte);
    }

    void UpdateQTE()
    {
        // Timer runs on unscaled time so slow-mo doesn't eat the deadline
        qteTimer -= Time.unscaledDeltaTime;

        if (qteTimer <= 0f)
        {
            FinishQTE(false);
            return;
        }

        char expected = char.ToLower(activeQTE.pattern[qtePatternIndex]);
        bool anyPressed = false;
        bool correct = false;

        if (kb.wKey.wasPressedThisFrame)      { anyPressed = true; correct = expected == 'w'; }
        else if (kb.aKey.wasPressedThisFrame)  { anyPressed = true; correct = expected == 'a'; }
        else if (kb.sKey.wasPressedThisFrame)  { anyPressed = true; correct = expected == 's'; }
        else if (kb.dKey.wasPressedThisFrame)  { anyPressed = true; correct = expected == 'd'; }

        if (!anyPressed) return;

        if (!correct)
        {
            FinishQTE(false);
            return;
        }

        qtePatternIndex++;
        OnQTEKeyPressed?.Invoke(qtePatternIndex, activeQTE.pattern.Length);

        if (qtePatternIndex >= activeQTE.pattern.Length)
            FinishQTE(true);
    }

    void FinishQTE(bool success)
    {
        Time.timeScale = savedTimeScale;
        QTEEvent finished = activeQTE;
        activeQTE = null;
        OnQTECompleted?.Invoke(finished, success);
    }

    // ─── End Game ────────────────────────────────────────────────────────────

    void EndGame()
    {
        gameRunning = false;
        Time.timeScale = 1f;

        if (musicSource != null)
            musicSource.Stop();

        OnGameEnded?.Invoke();
    }

    void OnDestroy()
    {
        // Safety: restore time scale if destroyed mid-QTE
        if (activeQTE != null)
            Time.timeScale = 1f;
    }

    // ─── Editor Gizmos ──────────────────────────────────────────────────────

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Draw lane spawn positions
        for (int lane = -1; lane <= 1; lane++)
        {
            float x = centreX + lane * laneWidth;
            Vector3 pos = new Vector3(x, spawnY, transform.position.z + spawnZOffset);

            Gizmos.color = lane == 0 ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(pos, new Vector3(laneWidth * 0.8f, 1f, 1f));
        }
    }
#endif
}
