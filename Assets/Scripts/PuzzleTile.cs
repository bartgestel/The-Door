using UnityEngine;
using UnityEngine.EventSystems;

public class PuzzleTile : MonoBehaviour, IPointerEnterHandler
{
    public int tileId;

    [HideInInspector] public bool isPlaced;

    private Rigidbody _rb;
    private Collider _col;
    
    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
        
        if (_rb == null)
            Debug.LogError($"PuzzleTile {gameObject.name} is missing a Rigidbody component!");
        if (_col == null)
            Debug.LogError($"PuzzleTile {gameObject.name} is missing a Collider component!");
    }
    
    public void SnapTo(Transform snapPoint, float heightOffset = 0f)
    {
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;

        if (_col != null) _col.enabled = false;

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
        if (_rb != null) _rb.isKinematic = false;
        if (_col != null) _col.enabled = true;
    }
}
