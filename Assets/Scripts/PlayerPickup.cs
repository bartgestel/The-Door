using TMPro;
using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    public Transform holdPoint;          // drag your HoldPoint here in inspector
    public float pickupRange = 3f;       // how far you can pick things up
    public float throwForce = 5f;        // optional if you want to throw
    private PuzzleTile heldTile;
    private Camera playerCamera;
    private MirrorScript mirrorScript;
    public Quaternion holdRotationOffset = Quaternion.Euler(0, 180, 0); // Adjust as needed
    public GameObject toolTipObject;
    public TextMeshProUGUI toolTipText;


    void Start()
    {
        // Find the camera - first try main camera, then find any camera
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindFirstObjectByType<Camera>();
        
        if (playerCamera == null)
            Debug.LogError("No camera found! PlayerPickup needs a camera to work.");

        if (toolTipObject != null)
        {
            toolTipText = toolTipObject.GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        // Show tooltip if looking at a pickupable tile
        if (playerCamera != null && heldTile == null)
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, pickupRange))
            {
                PuzzleTile tile = hit.collider.GetComponent<PuzzleTile>();
                MirrorScript mirror = hit.collider.GetComponent<MirrorScript>();
                if (tile != null && !tile.isPlaced && heldTile == null)
                {
                    ShowTooltip("Press E to pick up tile");
                }
                else if (mirror != null)
                {
                    ShowTooltip("Hold E to grab mirror");
                }else if(mirrorScript==null && tile==null)
                {
                    ShowTooltip("");
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.E)) // press E to pick up / drop
        {
            if (heldTile == null)
                TryPickup();
            else
                Drop();
        }

        if (Input.GetMouseButtonDown(1) && heldTile != null) // right click to throw
        {
            Throw();
        }

        // Keep held item positioned relative to camera
        UpdateHeldItemPosition();
    }

    void UpdateHeldItemPosition()
    {
        if (heldTile != null && holdPoint != null && playerCamera != null)
        {
            Vector3 targetPosition = playerCamera.transform.position +
                                     playerCamera.transform.forward * 1.5f +
                                     playerCamera.transform.right * 0.3f +
                                     playerCamera.transform.up * -0.3f;

            heldTile.transform.position = Vector3.Lerp(heldTile.transform.position, targetPosition, Time.deltaTime * 10f);
            heldTile.transform.rotation = Quaternion.Lerp(
                heldTile.transform.rotation,
                playerCamera.transform.rotation * holdRotationOffset,
                Time.deltaTime * 10f
            );
        }
    }
    
    void ShowTooltip(string message)
    {
        // Implement your tooltip display logic here
        toolTipText.text = message;
    }

    void TryPickup()
    {
        if (playerCamera == null) return;
        
        // Raycast from center of screen instead of mouse position for first-person gameplay
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hit;
        
        // Debug the raycast to see what we're hitting
        Debug.DrawRay(ray.origin, ray.direction * pickupRange, Color.red, 1f);
        
        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            PuzzleTile tile = hit.collider.GetComponent<PuzzleTile>();
            if (tile != null && !tile.isPlaced)
            {
                // Prevent picking up if tile is parented to a socket snapPoint (i.e., visually in a socket)
                if (tile.transform.parent != null && tile.transform.parent.GetComponent<InsertSocket>() != null)
                {
                    return;
                }
                // Notify all sockets to clear their reference to this tile
                foreach (var socket in FindObjectsByType<InsertSocket>(FindObjectsSortMode.None))
                {
                    socket.OnTilePickedUp(tile);
                }
                heldTile = tile;
                Rigidbody rb = heldTile.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    // Don't parent to holdPoint anymore - we'll control position manually
                    heldTile.transform.SetParent(null);
                }
                else
                {
                    Debug.LogError($"PuzzleTile {tile.name} is missing Rigidbody component!");
                }
            }
            else if (tile != null && tile.isPlaced)
            {
                Debug.Log("Cannot pick up tile - it's already placed");
            }
        }
        else
        {
            Debug.Log("No object hit within pickup range");
        }
    }

    void Drop()
    {
        if (heldTile != null)
        {
            Rigidbody rb = heldTile.GetComponent<Rigidbody>();
            
            // Re-enable the collider when dropping so it doesn't fall through the map
            Collider col = heldTile.GetComponent<Collider>();
            if (col != null) col.enabled = true;
            
            rb.isKinematic = false;
            heldTile = null;
        }
    }

    void Throw()
    {
        if (heldTile != null && playerCamera != null)
        {
            Rigidbody rb = heldTile.GetComponent<Rigidbody>();
            
            // Re-enable the collider when throwing so it doesn't fall through the map
            Collider col = heldTile.GetComponent<Collider>();
            if (col != null) col.enabled = true;
            
            rb.isKinematic = false;
            rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
            heldTile = null;
        }
    }
    
    // Public method to get the currently held tile (used by InsertSocket)
    public PuzzleTile GetHeldTile()
    {
        return heldTile;
    }
    
    // Method to force release the held tile (used by InsertSocket)
    public void ForceReleaseTile()
    {
        if (heldTile != null)
        {
            Rigidbody rb = heldTile.GetComponent<Rigidbody>();
            
            // Re-enable the collider
            Collider col = heldTile.GetComponent<Collider>();
            if (col != null) col.enabled = true;
            
            rb.isKinematic = false;
            heldTile = null;
        }
    }

    // Public method to pick up a specific tile (used by InsertSocket)
    public void PickupTile(PuzzleTile tile)
    {
        // Notify all sockets to clear their reference to this tile
        foreach (var socket in FindObjectsByType<InsertSocket>(FindObjectsSortMode.None))
        {
            socket.OnTilePickedUp(tile);
        }
        heldTile = tile;
        Rigidbody rb = heldTile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            heldTile.transform.SetParent(null);
        }
    }
}