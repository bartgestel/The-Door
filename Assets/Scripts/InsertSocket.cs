using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class InsertSocket : MonoBehaviour
{
    [Tooltip("ID van tile die hier in moet")]
    public int expectedID;

    [Tooltip("Optionele snap-locatie (child transform). Als null wordt deze transform gebruikt.")]
    public Transform snapPoint;
    
    [Tooltip("Height offset above the socket where the tile should be positioned")]
    public float tileHeightOffset = 0.1f;

    [HideInInspector] public bool occupied = false;

    public AudioClip correctSound;
    public AudioClip incorrectSound;

    // UnityEvent zodat je in inspector dingen kan koppelen (bv. geluid, animatie)
    public UnityEvent onCorrectPlacement;

    // Optioneel: verwijzing naar PuzzleManager (kan automatisch gevonden worden)
    public PuzzleManager puzzleManager;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void Start()
    {
        if (puzzleManager == null)
            puzzleManager = FindFirstObjectByType<PuzzleManager>();
        if (snapPoint == null)
            snapPoint = transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (occupied) return;

        PuzzleTile tile = other.GetComponent<PuzzleTile>();
        if (tile == null || tile.isPlaced) return;

        if (tile.tileId == expectedID)
        {
            // correcte plaatsing
            tile.SnapTo(snapPoint, tileHeightOffset);
            occupied = true;
            onCorrectPlacement?.Invoke();

            if (puzzleManager != null)
                puzzleManager.NotifyPlaced();

            if (correctSound != null)
                AudioSource.PlayClipAtPoint(correctSound, transform.position);
        }
        else
        {
            // foutieve tegel
            if (incorrectSound != null)
                AudioSource.PlayClipAtPoint(incorrectSound, transform.position);
            // eventueel: duw tile iets terug, toon UI-feedback, etc.
        }
    }
}