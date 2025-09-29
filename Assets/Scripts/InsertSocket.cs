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

    private PuzzleTile currentTile; // Track the tile in this socket

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
        if (currentTile != null) return; // Only one tile at a time

        PuzzleTile tile = other.GetComponent<PuzzleTile>();
        if (tile == null || tile.isPlaced) return;

        // Check if the tile is being held by the player
        PlayerPickup playerPickup = FindFirstObjectByType<PlayerPickup>();
        bool tileIsHeld = (playerPickup != null && playerPickup.GetHeldTile() == tile);

        if (tileIsHeld)
            playerPickup.ForceReleaseTile();

        tile.SnapTo(snapPoint, tileHeightOffset);
        currentTile = tile;

        if (tile.tileId == expectedID)
        {
            occupied = true;
            tile.isPlaced = true;
            onCorrectPlacement?.Invoke();

            if (puzzleManager != null)
                puzzleManager.NotifyPlaced();

            if (correctSound != null)
                AudioSource.PlayClipAtPoint(correctSound, transform.position);
        }
        else
        {
            occupied = false;
            tile.isPlaced = false;
            // Re-enable collider for incorrectly placed tiles so they can be picked up via raycast
            var col = tile.GetComponent<Collider>();
            if (col != null) col.enabled = true;
            if (incorrectSound != null)
                AudioSource.PlayClipAtPoint(incorrectSound, transform.position);
        }
    }

    // Called when a tile is picked up from this socket
    public void OnTilePickedUp(PuzzleTile tile)
    {
        if (currentTile == tile)
        {
            // If this was a correctly placed tile, notify the puzzle manager
            if (occupied && puzzleManager != null)
            {
                puzzleManager.NotifyRemoved();
            }
            
            currentTile = null;
            occupied = false;
        }
    }

    private void Update()
    {
        if (currentTile != null && !occupied)
        {
            // Allow picking up the tile again if not correct
            if (Input.GetKeyDown(KeyCode.E))
            {
                PlayerPickup playerPickup = FindFirstObjectByType<PlayerPickup>();
                if (playerPickup != null && playerPickup.GetHeldTile() == null)
                {
                    // Only allow picking up the tile if it is the currentTile
                    playerPickup.PickupTile(currentTile);
                    OnTilePickedUp(currentTile);
                    currentTile.transform.SetParent(null);
                    currentTile.isPlaced = false;
                    var rb = currentTile.GetComponent<Rigidbody>();
                    if (rb) rb.isKinematic = false;
                    var col = currentTile.GetComponent<Collider>();
                    if (col) col.enabled = true;
                }
            }
        }
    }
}