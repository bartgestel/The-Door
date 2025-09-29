using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    public Transform holdPoint;          // drag your HoldPoint here in inspector
    public float pickupRange = 3f;       // how far you can pick things up
    public float throwForce = 5f;        // optional if you want to throw
    private PuzzleTile heldTile;
    private Camera playerCamera;

    void Start()
    {
        // Find the camera - first try main camera, then find any camera
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindFirstObjectByType<Camera>();
        
        if (playerCamera == null)
            Debug.LogError("No camera found! PlayerPickup needs a camera to work.");
    }

    void Update()
    {
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
            // Position the held item relative to the camera's rotation
            Vector3 targetPosition = playerCamera.transform.position + 
                                   playerCamera.transform.forward * 1.5f + 
                                   playerCamera.transform.right * 0.3f + 
                                   playerCamera.transform.up * -0.3f;
            
            heldTile.transform.position = Vector3.Lerp(heldTile.transform.position, targetPosition, Time.deltaTime * 10f);
            heldTile.transform.rotation = Quaternion.Lerp(heldTile.transform.rotation, playerCamera.transform.rotation, Time.deltaTime * 10f);
        }
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
            Debug.Log($"Hit object: {hit.collider.name}");
            PuzzleTile tile = hit.collider.GetComponent<PuzzleTile>();
            if (tile != null && !tile.isPlaced)
            {
                Debug.Log($"Picking up tile with ID: {tile.tileId}");
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
            rb.isKinematic = false;
            heldTile = null;
        }
    }

    void Throw()
    {
        if (heldTile != null && playerCamera != null)
        {
            Rigidbody rb = heldTile.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
            heldTile = null;
        }
    }
}