using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleManager : MonoBehaviour
{
    // Tile socket puzzle
    public int totalTileSockets;
    private int placedTileCount;

    // Laser puzzle
    public int totalLaserPuzzles;
    private int solvedLaserCount;
    private HashSet<GameObject> solvedLaserReceivers = new HashSet<GameObject>();

    public UnityEvent onPuzzleSolved;
    [Header("Door Reference")]
    public DoorScript doorScript;
    [Header("Door Lights")]
    public GameObject tilePuzzleLight;
    public GameObject laserPuzzleLight;
    private bool tileLightLit = false;
    private bool laserLightLit = false;
    private Color solvedColor = Color.magenta;

    [Header("Story Panels")]
    [Tooltip("Story panel to show when the game starts")]
    public GameObject startStoryPanel;
    [Tooltip("How long to display the start story panel (seconds)")]
    public float startStoryDuration = 5f;
    [Tooltip("If true, pauses the game while showing the start story panel")]
    public bool pauseOnStartStory = true;

    [Tooltip("Story panel to show when tile puzzle is completed")]
    public GameObject tilePuzzleStoryPanel;
    [Tooltip("How long to display the tile puzzle story panel (seconds)")]
    public float tilePuzzleStoryDuration = 4f;
    [Tooltip("If true, pauses the game while showing the tile puzzle story panel")]
    public bool pauseOnTilePuzzleStory = true;

    [Tooltip("Story panel to show when laser puzzle is completed")]
    public GameObject laserPuzzleStoryPanel;
    [Tooltip("How long to display the laser puzzle story panel (seconds)")]
    public float laserPuzzleStoryDuration = 4f;
    [Tooltip("If true, pauses the game while showing the laser puzzle story panel")]
    public bool pauseOnLaserPuzzleStory = true;

    private bool tilePuzzleStoryShown = false;
    private bool laserPuzzleStoryShown = false;

    void Awake()
    {
        // Hide all story panels at start
        if (startStoryPanel != null)
            startStoryPanel.SetActive(false);
        if (tilePuzzleStoryPanel != null)
            tilePuzzleStoryPanel.SetActive(false);
        if (laserPuzzleStoryPanel != null)
            laserPuzzleStoryPanel.SetActive(false);

        // Auto-count tile sockets if not set
        if (totalTileSockets == 0)
            totalTileSockets = FindObjectsByType<InsertSocket>(FindObjectsSortMode.None).Length;
        // Auto-find door if not assigned
        if (doorScript == null)
            doorScript = FindFirstObjectByType<DoorScript>();
    }

    void Start()
    {
        // Show start story panel if assigned
        if (startStoryPanel != null)
        {
            StartCoroutine(ShowStoryPanel(startStoryPanel, startStoryDuration, pauseOnStartStory));
        }
    }

    // Tile socket puzzle methods
    public void NotifyPlaced()
    {
        placedTileCount++;
        Debug.Log($"Tile placed: {placedTileCount}/{totalTileSockets}");
        if (!tileLightLit && placedTileCount >= totalTileSockets)
        {
            if (tilePuzzleLight != null)
            {
                var renderer = tilePuzzleLight.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.color = solvedColor;
            }
            tileLightLit = true;
            
            // Show tile puzzle story panel
            if (!tilePuzzleStoryShown && tilePuzzleStoryPanel != null)
            {
                tilePuzzleStoryShown = true;
                StartCoroutine(ShowStoryPanel(tilePuzzleStoryPanel, tilePuzzleStoryDuration, pauseOnTilePuzzleStory));
            }
        }
        CheckAllPuzzlesSolved();
    }

    public void NotifyRemoved()
    {
        placedTileCount--;
        Debug.Log($"Tile removed: {placedTileCount}/{totalTileSockets}");
        if (placedTileCount < 0) placedTileCount = 0;
    }

    // Laser puzzle methods
    public void NotifyLaserReceiverHit(GameObject receiver)
    {
        if (!solvedLaserReceivers.Contains(receiver))
        {
            solvedLaserReceivers.Add(receiver);
            solvedLaserCount++;
            Debug.Log($"Laser puzzle solved: {solvedLaserCount}/{totalLaserPuzzles}");
            if (!laserLightLit && solvedLaserCount >= totalLaserPuzzles)
            {
                if (laserPuzzleLight != null)
                {
                    var renderer = laserPuzzleLight.GetComponent<Renderer>();
                    if (renderer != null)
                        renderer.material.color = solvedColor;
                }
                laserLightLit = true;
                
                // Show laser puzzle story panel
                if (!laserPuzzleStoryShown && laserPuzzleStoryPanel != null)
                {
                    laserPuzzleStoryShown = true;
                    StartCoroutine(ShowStoryPanel(laserPuzzleStoryPanel, laserPuzzleStoryDuration, pauseOnLaserPuzzleStory));
                }
            }
            CheckAllPuzzlesSolved();
        }
    }

    // Check if both puzzles are solved
    private void CheckAllPuzzlesSolved()
    {
        if (placedTileCount >= totalTileSockets && solvedLaserCount >= totalLaserPuzzles)
        {
            Debug.Log("All puzzles solved!");
            if (doorScript != null)
                doorScript.OpenDoor();
            onPuzzleSolved?.Invoke();
        }
    }

    // Reset helpers
    public void ResetPuzzle()
    {
        placedTileCount = 0;
        tileLightLit = false;
        if (tilePuzzleLight != null)
        {
            var renderer = tilePuzzleLight.GetComponent<Renderer>();
        }
        foreach (var s in FindObjectsByType<InsertSocket>(FindObjectsSortMode.None))
        {
            s.occupied = false;
        }
        foreach (var t in FindObjectsByType<PuzzleTile>(FindObjectsSortMode.None))
        {
            t.isPlaced = false;
            t.transform.SetParent(null);
            var rb = t.GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = false;
            var col = t.GetComponent<Collider>();
            if (col) col.enabled = true;
        }
    }

    public void ResetLaserPuzzles()
    {
        solvedLaserCount = 0;
        solvedLaserReceivers.Clear();
        laserLightLit = false;
        if (laserPuzzleLight != null)
        {
            var renderer = laserPuzzleLight.GetComponent<Renderer>();
        }
    }

    IEnumerator ShowStoryPanel(GameObject panel, float duration, bool pauseGame)
    {
        Debug.Log($"PuzzleManager: Showing story panel '{panel.name}'");
        
        // Unlock and show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        float originalTimeScale = Time.timeScale;
        if (pauseGame)
            Time.timeScale = 0f;

        // Ensure proper Canvas rendering order if it has a Canvas component
        Canvas canvas = panel.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = 999;
        }

        panel.SetActive(true);

        yield return new WaitForSecondsRealtime(duration);

        panel.SetActive(false);
        Debug.Log($"PuzzleManager: Story panel '{panel.name}' hidden");

        // Restore time scale and cursor state
        if (pauseGame)
            Time.timeScale = originalTimeScale;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}