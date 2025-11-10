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
    
    [Tooltip("Maximum distance from snap point to accept tile placement")]
    public float snapDistance = 1.0f;

    [HideInInspector] public bool occupied;

    public AudioClip correctSound;
    public AudioClip incorrectSound;

    // UnityEvent zodat je in inspector dingen kan koppelen (bv. geluid, animatie)
    public UnityEvent onCorrectPlacement;

    // Optioneel: verwijzing naar PuzzleManager (kan automatisch gevonden worden)
    public PuzzleManager puzzleManager;

    private PuzzleTile _currentTile; // Track the tile in this socket

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
        if (_currentTile != null) return; // Only one tile at a time

        PuzzleTile tile = other.GetComponent<PuzzleTile>();
        if (tile == null || tile.isPlaced) return;

        // Check if the tile is being held by the player
        PlayerPickup playerPickup = FindFirstObjectByType<PlayerPickup>();
        bool tileIsHeld = (playerPickup != null && playerPickup.GetHeldTile() == tile);

        // Only process the trigger if the tile is currently being held by the player.
        // This prevents stray collisions from forcing releases on already-placed tiles.
        if (!tileIsHeld) return;

        // Check if the tile ID matches the expected ID BEFORE doing anything else
        if (tile.tileId != expectedID)
        {
            Debug.Log($"Socket {gameObject.name}: Wrong tile (got {tile.tileId}, expected {expectedID}) - not accepting");
            return;
        }

        // Check if the tile is close enough to the snap point
        float distanceToSnap = Vector3.Distance(tile.transform.position, snapPoint.position);
        if (distanceToSnap > snapDistance)
        {
            Debug.Log($"Socket {gameObject.name}: Tile too far from snap point ({distanceToSnap:F2}m > {snapDistance}m)");
            return;
        }

        // At this point we know: playerPickup exists, tile is held, ID matches, and distance is good
        playerPickup.ForceReleaseTile();

        tile.SnapTo(snapPoint, tileHeightOffset);
        _currentTile = tile;

        // Mark as correctly placed
        occupied = true;
        tile.isPlaced = true;
        onCorrectPlacement?.Invoke();

        Debug.Log($"Socket {gameObject.name}: CORRECT tile {tile.tileId} placed! Notifying PuzzleManager.");
        
        if (puzzleManager != null)
            puzzleManager.NotifyPlaced();
        else
            Debug.LogWarning($"Socket {gameObject.name}: PuzzleManager is null!");

        if (correctSound != null)
            AudioSource.PlayClipAtPoint(correctSound, transform.position);
    }

    // Called when a tile is picked up from this socket
    public void OnTilePickedUp(PuzzleTile tile)
    {
        if (_currentTile == tile)
        {
            // If this was a correctly placed tile, notify the puzzle manager
            if (occupied && puzzleManager != null)
            {
                puzzleManager.NotifyRemoved();
            }
            
            _currentTile = null;
            occupied = false;
        }
    }
}