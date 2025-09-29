using UnityEngine;
using UnityEngine.Events;

public class PuzzleManager : MonoBehaviour
{
    public int totalSockets;
    private int placedCount;

    public UnityEvent onPuzzleSolved; // koppel deur/animatie in inspector
    
    [Header("Door Reference")]
    public DoorScript doorScript; // Drag your door here

    void Awake()
    {
        if (totalSockets == 0)
            totalSockets = FindObjectsByType<InsertSocket>(FindObjectsSortMode.None).Length;
            
        // Auto-find door if not assigned
        if (doorScript == null)
            doorScript = FindFirstObjectByType<DoorScript>();
    }

    public void NotifyPlaced()
    {
        placedCount++;
        Debug.Log($"Tile placed: {placedCount}/{totalSockets}");
        if (placedCount >= totalSockets)
            Solve();
    }

    public void NotifyRemoved()
    {
        placedCount--;
        Debug.Log($"Tile removed: {placedCount}/{totalSockets}");
        if (placedCount < 0) placedCount = 0; // Safety check
    }

    void Solve()
    {
        Debug.Log("Puzzle solved!");
        
        // Open the door when puzzle is solved
        if (doorScript != null)
            doorScript.OpenDoor();
            
        onPuzzleSolved?.Invoke();
        // voeg hier extra logica toe (deur openen, reward spawn, etc.)
    }

    // Optionele reset helper voor testen
    public void ResetPuzzle()
    {
        placedCount = 0;
        foreach (var s in FindObjectsByType<InsertSocket>(FindObjectsSortMode.None))
        {
            s.occupied = false;
        }
        // Vereenvoudigde reset: zet alle tegels los en activeer physics (je wilt meestal spawn points onthouden)
        foreach (var t in FindObjectsByType<PuzzleTile>(FindObjectsSortMode.None))
        {
            // verplaats ze niet automatisch; je kunt hier spawnlocaties toepassen
            t.isPlaced = false;
            t.transform.SetParent(null);
            var rb = t.GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = false;
            var col = t.GetComponent<Collider>();
            if (col) col.enabled = true;
        }
    }
}