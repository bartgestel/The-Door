using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PuzzleTile : MonoBehaviour, IPointerEnterHandler
{
    public int tileId;

    [HideInInspector] public bool isPlaced;

    private Rigidbody rb;
    private Collider col;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        
        if (rb == null)
            Debug.LogError($"PuzzleTile {gameObject.name} is missing a Rigidbody component!");
        if (col == null)
            Debug.LogError($"PuzzleTile {gameObject.name} is missing a Collider component!");
    }
    
    public void SnapTo(Transform snapPoint, float heightOffset = 0f)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        if (col != null) col.enabled = false;

        // Position the tile above the snap point by the specified offset
        Vector3 snapPosition = snapPoint.position + Vector3.up * heightOffset;
        
        // Place precisely at snapPoint position (with height offset) and parent it
        transform.position = snapPosition;
        transform.rotation = snapPoint.rotation;
        transform.SetParent(snapPoint);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OnPointerEnter");
    }

    // Optioneel: helper om tile weer actief te maken bij reset
    public void ResetTile(Vector3 worldPosition)
    {
        isPlaced = false;
        transform.SetParent(null);
        transform.position = worldPosition;
        if (rb != null) rb.isKinematic = false;
        if (col != null) col.enabled = true;
    }
}
