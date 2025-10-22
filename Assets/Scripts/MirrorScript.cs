using UnityEngine;

public class MirrorScript : MonoBehaviour
{
    public float rotationSpeed = 200f;
    public Transform rotationPivot; // Optional: specify a different pivot point
    private bool isGrabbed = false;

    void Update()
    {
        if (!isGrabbed)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit) && hit.transform == transform)
                {
                    isGrabbed = true;
                }
            }
        }
        else
        {
            if (Input.GetKeyUp(KeyCode.E))
            {
                isGrabbed = false;
            }
            else
            {
                float mouseX = Input.GetAxis("Mouse X");
                if (Mathf.Abs(mouseX) > 0.01f)
                {
                    float rotationAmount = mouseX * rotationSpeed * Time.deltaTime;

                    if (rotationPivot != null)
                    {
                        // Calculate offset from pivot
                        Vector3 offset = transform.position - rotationPivot.position;

                        // Rotate the offset around the Y axis
                        Quaternion rotation = Quaternion.AngleAxis(rotationAmount, Vector3.up);
                        offset = rotation * offset;

                        // Apply the rotated offset back to position
                        transform.position = rotationPivot.position + offset;

                        // Rotate the object itself
                        transform.rotation = rotation * transform.rotation;
                    }
                    else
                    {
                        // No pivot specified, rotate in place
                        Quaternion rotation = Quaternion.AngleAxis(rotationAmount, Vector3.up);
                        transform.rotation = rotation * transform.rotation;
                    }
                }
            }
        }
    }
}