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

    void Awake()
    {
        // Auto-count tile sockets if not set
        if (totalTileSockets == 0)
            totalTileSockets = FindObjectsByType<InsertSocket>(FindObjectsSortMode.None).Length;
        // Auto-find door if not assigned
        if (doorScript == null)
            doorScript = FindFirstObjectByType<DoorScript>();
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
}