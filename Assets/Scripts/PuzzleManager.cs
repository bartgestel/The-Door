using UnityEngine;
using UnityEngine.Events;

public class PuzzleManager : MonoBehaviour
{
    public int totalSockets;
    private int placedCount;

    public UnityEvent onPuzzleSolved; // koppel deur/animatie in inspector

    void Awake()
    {
        if (totalSockets == 0)
            totalSockets = FindObjectsByType<InsertSocket>(FindObjectsSortMode.None).Length;
    }

    public void NotifyPlaced()
    {
        placedCount++;
        Debug.Log($"Tile placed: {placedCount}/{totalSockets}");
        if (placedCount >= totalSockets)
            Solve();
    }

    void Solve()
    {
        Debug.Log("Puzzle solved!");
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